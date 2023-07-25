using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using WIGO.Core;

public class TestRPC : MonoBehaviour
{
    [SerializeField] TMP_Text _text;
    [SerializeField] string _ltoken;

    const string URL = "http://v2.cerebrohq.com/testapi/rpc.php";

    public async void OnTestBtnClick()
    {
        var res = await NetService.TryLogin(_ltoken);
        if (res != null)
        {
            _text.text = string.Format("UID: {0}\r\n+7 {1}", res.uid, res.phone);
        }
    }

    public void OnTestRequest()
    {
        var uid = SystemInfo.deviceUniqueIdentifier;
        var language = Application.systemLanguage.ToString();
        string slang = language.Substring(0, 2).ToLower();
        RequestTestRPC test = new RequestTestRPC()
        {
            jsonrpc = "2.0",
            method = "register",
            paramsList = new List<string> { slang, uid, "79032222222" },
            id = "0"
        };
        string json = JsonReader.Serialize(test); //"{\"jsonrpc\":\"2.0\"," +
                                                  //"\"method\":\"register\"}" +
                                                  //"\"params\":\"[\"ru\", \"sdfsdfsfdsfs346sfgnsfg567cv\", \"79032222222\"]\"}" +
                                                  //"\"id\":\"0\"}";
        string request = json.Replace("paramsList", "params");
        PostRequest(request);
        //StartCoroutine(Post(request));
    }

    async void PostRequest(string request)
    {
        // [TODO]: make STATIC
        HttpClient client = new HttpClient();
        //var content = new StringContent(request, Encoding.UTF8, "application/json");
        //var result = await client.PostAsync(URL, content);

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

        var response = await client.SendAsync(httpRequestMessage);

        if (response.IsSuccessStatusCode)
        {
            var res = await response.Content.ReadAsStringAsync();
            _text.text = res;
        }
    }

    IEnumerator Post(string bodyJsonString)
    {
        var request = new UnityWebRequest(URL, "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        //request.SetRequestHeader("rpcauth", "adg463df");

        yield return request.SendWebRequest();

        switch (request.result)
        {
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.DataProcessingError:
                Debug.LogError("Error: " + request.error);
                break;
            case UnityWebRequest.Result.ProtocolError:
                Debug.LogError("HTTP Error: " + request.error);
                break;
            case UnityWebRequest.Result.Success:
                _text.text = request.downloadHandler.text;
                break;
        }
    }

    [System.Serializable]
    public class RequestTestRPC
    {
        public string jsonrpc;
        public string method;
        public List<string> paramsList;
        public string id;
    }
}
