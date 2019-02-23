﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Fleck.Extensions.Redis
{
    public readonly struct RedisInvocation
    {
        /// <summary>
        /// Gets a list of connections that should be excluded from this invocation.
        /// May be null to indicate that no connections are to be excluded.
        /// </summary>
        public IReadOnlyList<string> ExcludedConnectionIds { get; }

        /// <summary>
        /// Gets the message serialization cache containing serialized payloads for the message.
        /// </summary>
        public SerializedSimpleMessage Message { get; }

        public RedisInvocation(SerializedSimpleMessage message, IReadOnlyList<string> excludedConnectionIds)
        {
            Message = message;
            ExcludedConnectionIds = excludedConnectionIds;
        }

        public static RedisInvocation Create(Message message, IReadOnlyList<string> excludedConnectionIds = null)
        {
            return new RedisInvocation(
                new SerializedSimpleMessage(message),//message
                excludedConnectionIds);
        }
    }
}
