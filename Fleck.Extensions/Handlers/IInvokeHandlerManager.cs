using System;
using System.Collections.Generic;
using System.Text;

namespace Fleck.Extensions
{
    public interface IInvokeHandlerManager
    {
        void AddMessageHandler(string op, Lazy<IMessageHandler> messageHandler);
        IMessageHandler GetMessageHandler(string op);
    }
}
