using Amazon.Runtime;
using Amazon.S3;
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

            if (string.IsNullOrEmpty(resJson))
            {
                Debug.LogError("Globals request is empty");
                return null;
            }

            try
            {
                RPCResult<List<GlobalsData>> res = JsonReader.Deserialize<RPCResult<List<GlobalsData>>>(resJson);
                return res.result[0];
            }
            catch
            {
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
                    Debug.LogErrorFormat("Error create event: {0}", e.Message);
                    return null;
                }
            }
        }

        #region EVENTS
        public static async Task<IEnumerable<Event>> TryGetFeedEvents(FeedRequest data, string url, string stoken, CancellationToken ctoken = default)
        {
            string jsonData = JsonReader.Serialize(data);
            RPCRequest request = new RPCRequest()
            {
                jsonrpc = "2.0",
                method = "eventList",
                @params = new List<string>() { jsonData },
                id = "0"
            };

            string json = JsonReader.Serialize(request);
            var resJson = await PostRequest(json, url, ctoken, stoken);

            Debug.LogFormat("Answer: {0}", resJson);
            if (string.IsNullOrEmpty(resJson))
            {
                Debug.LogError("Feed request result is empty");
                return null;
            }

            try
            {
                RPCResult<List<Event>> res = JsonReader.Deserialize<RPCResult<List<Event>>>(resJson);
                return res?.result;
            }
            catch
            {
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
                    Debug.LogErrorFormat("Error get feed: {0}", e.Message);
                    return null;
                }
            }
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
            return GetParsedEvent(resJson);
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
            return GetParsedEvent(resJson);
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
            if (string.IsNullOrEmpty(resJson))
            {
                Debug.LogError("Request result is empty");
                return null;
            }

            try
            {
                RPCResult<List<Request>> res = JsonReader.Deserialize<RPCResult<List<Request>>>(resJson);
                return (res != null && res.result.Count > 0) ? res.result[0] : null;
            }
            catch
            {
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
                    Debug.LogErrorFormat("Error create event: {0}", e.Message);
                    return null;
                }
            }
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
            if (string.IsNullOrEmpty(resJson))
            {
                Debug.LogError("Create event request is empty");
                return;
            }

            try
            {
                RPCResult<List<AbstractEvent>> res = JsonReader.Deserialize<RPCResult<List<AbstractEvent>>>(resJson);
                if (res.result.Exists(x => string.Compare(x.uid, eventId) == 0))
                {
                    Debug.LogErrorFormat("Fail to remove event: {0}", eventId);
                }
            }
            catch
            {
                try
                {
                    RPCError error = JsonReader.Deserialize<RPCError>(resJson);
                    if (error != null)
                    {
                        ReportError(error.error);
                    }

                    return;
                }
                catch (System.Exception e)
                {
                    Debug.LogErrorFormat("Error create event: {0}", e.Message);
                    return;
                }
            }
        }

        public static async Task TryAcceptOrDeclineRequest(string requestId, string url, bool accept, string stoken, CancellationToken ctoken = default)
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
            if (string.IsNullOrEmpty(resJson))
            {
                Debug.LogError("Accept or decline request is empty");
                return;
            }

            try
            {
                RPCResult<List<AbstractEvent>> res = JsonReader.Deserialize<RPCResult<List<AbstractEvent>>>(resJson);
            }
            catch
            {
                try
                {
                    RPCError error = JsonReader.Deserialize<RPCError>(resJson);
                    if (error != null)
                    {
                        ReportError(error.error);
                    }

                    return;
                }
                catch (System.Exception e)
                {
                    Debug.LogErrorFormat("Error accept or decline request: {0}", e.Message);
                    return;
                }
            }
        }

        public static async Task TrySendLocation(Location location, string url, string stoken, CancellationToken ctoken = default)
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
                return;
            }

            try
            {
                RPCError error = JsonReader.Deserialize<RPCError>(resJson);
                if (error != null)
                {
                    ReportError(error.error);
                }
            }
            catch (System.Exception)
            {
                Debug.Log("Location sent");
            }
        }

        public static async Task TrySendComplaint(CreateComplaintRequest data, string url, string stoken, CancellationToken ctoken = default)
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
                return;
            }

            try
            {
                RPCError error = JsonReader.Deserialize<RPCError>(resJson);
                if (error != null)
                {
                    ReportError(error.error);
                }
            }
            catch (System.Exception)
            {
                Debug.Log("Location sent");
            }
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
            if (string.IsNullOrEmpty(resJson))
            {
                Debug.LogError("Decline event request is empty");
                return null;
            }

            try
            {
                RPCResult<List<string>> res = JsonReader.Deserialize<RPCResult<List<string>>>(resJson);
                return res.result[0];
            }
            catch
            {
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
                    Debug.LogErrorFormat("Error decline event: {0}", e.Message);
                    return null;
                }
            }
        }

        static Event GetParsedEvent(string resJson)
        {
            if (string.IsNullOrEmpty(resJson))
            {
                Debug.LogError("Request result is empty");
                return null;
            }

            try
            {
                RPCResult<List<Event>> res = JsonReader.Deserialize<RPCResult<List<Event>>>(resJson);
                return (res != null && res.result.Count > 0) ? res.result[0] : null;
            }
            catch
            {
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
                    Debug.LogErrorFormat("Error create event: {0}", e.Message);
                    return null;
                }
            }
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
            if (string.IsNullOrEmpty(resJson))
            {
                Debug.LogError("Confirm register request is empty");
                return new ConfirmRegisterResult();
            }

            try
            {
                RPCResult<ConfirmRegisterResult> res = JsonReader.Deserialize<RPCResult<ConfirmRegisterResult>>(resJson);
                return res.result;
            }
            catch
            {
                try
                {
                    RPCError error = JsonReader.Deserialize<RPCError>(resJson);
                    if (error != null)
                    {
                        ReportError(error.error);
                    }

                    return new ConfirmRegisterResult();
                }
                catch (System.Exception ex)
                {
                    Debug.LogErrorFormat("Error get keys: {0}", ex.Message);
                    return new ConfirmRegisterResult();
                }
            }
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
            if (string.IsNullOrEmpty(resJson))
            {
                Debug.LogError("Confirm register request is empty");
                return new ConfirmRegisterResult();
            }

            try
            {
                RPCResult<ConfirmRegisterResult> res = JsonReader.Deserialize<RPCResult<ConfirmRegisterResult>>(resJson);
                return res.result;
            }
            catch
            {
                try
                {
                    RPCError error = JsonReader.Deserialize<RPCError>(resJson);
                    if (error != null)
                    {
                        ReportError(error.error);
                    }

                    return new ConfirmRegisterResult();
                }
                catch (System.Exception ex)
                {
                    Debug.LogErrorFormat("Error get keys: {0}", ex.Message);
                    return new ConfirmRegisterResult();
                }
            }
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
            if (string.IsNullOrEmpty(resJson))
            {
                Debug.LogError("Check bday request is empty");
                return true;
            }

            try
            {
                RPCResult<List<CheckBirthdayResult>> res = JsonReader.Deserialize<RPCResult<List<CheckBirthdayResult>>>(resJson);
                return res.result[0].invalid;
            }
            catch
            {
                try
                {
                    RPCError error = JsonReader.Deserialize<RPCError>(resJson);
                    if (error != null)
                    {
                        ReportError(error.error);
                    }

                    return true;
                }
                catch (System.Exception ex)
                {
                    Debug.LogErrorFormat("Error get keys: {0}", ex.Message);
                    return true;
                }
            }
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
            if (string.IsNullOrEmpty(resJson))
            {
                Debug.LogError("Log in request is empty");
                return new ConfirmRegisterResult();
            }

            try
            {
                RPCResult<ConfirmRegisterResult> res = JsonReader.Deserialize<RPCResult<ConfirmRegisterResult>>(resJson);
                return res.result;
            }
            catch
            {
                try
                {
                    RPCError error = JsonReader.Deserialize<RPCError>(resJson);
                    if (error != null)
                    {
                        ReportError(error.error);
                    }

                    return new ConfirmRegisterResult();
                }
                catch (System.Exception ex)
                {
                    Debug.LogErrorFormat("Error login: {0}", ex.Message);
                    return new ConfirmRegisterResult();
                }
            }
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

            try
            {
                RPCError error = JsonReader.Deserialize<RPCError>(resJson);
                if (error != null)
                {
                    ReportError(error.error);
                }
            }
            catch
            {
                Debug.Log("User successfully deleted");
            }
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
            if (string.IsNullOrEmpty(resJson))
            {
                Debug.LogError("Log in request is empty");
                return null;
            }

            Debug.Log(resJson);
            try
            {
                RPCResult<List<ProfileContainer>> res = JsonReader.Deserialize<RPCResult<List<ProfileContainer>>>(resJson);
                return res.result[0].profile;
            }
            catch
            {
                try
                {
                    RPCError error = JsonReader.Deserialize<RPCError>(resJson);
                    if (error != null)
                    {
                        ReportError(error.error);
                    }

                    return null;
                }
                catch (System.Exception ex)
                {
                    Debug.LogErrorFormat("Error update user: {0}", ex.Message);
                    return null;
                }
            }
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
            if (string.IsNullOrEmpty(resJson))
            {
                Debug.LogError("My requests result is empty");
                return null;
            }

            try
            {
                RPCResult<List<Request>> res = JsonReader.Deserialize<RPCResult<List<Request>>>(resJson);
                return res?.result;
            }
            catch
            {
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
                    Debug.LogErrorFormat("Error get my requests: {0}", e.Message);
                    return null;
                }
            }
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
