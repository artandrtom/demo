using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using RSG;
using UnityEngine;

namespace DefaultNamespace
{
    public class FileUploader
    {
        private readonly Dictionary<string, float> _loadingInfos = new();
        public event Action<string, float> UploadProgressChanged;
        public event Action<string, bool> UploadComplete;
        
        public IPromise UploadMediaContent(string filePath, string url, string contentType, string uploadId)
        {
            var promise = new Promise();
            UploadMediaContentAsync(filePath, url, contentType, uploadId, promise);
            return promise;
        }

        private async void UploadMediaContentAsync(string filePath, string url, string contentType, string uploadId, IPendingPromise promise)
        {
            _loadingInfos[uploadId] = 0f;
            var result = await UploadMediaContentTask(filePath, url, contentType, uploadId);
            if (result.IsSuccess)
            {
                promise.Resolve();
            }
            else
            {
                promise.Reject(result.Exception);
            }

            _loadingInfos.Remove(uploadId);
            UploadComplete?.Invoke(uploadId, result.IsSuccess);
        }
        
        private async Task<Result> UploadMediaContentTask(string filePath, string url, string contentType, string uploadId)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(204800);
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = WebRequestMethods.Http.Put;
                request.Timeout = 3600000; // Set timeout to 1 hour
                request.ContentType = contentType;
                request.AllowWriteStreamBuffering = false;
                // Open the file stream
                using var fileStream = File.OpenRead(filePath);
                // Set the content length to the size of the file
                request.ContentLength = fileStream.Length;
                // Get the request stream
                using var requestStream = await request.GetRequestStreamAsync().ConfigureAwait(false);
                int bytesRead;
                long bytesSent = 0;

                // Upload the file in chunks
                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
                {
                    await requestStream.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
                    bytesSent += bytesRead;
                    float progress = (float)bytesSent / fileStream.Length;
                    MainThreadDispatcher.Post(() => UploadProgressChanged?.Invoke(uploadId, progress));
                }
                await requestStream.FlushAsync().ConfigureAwait(false);

                // Get the response from the server
                using var response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Debug.Log("File uploaded successfully.");
                    return Result.Success();
                }
                else
                {
                    return Result.FromError(new Exception($"Got response with status code {response.StatusCode}, description: {response.StatusDescription}"));
                }
            }
            catch (Exception e)
            {
                return Result.FromError(e);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}