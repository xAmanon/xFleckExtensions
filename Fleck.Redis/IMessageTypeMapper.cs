using System;
using System.Collections.Generic;
using System.Text;

namespace Fleck.Extensions
{
    public interface IMessageTypeMapper
    {
        Type GetMapType(string op);
        void Register(string op, Type type);
    }
}
