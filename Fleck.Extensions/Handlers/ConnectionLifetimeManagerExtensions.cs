using Fleck.Extensions.Core;
using Fleck.Extensions.Core.Abstracts;
using Fleck.Extensions.Handlers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fleck.Extensions
{
    public static class ConnectionLifetimeManagerExtensions
    {
        private static ObjectPushMessage CreatePushMessage(this IConnectionContext context,Message message)
        {
            return new ObjectPushMessage(message, context.GetProtocol()); 
        }

        private static IConnectionLifetimeManager GetConnectionLifetimeManager(this IConnectionContext context)
        {
            var lifetimeManager = context.GetLifetimeManager();
            if (lifetimeManager == null)
            {
                throw new Exception("Not found connectionLifetimeManager");
            }
            return lifetimeManager;
        }

        public static async Task SendToCaller(this IConnectionContext context, Message message)
        {
            var pushMessage = context.CreatePushMessage(message);
            await context.WriteAsync(pushMessage, CancellationToken.None);
        }

        public static async Task SendGroupAsync(this IConnectionContext context, string groupName, Message message)
        {
            var pushMessage = context.CreatePushMessage(message);    
            await context.GetConnectionLifetimeManager().SendGroupAsync(groupName, pushMessage);
        }

        public static async Task SendGroupsAsync(this IConnectionContext context, IReadOnlyList<string> groupNames, Message message)
        {
            var pushMessage = context.CreatePushMessage(message);
            await context.GetConnectionLifetimeManager().SendGroupsAsync(groupNames, pushMessage);
        }

        public static async Task SendGroupExceptAsync(this IConnectionContext context, string groupName, Message message, IReadOnlyList<string> excludedConnectionIds)
        {
            var pushMessage = context.CreatePushMessage(message);
            await context.GetConnectionLifetimeManager().SendGroupExceptAsync(groupName, pushMessage, excludedConnectionIds);
        }

        public static async Task SendAllAsync(this IConnectionContext context, Message message)
        {
            var pushMessage = context.CreatePushMessage(message);
            await context.GetConnectionLifetimeManager().SendAllAsync(pushMessage);
        }

        public static async Task SendAllExceptAsync(this IConnectionContext context, Message message, IReadOnlyList<string> excludedConnectionIds)
        {
            var pushMessage = context.CreatePushMessage(message);
            await context.GetConnectionLifetimeManager().SendAllExceptAsync(pushMessage, excludedConnectionIds);
        }

        public static async Task SendConnectionsAsync(this IConnectionContext context, IReadOnlyList<string> connectionIds, Message message)
        {
            var pushMessage = context.CreatePushMessage(message);
            await context.GetConnectionLifetimeManager().SendConnectionsAsync(connectionIds, pushMessage);
        }

        public static async Task SendUsersAsync(this IConnectionContext context, IReadOnlyList<string> userIds, Message message)
        {
            var pushMessage = context.CreatePushMessage(message);
            await context.GetConnectionLifetimeManager().SendUsersAsync(userIds, pushMessage);
        }
    }
}
