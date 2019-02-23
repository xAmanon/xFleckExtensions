using Fleck.Extensions.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fleck.Extensions
{
    public interface IConnectionContext
    {
        ISimpleProtocol Protocol { get; set; }
        IFeatureCollection Features { get; }
        string UserIdentifier { get; set; }
        string ConnectionId { get; }
        IWebSocketConnection WebSocket { get; }
        Task WriteAsync(SerializedSimpleMessage message, CancellationToken cancellationToken);
    }
}
