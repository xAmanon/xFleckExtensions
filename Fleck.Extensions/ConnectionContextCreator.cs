using Fleck.Extensions.Core.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fleck.Extensions
{
    public class ConnectionContextCreator
    {
        public static IConnectionContext Create(IWebSocketConnection webSocket,
            IConnectionLifetimeManager lifetimeManager,
            ISimpleProtocol simpleProtocol,
            IUserIdProvider userIdProvider)
        {
            var connectionId = webSocket.ConnectionInfo.Id.ToString();
            var context = new ConnectionContext(connectionId, webSocket);
            context.Features.Set(simpleProtocol);
            context.Features.Set(lifetimeManager);
            context.UserIdentifier = userIdProvider.GetUserId(webSocket.ConnectionInfo);
            return context;
        }
    }
}
