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

        public static async Task<string> TryRegisterNewAccount(string phoneNumber, CancellationToken token = default)
        {
            var uid = SystemInfo.deviceUniqueIdentifier;
            var language = Application.systemLanguage.ToString();
            string slang = language.Substring(0, 2).ToLower();
            RPCRequest request = new RPCRequest()
            {
                jsonrpc = "2.0",
                method = "register",
                @params = new List<string> { slang, uid, phoneNumber },
                id = "0"
            };
            string json = JsonReader.Serialize(request);
            var resJson = await PostRequest(json, token);

            Debug.LogFormat("Answer: {0}", resJson);
            if (string.IsNullOrEmpty(resJson))
            {
                Debug.LogError("Register request is empty");
                return null;
            }

            try
            {
                RPCResult<RegisterResult> res = JsonReader.Deserialize<RPCResult<RegisterResult>>(resJson);
                return res.result.token;
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
                    Debug.LogErrorFormat("Error get token: {0}", e.Message);
                    return null;
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
            catch (System.Exception ex)
            {
                Debug.LogErrorFormat("Error user remove: {0}", ex.Message);
                return;
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

        public static async Task<ProfileData> TryUpdateUser(string uid, string stoken, string birthday, string username, CancellationToken token = default)
        {
            string userUpdJson = "{\"uid\":" + $"\"{uid}\"," +
                                 "\"birthday\":" + $"\"{birthday}\"," +
                                 "\"nickname\":" + $"\"{username}\"}}";
            RPCRequest request = new RPCRequest()
            {
                jsonrpc = "2.0",
                method = "userSet",
                @params = new List<string> { userUpdJson },
                id = "0"
            };

            string json = JsonReader.Serialize(request);
            var resJson = await PostRequest(json, token, stoken);

            if (string.IsNullOrEmpty(resJson))
            {
                Debug.LogError("Log in request is empty");
                return null;
            }

            Debug.Log(resJson);
            try
            {
                RPCResult<List<ProfileData>> res = JsonReader.Deserialize<RPCResult<List<ProfileData>>>(resJson);
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
                catch (System.Exception ex)
                {
                    Debug.LogErrorFormat("Error update user: {0}", ex.Message);
                    return null;
                }
            }
        }

        static async Task<string> PostRequest(string request, CancellationToken token, string addHeader = null)
        {
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new System.Uri(URL),
                Headers = {
                    { HttpRequestHeader.ContentType.ToString(), "application/json" }//,
                    //{ "rpcauth", "adg463df" }
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
    }
}
