using System;
using System.Collections.Generic;
using UnityEngine;

namespace WIGO.Core
{
    [Serializable]
    public class RPCRequest
    {
        public string jsonrpc;
        public string method;
        public List<string> @params;
        public string id;
    }

    [Serializable]
    public class RPCResult<TObj>
    {
        public TObj result;
        public ErrorResult error;
        public string jsonrpc;
        public string id;
    }

    [Serializable]
    public struct RegisterResult
    {
        public string token;
    }

    [Serializable]
    public struct ConfirmRegisterResult
    {
        public string ltoken;
        public string stoken;
        public ProfileData profile;
    }

    [Serializable]
    public struct AuthorizeResult
    {
        public string stoken;
        public ProfileData profile;
    }

    [Serializable]
    public class ErrorResult
    {
        public int code;
        public string message;
    }
}
