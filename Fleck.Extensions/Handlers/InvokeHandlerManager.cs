using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Fleck.Extensions
{
    public class InvokeHandlerManager : IInvokeHandlerManager
    {
        private ConcurrentDictionary<string, Lazy<IMessageHandler>> _messageHandler = new ConcurrentDictionary<string, Lazy<IMessageHandler>>();

        public void AddMessageHandler(string op, Lazy<IMessageHandler> messageHandler)
        {
            _messageHandler[op] = messageHandler;
        }

        public IMessageHandler GetMessageHandler(string op)
        {
            if (_messageHandler.TryGetValue(op, out Lazy<IMessageHandler> handler))
            {
                return handler.Value;
            }
            return null;
        }
    }
}
