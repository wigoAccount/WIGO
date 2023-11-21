using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace WIGO.Core
{
    public static class NetService
    {
        static HttpClient _client = new HttpClient();

        const string URL = "http://v2.cerebrohq.com/testapi/rpc.php";

        #region EVENTS
        public static async Task<IEnumerable<Event>> TryGetFeedEvents(FeedRequest data, string url, string stoken, CancellationToken ctoken = default)
        {
            string jsonData = JsonReader.Serialize(data).Replace(":[]", ":null");
            string correctData = jsonData.Replace(":0", ":null");
            RPCRequest request = new RPCRequest()
            {
                jsonrpc = "2.0",
                method = "eventList",
                @params = new List<string>() { correctData },
                id = "0"
            };

            string json = JsonReader.Serialize(request);
            var resJson = await PostRequest(json, url, ctoken, stoken);

            Debug.LogFormat("Answer: {0}", resJson);
            var res = GetParsedResult<List<Event>>(resJson, "Get Feed");
            return res?.result;
        }

        public static async Task<Event> TryGetMyEvent(string url, string stoken, CancellationToken ctoken = default)
        {
            RPCRequest request = new RPCRequest()
            {
                jsonrpc = "2.0",
                method = "eventMyListActive",
                @params = new List<string>(),
                id = "0"
            };

            string json = JsonReader.Serialize(request);
            var resJson = await PostRequest(json, url, ctoken, stoken);

            Debug.LogFormat("Answer: {0}", resJson);
            var res = GetParsedResult<List<Event>>(resJson, "Get my event");
            return (res != null && res.result.Count > 0) ? res.result[0] : null;
        }

        public static async Task<Event> TryCreateEvent(CreateEventRequest data, string url, string stoken, CancellationToken ctoken = default)
        {
            string jsonData = JsonReader.Serialize(data);
            RPCRequest request = new RPCRequest()
            {
                jsonrpc = "2.0",
                method = "eventPost",
                @params = new List<string> { jsonData },
                id = "0"
            };

            string json = JsonReader.Serialize(request);
            var resJson = await PostRequest(json, url, ctoken, stoken);

            Debug.LogFormat("Answer: {0}", resJson);
            var res = GetParsedResult<List<Event>>(resJson, "Create event");
            return (res != null && res.result.Count > 0) ? res.result[0] : null;
        }

        public static async Task TryRemoveEvent(string eventId, string url, bool isRequest, string stoken, CancellationToken ctoken = default)
        {
            string methodName = isRequest ? "requestCancel" : "eventCancel";
            RPCRequest request = new RPCRequest()
            {
                jsonrpc = "2.0",
                method = methodName,
                @params = new List<string> { eventId },
                id = "0"
            };

            string json = JsonReader.Serialize(request);
            var resJson = await PostRequest(json, url, ctoken, stoken);

            Debug.LogFormat("Answer: {0}", resJson);
            var res = GetParsedResult<List<AbstractEvent>>(resJson, methodName);
            if (res.result.Exists(x => string.Compare(x.uid, eventId) == 0))
            {
                Debug.LogErrorFormat("Fail to remove event/request: {0}", eventId);
            }
        }

        public static async Task<bool> TrySendComplaint(CreateComplaintRequest data, string url, string stoken, CancellationToken ctoken = default)
        {
            string jsonData = JsonReader.Serialize(data);
            RPCRequest request = new RPCRequest()
            {
                jsonrpc = "2.0",
                method = "complaintPost",
                @params = new List<string> { jsonData },
                id = "0"
            };

            string json = JsonReader.Serialize(request);
            var resJson = await PostRequest(json, url, ctoken, stoken);

            Debug.LogFormat("Answer: {0}", resJson);
            if (string.IsNullOrEmpty(resJson))
            {
                Debug.LogError("Send complaint request is empty");
                return false;
            }

            if (resJson.Contains("error"))
            {
                try
                {
                    RPCError error = JsonReader.Deserialize<RPCError>(resJson);
                    if (error != null)
                    {
                        ReportError(error.error);
                    }

                    return false;
                }
                catch (System.Exception) { }
            }

            return true;
        }

        public static async Task<string> TrySendDeclineEvent(string eventId, string url, string stoken, CancellationToken ctoken = default)
        {
            RPCRequest request = new RPCRequest()
            {
                jsonrpc = "2.0",
                method = "eventDecline",
                @params = new List<string> { eventId },
                id = "0"
            };

            string json = JsonReader.Serialize(request);
            var resJson = await PostRequest(json, url, ctoken, stoken);

            Debug.LogFormat("Answer: {0}", resJson);
            var res = GetParsedResult<List<string>>(resJson, "Decline event");
            return (res != null && res.result.Count > 0) ? res.result[0] : null;
        }
        #endregion

        #region USER
        public static async Task<ConfirmRegisterResult> TryRegisterNewAccount(string phoneNumber, string appleId, CancellationToken token = default)
        {
            var uid = SystemInfo.deviceUniqueIdentifier;
            var language = Application.systemLanguage.ToString();
            string slang = language.Substring(0, 2).ToLower();
            RPCRequest request = new RPCRequest()
            {
                jsonrpc = "2.0",
                method = "register",
                @params = new List<string> { slang, uid, appleId, phoneNumber },
                id = "0"
            };
            string json = JsonReader.Serialize(request);
            var resJson = await PostRequest(json, token);

            Debug.LogFormat("Answer: {0}", resJson);
            var res = GetParsedResult<ConfirmRegisterResult>(resJson, "Register");
            return res != null ? res.result : new ConfirmRegisterResult();
        }

        public static async Task<ConfirmRegisterResult> TryConfirmRegister(string token, string code, CancellationToken ctoken = default)
        {
            var uid = SystemInfo.deviceUniqueIdentifier;
            RPCRequest request = new RPCRequest()
            {
                jsonrpc = "2.0",
                method = "registerConfirm",
                @params = new List<string> { token, uid, code },
                id = "0"
            };

            string json = JsonReader.Serialize(request);
            var resJson = await PostRequest(json, ctoken);

            Debug.LogFormat("Answer: {0}", resJson);
            var res = GetParsedResult<ConfirmRegisterResult>(resJson, "Confirm register");
            return res != null ? res.result : new ConfirmRegisterResult();
        }

        public static async Task<bool> CheckBirthdayInvalid(string birthday, string stoken, CancellationToken ctoken = default)
        {
            RPCRequest request = new RPCRequest()
            {
                jsonrpc = "2.0",
                method = "userBirthdaysInvalid",
                @params = new List<string> { birthday },
                id = "0"
            };

            string json = JsonReader.Serialize(request);
            var resJson = await PostRequest(json, ctoken, stoken);

            Debug.LogFormat("Answer: {0}", resJson);
            var res = GetParsedResult<List<CheckBirthdayResult>>(resJson, "Check birthday invalid");
            return (res != null && res.result.Count > 0) ? res.result[0].invalid : true;
        }

        public static async Task<ConfirmRegisterResult> TryLogin(string ltoken, CancellationToken token = default)
        {
            var uid = SystemInfo.deviceUniqueIdentifier;
            RPCRequest request = new RPCRequest()
            {
                jsonrpc = "2.0",
                method = "start",
                @params = new List<string> { ltoken, uid },
                id = "0"
            };

            string json = JsonReader.Serialize(request);
            var resJson = await PostRequest(json, token);

            Debug.LogFormat("Answer: {0}", resJson);
            var res = GetParsedResult<ConfirmRegisterResult>(resJson, "Log in");
            return res != null ? res.result : new ConfirmRegisterResult();
        }

        public static async Task TryDeleteAccount(string stoken, CancellationToken token = default)
        {
            RPCRequest request = new RPCRequest()
            {
                jsonrpc = "2.0",
                method = "userRemove",
                @params = new List<string>(),
                id = "0"
            };

            string json = JsonReader.Serialize(request);
            var resJson = await PostRequest(json, token, stoken);

            Debug.LogFormat("Answer: {0}", resJson);
            if (string.IsNullOrEmpty(resJson))
            {
                Debug.LogError("User remove request is empty");
            }

            if (resJson.Contains("error"))
            {
                try
                {
                    RPCError error = JsonReader.Deserialize<RPCError>(resJson);
                    if (error != null)
                    {
                        ReportError(error.error);
                    }
                }
                catch { }

                return;
            }

            Debug.Log("User successfully deleted");
        }

        public static async Task<ProfileData> TryUpdateUser(string userUpdJson, string stoken, CancellationToken token = default)
        {
            RPCRequest request = new RPCRequest()
            {
                jsonrpc = "2.0",
                method = "userSet",
                @params = new List<string> { userUpdJson },
                id = "0"
            };

            string json = JsonReader.Serialize(request);
            var resJson = await PostRequest(json, token, stoken);

            Debug.LogFormat("Answer: {0}", resJson);
            var res = GetParsedResult<List<ProfileContainer>>(resJson, "Update user");
            return (res != null && res.result.Count > 0) ? res.result[0].profile : null;
        }

        public static async Task<bool> TrySendLocation(Location location, string url, string stoken, CancellationToken ctoken = default)
        {
            string jsonData = JsonReader.Serialize(location);
            RPCRequest request = new RPCRequest()
            {
                jsonrpc = "2.0",
                method = "locationSet",
                @params = new List<string> { jsonData },
                id = "0"
            };

            string json = JsonReader.Serialize(request);
            var resJson = await PostRequest(json, url, ctoken, stoken);

            Debug.LogFormat("Answer: {0}", resJson);
            if (string.IsNullOrEmpty(resJson))
            {
                Debug.LogError("Send location request is empty");
                return false;
            }

            if (resJson.Contains("error"))
            {
                try
                {
                    RPCError error = JsonReader.Deserialize<RPCError>(resJson);
                    if (error != null)
                    {
                        ReportError(error.error);
                    }
                }
                catch { }

                return false;
            }

            Debug.Log("Location sent");
            return true;
        }

        public static async Task<GlobalsData> RequestGlobal(string url, string stoken, CancellationToken ctoken = default)
        {
            RPCRequest request = new RPCRequest()
            {
                jsonrpc = "2.0",
                method = "globals",
                @params = new List<string>(),
                id = "0"
            };

            string json = JsonReader.Serialize(request);
            var resJson = await PostRequest(json, url, ctoken, stoken);
            Debug.LogFormat("<color=cyan>GLOBALS: {0}</color>", resJson);

            var res = GetParsedResult<List<GlobalsData>>(resJson, "Get globals");
            return (res != null && res.result.Count > 0) ? res.result[0] : null;
        }
        #endregion

        #region REQUESTS
        public static async Task<IEnumerable<Request>> TryGetMyRequests(string url, string stoken, CancellationToken ctoken = default)
        {
            RPCRequest request = new RPCRequest()
            {
                jsonrpc = "2.0",
                method = "requestMyListActive",
                @params = new List<string>(),
                id = "0"
            };

            string json = JsonReader.Serialize(request);
            var resJson = await PostRequest(json, url, ctoken, stoken);

            Debug.LogFormat("Answer: {0}", resJson);
            var res = GetParsedResult<List<Request>>(resJson, "Get my requests");
            return res?.result;
        }

        public static async Task<Request> TryCreateRequest(CreateResponseRequest data, string url, string stoken, CancellationToken ctoken = default)
        {
            string jsonData = JsonReader.Serialize(data);
            RPCRequest request = new RPCRequest()
            {
                jsonrpc = "2.0",
                method = "requestPost",
                @params = new List<string> { jsonData },
                id = "0"
            };

            string json = JsonReader.Serialize(request);
            var resJson = await PostRequest(json, url, ctoken, stoken);

            Debug.LogFormat("Answer: {0}", resJson);
            var res = GetParsedResult<List<Request>>(resJson, "Create request");
            return (res != null && res.result.Count > 0) ? res.result[0] : null;
        }

        public static async Task<Event> TryAcceptOrDeclineRequest(string requestId, string url, bool accept, string stoken, CancellationToken ctoken = default)
        {
            string methodName = accept ? "requestAccept" : "requestDecline";
            RPCRequest request = new RPCRequest()
            {
                jsonrpc = "2.0",
                method = methodName,
                @params = new List<string> { requestId },
                id = "0"
            };

            string json = JsonReader.Serialize(request);
            var resJson = await PostRequest(json, url, ctoken, stoken);

            Debug.LogFormat("Answer: {0}", resJson);
            var res = GetParsedResult<List<Event>>(resJson, methodName);
            return (res != null && res.result.Count > 0) ? res.result[0] : null;
        }

        public static async Task TryMarkRequestAsWatched(string requestId, string url, string stoken, CancellationToken ctoken = default)
        {
            RPCRequest request = new RPCRequest()
            {
                jsonrpc = "2.0",
                method = "requestWatch",
                @params = new List<string> { requestId },
                id = "0"
            };

            string json = JsonReader.Serialize(request);
            var resJson = await PostRequest(json, url, ctoken, stoken);

            Debug.LogFormat("Answer: {0}", resJson);
            if (string.IsNullOrEmpty(resJson))
            {
                Debug.LogError("Mark as watched request is empty");
                return;
            }

            if (resJson.Contains("error"))
            {
                try
                {
                    RPCError error = JsonReader.Deserialize<RPCError>(resJson);
                    if (error != null)
                    {
                        ReportError(error.error);
                    }
                }
                catch { }

                return;
            }

            Debug.LogFormat("Request '{0}' marked as watched", requestId);
        }
        #endregion

        static async Task<string> PostRequest(string request, CancellationToken token, string addHeader = null)
        {
            return await PostRequest(request, URL, token, addHeader);
        }

        static async Task<string> PostRequest(string request, string url, CancellationToken token, string addHeader = null)
        {
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new System.Uri(url),
                Headers = {
                    { HttpRequestHeader.ContentType.ToString(), "application/json" }
                },
                Content = new StringContent(request)
            };

            Debug.LogFormat("Request: {0}", request);
            if (!string.IsNullOrEmpty(addHeader))
            {
                httpRequestMessage.Headers.Add("rpcauth", addHeader);
            }

            try
            {
                var response = await _client.SendAsync(httpRequestMessage, token);
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    return res;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
            }
            
            return string.Empty;
        }

        #region HELPERS
        static RPCResult<T> GetParsedResult<T>(string resJson, string debugName)
        {
            if (string.IsNullOrEmpty(resJson))
            {
                Debug.LogErrorFormat("'{0}' request is empty", debugName);
                return null;
            }

            if (!resJson.Contains("error"))
            {
                try
                {
                    RPCResult<T> res = JsonReader.Deserialize<RPCResult<T>>(resJson);
                    return res;
                }
                catch (System.Exception ex)
                {
                    Debug.LogErrorFormat("Error parse {0}: {1}", debugName, ex.Message);
                    return null;
                }
            }

            try
            {
                RPCError error = JsonReader.Deserialize<RPCError>(resJson);
                if (error != null)
                {
                    ReportError(error.error);
                }

                return null;
            }
            catch (System.Exception e)
            {
                Debug.LogErrorFormat("Error {0}: {1}", debugName, e.Message);
                return null;
            }
        }

        static void ReportError(ErrorResult error)
        {
            string message = error.message;
            int startIndex = message.IndexOf("ERROR:") + 8;
            int endIndex = message.IndexOf("\n");
            int.TryParse(message.Substring(startIndex, endIndex - startIndex), out int errorId);
            Debug.LogFormat("Received error with id: {0}", errorId);
            ServiceLocator.Get<Userinterface.UIManager>().GetPopupManager().AddErrorNotification(errorId);
        }

        public static async Task<Texture2D> GetRemoteTexture(string url, CancellationToken token)
        {
            var response = await _client.GetAsync(url, token);
            if (response.IsSuccessStatusCode)
            {
                var res = await response.Content.ReadAsByteArrayAsync();
                if (res == null || res.Length == 0)
                {
                    return null;
                }

                var texture = TextureCreator.GetCompressedTexture();
                texture.LoadImage(res);
                texture.Apply();

                return texture;
            }

            return null;
        }
        #endregion
    }
}
