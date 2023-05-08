using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MEC;
using Newtonsoft.Json;
using Polly;
using Polly.Contrib.WaitAndRetry;
using RSG;
using UnityEngine;
using UnityEngine.Networking;

namespace DefaultNamespace.WebRequests
{
    public class WebRequestExample : MonoBehaviour
    {
        private static string Url = "https://google.com";
        
        public void GetUserData()
        {
            StartCoroutine(GetUserDataRoutine(Url, OnGetUserDataSuccess, OnGetUserDataError));
        }

        #region get async

        public async void GetUserDataAsync()
        {
            var result = await GetUserDataTask(Url);
            if (result.IsSuccess)
            {
                
            }
            
        }

        public async void GetUserDataHttp()
        {
            var result = await GetUserDataTaskHttp<UserData>(Url);
            if (result.IsSuccess)
            {
                
            }
        }
        public async void GetUserDataHttpWithRetry()
        {
            var result = await GetUserDataTaskHttpWithRetry<UserData>(Url);
            if (result.IsSuccess)
            {
                
            }
        }

        
        #endregion

        #region get with promise

        public void GetUserDataWithPromise()
        {
            GetUserDataPromise().Done(OnGetUserDataSuccess, OnGetUserDataError);
        }

        public IPromise<UserData> GetUserDataPromise()
        {   
            var promise = new Promise<UserData>();
            ExecuteRequestWithPromise(Url, HttpMethod.Get, promise);
            return promise;
        }
        
        #endregion
        
        private void OnGetUserDataSuccess(UserData data)
        {
            Debug.Log("Get user data success");
        }

        private void OnGetUserDataError(Exception exception)
        {
            Debug.LogWarning(exception);
        }

        private void OnDestroy()
        {
            StopCoroutine(nameof(GetUserDataRoutine));
        }

        #region unity coroutine

        private IEnumerator GetUserDataRoutine([NotNull]string url, Action<UserData> onSuccess, Action<Exception> onError)
        {
            using (var request = UnityWebRequest.Get(url))
            {
                request.timeout = 20;
                yield return request.SendWebRequest();

                if (!string.IsNullOrEmpty(request.error))
                {
                    onError?.Invoke(new Exception(request.error));
                    yield break;
                }
                if(string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    onError?.Invoke(new Exception("Empty response"));
                    yield break;
                }

                UserData userData;
                try
                {
                    userData = JsonConvert.DeserializeObject<UserData>(request.downloadHandler.text);
                }
                catch (Exception e)
                {
                    onError?.Invoke(e);
                    yield break;
                }
                
                onSuccess?.Invoke(userData);
            }
        }

        #endregion

        #region unity MEC coroutine
        
        private IEnumerator<float> GetUserDataMEC([NotNull]string url, Action<UserData> onSuccess, Action<Exception> onError)
        {
            using (var request = UnityWebRequest.Get(url))
            {
                request.timeout = 20;
                yield return Timing.WaitUntilDone(request.SendWebRequest());

                if (!string.IsNullOrEmpty(request.error))
                {
                    onError?.Invoke(new Exception(request.error));
                    yield break;
                }
                if(string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    onError?.Invoke(new Exception("Empty response"));
                    yield break;
                }

                UserData userData;
                try
                {
                    userData = JsonConvert.DeserializeObject<UserData>(request.downloadHandler.text);
                }
                catch (Exception e)
                {
                    onError?.Invoke(e);
                    yield break;
                }
                
                onSuccess?.Invoke(userData);
            }
        }
        
        private IEnumerator<float> WaitForAll()
        {
            var t = GetUserDataMEC(Url, OnGetUserDataSuccess, OnGetUserDataError).CancelWith(gameObject).RunCoroutine();
            var t1 = GetUserDataMEC(Url, OnGetUserDataSuccess, OnGetUserDataError).CancelWith(gameObject).RunCoroutine();
            yield return new[] { t, t1 }.WaitWhenAll();
        }
        
        #endregion

        #region unity async task

