using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Fleck.Extensions
{
    public interface ISimpleProtocol
    {
        /// <summary>
        /// Gets the name of the protocol. The name is used by SignalR to resolve the protocol between the client and server.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Creates a new <see cref="HubMessage"/> from the specified serialized representation, and using the specified binder.
        /// </summary>
        /// <param name="input">The serialized representation of the message.</param>
       
        /// <param name="message">When this method returns <c>true</c>, contains the parsed message.</param>
        /// <returns>A value that is <c>true</c> if the <see cref="HubMessage"/> was successfully parsed; otherwise, <c>false</c>.</returns>
        bool TryParseMessage(string text, out Message message);

        /// <summary>
        /// TryParseMessage
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="text"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool TryParseMessage<TMessage>(string text, out TMessage message) where TMessage : Message;

        /// <summary>
        /// Converts the specified <see cref="HubMessage"/> to its serialized representation.
        /// </summary>
        /// <param name="message">The message to convert.</param>
        /// <returns>The serialized representation of the message.</returns>
        string GetMessageText(Message message);
    }
}
