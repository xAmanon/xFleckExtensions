using System;
using System.Collections.Generic;
using System.Text;

namespace Fleck.Extensions
{
    public class ReturnException : Exception
    {
        public int ErrorCode { get; set; }

        public ReturnException(int errorCode, string message) : base(message)
        {
            this.ErrorCode = errorCode;
        }
    }
}
