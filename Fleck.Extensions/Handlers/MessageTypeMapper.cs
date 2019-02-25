using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Fleck.Extensions
{
    public class MessageTypeMapper : IMessageTypeMapper
    {
        private static readonly ConcurrentDictionary<string, Type> _types =
         new ConcurrentDictionary<string, Type>(StringComparer.Ordinal);

        public Type GetMapType(string op)
        {
            Type type = null;
            if (_types.TryGetValue(op, out type))
            {
                return type;
            }
            return type;
        }

        public void Register(string op, Type type)
        {
            _types[op] = type;
        }
    }
}
