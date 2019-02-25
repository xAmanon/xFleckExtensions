using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fleck.Extensions
{
    public abstract class MessageHandler<TData> : IMessageHandler where TData : Message
    {
        public virtual Task<Response> HandleMessage(Message message, string messageContent, IConnectionContext context)
        {
            var messageData = message as TData;
            if (messageData == null)
            {
               // context.Protocol.TryParseMessage(messageContent, out messageData);
            }
            return this.Handle(messageData, context);
        }

        public abstract Task<Response> Handle(TData data, IConnectionContext context);
    }
}
