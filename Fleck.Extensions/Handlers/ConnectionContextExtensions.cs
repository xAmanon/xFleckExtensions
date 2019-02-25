using System;
using System.Collections.Generic;
using System.Text;

namespace Fleck.Extensions.Handlers
{
    public static class ConnectionContextExtensions
    {
        public static void SetProtocol(this IConnectionContext context, ISimpleProtocol simpleProtocol)
        {
            context.Features.Set(simpleProtocol);
        }

        public static ISimpleProtocol GetProtocol(this IConnectionContext context)
        {
            return context.Features.Get<ISimpleProtocol>();
        }

        public static void SetLifetimeManager(this IConnectionContext context, IConnectionLifetimeManager connectionLifetimeManager)
        {
            context.Features.Set(connectionLifetimeManager);
        }

        public static IConnectionLifetimeManager GetLifetimeManager(this IConnectionContext context)
        {
            return context.Features.Get<IConnectionLifetimeManager>();
        }
    }
}
