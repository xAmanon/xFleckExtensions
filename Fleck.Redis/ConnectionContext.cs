﻿using Fleck.Extensions.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fleck.Extensions
{
    public class ConnectionContext : IConnectionContext
    {
        public ISimpleProtocol Protocol { get; set; }

        public IFeatureCollection Features { get; } = new FeatureCollection();

        public string UserIdentifier { get; set; }

        public string ConnectionId { get; private set; }

        public IWebSocketConnection WebSocket { get; private set; }

        public ConnectionContext(string connectionId, IWebSocketConnection webSocket, string userIdentity = null)
        {
            this.ConnectionId = connectionId;
            this.WebSocket = webSocket;
            this.UserIdentifier = userIdentity;
        }

        public Task WriteAsync(SerializedSimpleMessage message, CancellationToken cancellationToken)
        {
            var msg = message.GetSerializedMessage(Protocol);
            return this.WebSocket.Send(msg);
        }
    }
}
