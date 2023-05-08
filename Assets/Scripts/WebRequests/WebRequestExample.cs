using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MEC;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace DefaultNamespace.WebRequests
{
    public class WebRequestExample : MonoBehaviour
    {
        private static string Url = "https://google.com";
        
        public void GetUserData()
        {
            StartCoroutine(GetUserData(Url, OnGetUserDataSuccess, OnGetUserDataError));
        }
        
        private void OnGetUserDataSuccess(UserData data)
        {
            
        }

        private void OnGetUserDataError(Exception exception)
        {
            
        }

        private void OnDestroy()
        {
            StopCoroutine(nameof(GetUserData));
        }

        #region unity coroutine

        private IEnumerator GetUserData([NotNull]string url, Action<UserData> onSuccess, Action<Exception> onError)
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
        
        
        
        [Serializable]
        public struct UserData
        {
            public string Name;
            public string Email;
            public string Role;
        }
    }
}