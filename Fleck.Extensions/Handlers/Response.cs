using System;
using System.Collections.Generic;
using System.Text;

namespace Fleck.Extensions
{
    public class Response
    {
        public static readonly Response VoidResponse = new Response();
        public static readonly Response NullResponse = null;

        public virtual object GetData()
        {
            return null;
        }
    }


    public class Response<T> : Response
    {
        public T Data { get; set; }
    }
}
