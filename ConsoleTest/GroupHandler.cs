using Fleck.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KCEX.Futures.OpenApi.WebSocket
{
    public class GroupHandler: MessageHandler<GroupMessage>
    {
        private readonly IConnectionLifetimeManager connectionLifetimeManager;

        public GroupHandler(IConnectionLifetimeManager connectionLifetimeManager)
        {
            this.connectionLifetimeManager = connectionLifetimeManager;
        }

        public override async Task<Response> Handle(GroupMessage message,  IConnectionContext context)
        {
            Console.WriteLine("message");

            switch (message.data.Operator)
            {
                case GroupOperator.Add:
                    await this.connectionLifetimeManager.AddToGroupAsync(context.ConnectionId, message.data.GroupName);
                    break;
                case GroupOperator.Remove:
                    await this.connectionLifetimeManager.RemoveFromGroupAsync(context.ConnectionId, message.data.GroupName);
                    break;
                default:
                    break;
            }

            return Response.NullResponse;
        }
    }

    public class GroupMessage : Message<GroupData>
    {

    }

    public class GroupData
    {
        public GroupOperator Operator { get; set; }
        public string GroupName { get; set; }
    }

    public enum GroupOperator
    {
       Add,
       Remove
    }


    public class GroupSendHandler : MessageHandler<GroupSendMessage>
    {
        private readonly IConnectionLifetimeManager connectionLifetimeManager;

        public GroupSendHandler(IConnectionLifetimeManager connectionLifetimeManager)
        {
            this.connectionLifetimeManager = connectionLifetimeManager;
        }

        public override async Task<Response> Handle(GroupSendMessage message, IConnectionContext context)
        {
            Console.WriteLine("GroupSendHandler");

            await connectionLifetimeManager.SendGroupAsync(message.GroupName, message);

            return Response.NullResponse;
        }
    }


    public class GroupSendMessage : Message
    {
        public string GroupName { get; set; }
        public string Data { get; set; }
    }
}
