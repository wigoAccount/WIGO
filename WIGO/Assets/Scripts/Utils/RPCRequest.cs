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
        public string jsonrpc;
        public string id;
    }

    [Serializable]
    public class RPCError
    {
        public ErrorResult error;
        public string jsonrpc;
        public string id;
    }

    [Serializable]
    public struct ErrorResult
    {
        public int code;
        public string message;
    }

    #region Authorize
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
        public LinksData links;
        public ProfileData profile;
    }

    [Serializable]
    public struct AuthorizeResult
    {
        public string stoken;
        public ProfileData profile;
    }

    [Serializable]
    public struct CheckBirthdayResult
    {
        public bool invalid;
    }

    [Serializable]
    public struct LinksData
    {
        public DataRequestAddress data;
        public MediaLinks photo;
        public MediaLinks video;
    }

    [Serializable]
    public struct DataRequestAddress
    {
        public string address;
    }

    [Serializable]
    public struct MediaLinks
    {
        public string download;
        public string upload;
    }

    [Serializable]
    public struct ProfileContainer
    {
        public ProfileData profile;
    }
    #endregion

    [Serializable]
    public struct CreateEventRequest
    {
        public string title;
        public string about;
        public int waiting;
        public int duration;
        public Location location;
        public string address;
        public string area;
        public string video;
        public string preview;
        public int[] tags_add;
    }
}
