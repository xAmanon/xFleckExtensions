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
        Task SendGroupAsync(string groupName, Message message, CancellationToken cancellationToken = default);
        Task SendGroupsAsync(IReadOnlyList<string> groupNames, Message message, CancellationToken cancellationToken = default);
        Task SendGroupExceptAsync(string groupName, Message message, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default);
        Task SendAllAsync(Message message, CancellationToken cancellationToken = default);
        Task SendAllExceptAsync(Message message, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default);
        Task SendConnectionsAsync(IReadOnlyList<string> connectionIds, Message message, CancellationToken cancellationToken = default);
        Task SendUsersAsync(IReadOnlyList<string> userIds, Message message, CancellationToken cancellationToken = default);
    }
}
