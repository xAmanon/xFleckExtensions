using System;
using System.Collections.Generic;
using System.Text;

namespace Fleck.Extensions
{
    public readonly struct SerializedMessage
    {
        /// <summary>
        /// Gets the protocol of the serialized message.
        /// </summary>
        public string ProtocolName { get; }

        /// <summary>
        /// Gets the serialized representation of the message.
        /// </summary>
        public string Serialized { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializedSimpleMessage"/> class.
        /// </summary>
        /// <param name="protocolName">The protocol of the serialized message.</param>
        /// <param name="serialized">The serialized representation of the message.</param>
        public SerializedMessage(string protocolName, string serialized)
        {
            ProtocolName = protocolName;
            Serialized = serialized;
        }
    }
}
