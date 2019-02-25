using Fleck.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest
{
    public class TestHandler : IMessageHandler
    {
        private readonly IConnectionLifetimeManager connectionLifetimeManager;

        public TestHandler(IConnectionLifetimeManager connectionLifetimeManager)
        {
            this.connectionLifetimeManager = connectionLifetimeManager;
        }

        public async Task<Response> HandleMessage(Message message, string messageContent, IConnectionContext context)
        {
            Console.WriteLine(messageContent);

            await context.SendAllExceptAsync(message, new string[] { context.ConnectionId });

            return Response.VoidResponse;
        }
    }
}
