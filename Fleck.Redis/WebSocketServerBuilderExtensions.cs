using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Fleck.Extensions
{
    public static class WebSocketServerBuilderExtensions
    {
        public static WebSocketServerBuilder UseHandlerMessage<IHandler>(this WebSocketServerBuilder builder, string opts, Lazy<IHandler> handler) where IHandler : IMessageHandler
        {
            var msgType = GetMessageType(typeof(IHandler));
            if (msgType != null)
            {
                builder.MessageTypeMapper.Register(opts, msgType);
            }

            builder.InvokeHandlerManager.AddMessageHandler(opts, new Lazy<IMessageHandler>(() => (IMessageHandler)handler.Value));

            return builder;
        }

        public static WebSocketServerBuilder UseHandlerMessage<IHandler>(this WebSocketServerBuilder builder, string opts) where IHandler : IMessageHandler
        {
            var msgType = GetMessageType(typeof(IHandler));
            if (msgType != null)
            {
                builder.MessageTypeMapper.Register(opts, msgType);
            }

            var type = typeof(IHandler);

            builder.Services.AddTransient(type, type);

            builder.InvokeHandlerManager.AddMessageHandler(opts, new Lazy<IMessageHandler>(() => (IMessageHandler)builder.ServiceProvider.GetService(type)));

            return builder;
        }


        private static Type GetMessageType(Type messageHandler)
        {
            var generType = Utils.GetImplementedRawGeneric(messageHandler, typeof(MessageHandler<>));
            if (generType != null)
            {
                return generType.GenericTypeArguments[0];
            }
            return null;
        }
    }
}
