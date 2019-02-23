using Fleck.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KCEX.Futures.OpenApi.WebSocket
{
    public class TestHandler : IMessageHandler
    {
        private readonly IConnectionLifetimeManager connectionLifetimeManager;

        public TestHandler(IConnectionLifetimeManager connectionLifetimeManager)
        {
            this.connectionLifetimeManager = connectionLifetimeManager;
        }

        public Task<Response> HandleMessage(Message message, string messageContent, IConnectionContext context)
        {
            Console.WriteLine(messageContent);

            connectionLifetimeManager.SendAllExceptAsync(message, new string[] { context.ConnectionId });

            return Task.FromResult(Response.VoidResponse);
        }
    }
}
