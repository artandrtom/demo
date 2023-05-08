using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DefaultNamespace;
using DefaultNamespace.WebRequests;
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

        private readonly Dictionary<string, Func<HttpListenerRequest, HttpListenerResponse, Task<bool>>> _handlers;
        
        public WebhooksListener()
        {
            _handlers = new Dictionary<string, Func<HttpListenerRequest, HttpListenerResponse, Task<bool>>>
            {
                { "/banUser", HandleUserBanRequest },
                { "/getUserData", HandleGetUserDataRequest },
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

                    var handled = await HandleRequest(req, resp).ConfigureAwait(false);
                    if (!handled)
                    {
                        await FailResponse(resp).ConfigureAwait(false);
                    }
                    
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
        
        private async Task<bool> HandleRequest(HttpListenerRequest req, HttpListenerResponse res)
        {
            if (!_handlers.TryGetValue(req.Url.AbsolutePath, out var handler)) return false;
            var result = await handler(req, res).ConfigureAwait(false);
            return result;
        }

        public event Action<string> OnBanUser;
        
        private async Task<bool> HandleUserBanRequest(HttpListenerRequest req, HttpListenerResponse res)
        {
            if (!req.QueryString.HasKeys()) return false;
            var userId = req.QueryString.Get("id");
            if (string.IsNullOrEmpty(userId)) return false;
            Debug.Log($"Received webhook to kick banned user: {userId}");
            MainThreadDispatcher.Post(() => OnBanUser?.Invoke(userId));
            
            await OkResponse(res).ConfigureAwait(false);
            
            return true;
        }
        
        private async Task<bool> HandleGetUserDataRequest(HttpListenerRequest req, HttpListenerResponse res)
        {
            try
            {
                WebRequestExample.UserData data = new WebRequestExample.UserData
                {
                    Name = "Artur",
                    Email = "a.levchenko@visartech.com",
                    Role = "Unity dev"
                };
                var content = JsonConvert.SerializeObject(data);
                    
                var buffer = Encoding.UTF8.GetBytes(content);
                
                res.StatusCode = 200;
                res.ContentType = "application/json";
                res.ContentEncoding = Encoding.UTF8;
                res.ContentLength64 = buffer.LongLength;
                
                using (var stream = res.OutputStream)
                {
                    await stream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }

        private async Task OkResponse(HttpListenerResponse resp)
        {
            var data = Encoding.UTF8.GetBytes(SuccessResponse);
            
            resp.StatusCode = 200;
            resp.ContentType = "application/json";
            resp.ContentEncoding = Encoding.UTF8;
            resp.ContentLength64 = data.LongLength;
            
            using (var stream = resp.OutputStream)
            {
                await stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
            }
            
        }

        private async Task FailResponse(HttpListenerResponse resp)
        {
            var data = Encoding.UTF8.GetBytes(FailedResponse);
            
            resp.StatusCode = 421;
            resp.ContentType = "application/json";
            resp.ContentEncoding = Encoding.UTF8;
            resp.ContentLength64 = data.LongLength;
            
            using (var stream = resp.OutputStream)
            {
                await stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
            }
            
        }
    }
}