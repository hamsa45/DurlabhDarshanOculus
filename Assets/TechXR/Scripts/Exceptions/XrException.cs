using System;
using UnityEngine;

namespace co.techxr.unity.exceptions
{
    public class XrException : Exception
    {
        public int errorCode { get; set; }
        public string message { get; set; }
    
        public XrException(string msg)
        {
            this.message = msg;
            //Debug.Log(string.Format("Error Message: {0}", this.message));
            Debug.LogError(this.message);
            //Debug.LogException(this);
        }
        public XrException(string msg, int code)
        {
            this.message = msg;
            this.errorCode = code;
            //Debug.Log(string.Format("ErrorCode: {0}\tMessage: {1}", this.errorCode, this.message));
            Debug.LogError(this.message);
            //Debug.LogException(this);
        }
        override public string ToString()
        {
            return string.Format("XrException: {0}", this.message);
        }
    }
}
