using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DefaultNamespace;
using Newtonsoft.Json;
using UnityEngine;

namespace Networking.Webhooks
{
    public sealed class WebhooksListener
    {
        private HttpListener _listener;
        private const string URL = "http://+:7778/";
        private bool _runListener;
        
        private const string SuccessResponse = "{\r\n  \"success\": true\r\n}";
        private const string FailedResponse = "{\r\n  \"success\": false\r\n}";

        private readonly Dictionary<string, Func<HttpListenerRequest, Task<bool>>> _handlers;
        
        public WebhooksListener()
        {
            _handlers = new Dictionary<string, Func<HttpListenerRequest, Task<bool>>>
            {
                { "/banUser", HandleUserBanRequest },
                { "/scheduleChanged", HandleScheduleChangedRequest },
                { "/resourcesChanged", HandleSharedResourcesChangedRequest },
                { "/chestGamesChanged", HandleChestGamesChangedRequest}
            };
        }
        
        public void Start()
        {
            _runListener = true;
            Application.quitting += ApplicationOnQuit;
            //run http server in separate thread
            Task.Factory.StartNew(HandleIncomingWebhooks, TaskCreationOptions.LongRunning).
                ConfigureAwait(false);
        }

        private void ApplicationOnQuit()
        {
            _runListener = false;
        }

        public void Stop()
        {
            Application.quitting -= ApplicationOnQuit;
        }
        
        private async Task HandleIncomingWebhooks()
        {
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add(URL);
                _listener.Start();
                Debug.Log($"Starting listening to webhooks on {URL}");
                while (_runListener)
                {
                    // Will wait here until we hear from a connection
                    var ctx = await _listener.GetContextAsync().ConfigureAwait(false);
                    // Peel out the requests and response objects
                    var req = ctx.Request;
                    var resp = ctx.Response;
                    
                    byte[] data;
                    bool success = await HandleRequest(req).ConfigureAwait(false);
                    if (success)
                    {
                        data = Encoding.UTF8.GetBytes(SuccessResponse);
                        resp.StatusCode = 200;
                    }
                    else
                    {
                        data = Encoding.UTF8.GetBytes(FailedResponse);
                        resp.StatusCode = 421;
                    }
                    
                    resp.ContentType = "application/json";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = data.LongLength;

                    // Write out to the response stream (asynchronously), then close it
                    await resp.OutputStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
                    resp.Close();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Got error running http listener: \n{e}");
            }
            finally
            {
                // Close the listener
                _listener.Close();
            }
        }
        
        private async Task<bool> HandleRequest(HttpListenerRequest req)
        {
            if (!_handlers.TryGetValue(req.Url.AbsolutePath, out var handler)) return false;
            var result = await handler(req).ConfigureAwait(false);
            return result;
        }

        private async Task<bool> HandleUserBanRequest(HttpListenerRequest req)
        {
            if (!req.QueryString.HasKeys()) return false;
            var userId = req.QueryString.Get("id");
            if (string.IsNullOrEmpty(userId)) return false;
            Debug.Log($"Received webhook to kick banned user: {userId}");
            await Task.CompletedTask;
            //MainThreadDispatcher.Post((action) => _signalBus.Fire(new UserBannedWebhookSignal { uid = userId }), this);
            return true;
        }
        
        private async Task<bool> HandleScheduleChangedRequest(HttpListenerRequest req)
        {
            Debug.Log($"Received schedule changed webhook");
            await Task.CompletedTask;
            //MainThreadDispatcher.Post((action) => _signalBus.Fire(new ScheduleChangedWebhookSignal()), this);
            return true;
        }
        
        private async Task<bool> HandleSharedResourcesChangedRequest(HttpListenerRequest req)
        {
            Debug.Log($"Received shared resources changed webhook");
            try
            {
                using (var inputStream = req.InputStream)
                {
                    byte[] buffer = new byte[req.ContentLength64];
                    var read = await inputStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                    if (read < 1)
                    {
                        return false;
                    }
                    string jsonString = Encoding.UTF8.GetString(buffer, 0, read);
                    //var signal = JsonConvert.DeserializeObject<SharedResourcesChangedWebhookSignal>(jsonString);
                    //MainThreadDispatcher.Post((action) => _signalBus.Fire(signal), this);
                }
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }
        
        private async Task<bool> HandleChestGamesChangedRequest(HttpListenerRequest req)
        {
            Debug.Log($"Received chest games changed webhook");
            await Task.CompletedTask;
            //MainThreadDispatcher.Post((action) => _signalBus.Fire(new ChestGamesChangedWebhookSignal()), this);
            return true;
        }
    }
}