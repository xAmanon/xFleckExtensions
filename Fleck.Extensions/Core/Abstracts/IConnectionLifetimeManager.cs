using Fleck.Extensions.Core.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fleck.Extensions
{
    public interface IConnectionLifetimeManager
    {
        Task<IConnectionContext> GetConnectionContext(string connectionId);
        Task OnConnectedAsync(IConnectionContext connection);
        Task OnDisconnectedAsync(IConnectionContext connection);
        Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default);
        Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default);
        Task SendGroupAsync(string groupName, IPushMessage message, CancellationToken cancellationToken = default);
        Task SendGroupsAsync(IReadOnlyList<string> groupNames, IPushMessage message, CancellationToken cancellationToken = default);
        Task SendGroupExceptAsync(string groupName, IPushMessage message, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default);
        Task SendAllAsync(IPushMessage message, CancellationToken cancellationToken = default);
        Task SendAllExceptAsync(IPushMessage message, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default);
        Task SendConnectionsAsync(IReadOnlyList<string> connectionIds, IPushMessage message, CancellationToken cancellationToken = default);
        Task SendUsersAsync(IReadOnlyList<string> userIds, IPushMessage message, CancellationToken cancellationToken = default);
    }
}
