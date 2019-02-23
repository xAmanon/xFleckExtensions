using System;
using System.Collections.Generic;
using System.Text;

namespace Fleck.Extensions
{
    public class Message
    {
        public string id { get; set; }
        public string op { get; set; }
        public Int64 time { get; set; }
    }

    public class Message<T> : Message
    {
        public T data { get; set; }
    }
}
