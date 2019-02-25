using Fleck.Extensions.Core.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fleck.Extensions.Core
{
    public static class ConnectionContextExtensions
    {
        public static async Task WriteAsync(this IConnectionContext context, IPushMessage pushMessage, CancellationToken cancellationToken)
        {
            var message = pushMessage.GetMessage();
            await context.WriteAsync(message, cancellationToken);
        }
    }
}