        private async Task<Result<UserData>> GetUserDataTask([NotNull]string url)
        {
            using (var request = UnityWebRequest.Get(url))
            {
                request.timeout = 20;
                request.SendWebRequest();

                while (!request.isDone)
                {
                    await Task.Yield();
                }
                
                if (!string.IsNullOrEmpty(request.error))
                {
                    return new Exception(request.error);
                }
                if(string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    return new Exception("Empty response");
                }

                UserData userData;
                try
                {
                    userData = JsonConvert.DeserializeObject<UserData>(request.downloadHandler.text);
                }
                catch (Exception e)
                {
                    return e;

                }
                return userData;
            }
        }

        #endregion

        #region request with http client

        private readonly HttpClient _httpClient = new HttpClient();
        private async Task<Result<T>> GetUserDataTaskHttp<T>([NotNull]string url) where T : struct
        {
            Result<T> result;
            try
            {
                result = await ExecuteRequestInternal<T>(url, HttpMethod.Get).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                result = e;
            }
            
            return result;
        }
        
        private async Task<Result<T>> ExecuteRequestInternal<T>([NotNull]string url, HttpMethod httpMethod) where T : struct
        {
            using var request = new HttpRequestMessage(httpMethod, url);

            using var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return TryCreateFromResponse<T>(content);
            }
            
            var message = $"{response.StatusCode} : {response.StatusCode.ToString()}";
            return response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden
                ? new Exception("Unauthorized")
                : new Exception(message);
        }
        
        //cached deserializer, so it will not be created during every deserialization call
        private static readonly JsonSerializer Deserializer = JsonSerializer.CreateDefault(new 
            JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                MissingMemberHandling = MissingMemberHandling.Ignore
            });
    
        //cached serializer settings
        private static readonly JsonSerializerSettings  SerializeSettings = new 
            JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
    
        private Result<T> TryCreateFromResponse<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return new Exception($"Can not parse empty json string into target type: {typeof(T)}!");
            }
            try
            {
                using var reader = new JsonTextReader(new StringReader(json));
                return new Result<T>((T)Deserializer.Deserialize(reader, typeof(T)));
            }
            catch (Exception e)
            {
                return new Exception($"Error trying to parse {typeof(T)}! \n{e}");
            }
        }
        
        #endregion

        #region request with http client with retry
        
        private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<OperationCanceledException>()
            .OrResult(x =>
                x.StatusCode is >= HttpStatusCode.InternalServerError or HttpStatusCode.RequestTimeout)
            .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1f), 3));
        
        private async Task<Result<T>> GetUserDataTaskHttpWithRetry<T>([NotNull]string url) where T : struct
        {
            Result<T> result;
            try
            {
                result = await ExecuteRequestInternalWithRetry<T>(url, HttpMethod.Get).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                result = e;
            }
            
            return result;
        }
        
        private async Task<Result<T>> ExecuteRequestInternalWithRetry<T>([NotNull]string url, HttpMethod httpMethod) where T : struct
        {
            using var response = await _retryPolicy.ExecuteAsync(()=> _httpClient.SendAsync(new HttpRequestMessage(httpMethod, url)));

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return TryCreateFromResponse<T>(content);
            }
            
            var message = $"{response.StatusCode} : {response.StatusCode.ToString()}";
            return response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden
                ? new Exception("Unauthorized")
                : new Exception(message);
        }

        #endregion

        #region request with promise

        private async void ExecuteRequestWithPromise<T>([NotNull] string url, HttpMethod httpMethod, IPendingPromise<T> promise) where T : struct
        {
            var result = await ExecuteRequestTaskWithPromise<T>(url, httpMethod);
            if (result.IsSuccess)
            {
                promise.Resolve(result.Value);
                return;
            }
            promise.Reject(result.Exception); 
        }

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(25, 25);
        private async Task<Result<T>> ExecuteRequestTaskWithPromise<T>(string url, HttpMethod httpMethod) where T : struct
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            Result<T> result;
            try
            {
                result = await ExecuteRequestInternalWithRetry<T>(url, httpMethod).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                result = e;
            }
            finally
            {
                _semaphore.Release();
            }

            return result;
        }
        
        #endregion
        
        [Serializable]
        public struct UserData
        {
            public string Name;
            public string Email;
            public string Role;
        }
    }
}