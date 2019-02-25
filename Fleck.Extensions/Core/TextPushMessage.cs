using Fleck.Extensions.Core.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fleck.Extensions.Core
{
    public class TextPushMessage : IPushMessage
    {
        public string Message { get; set; }

        public TextPushMessage(string message)
        {
            this.Message = message;
        }

        public string GetMessage()
        {
            return this.Message;
        }
    }
}
