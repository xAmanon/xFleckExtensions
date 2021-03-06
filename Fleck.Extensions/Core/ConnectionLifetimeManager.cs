﻿using Fleck.Extensions.Core.Abstracts;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fleck.Extensions
{
    public class ConnectionLifetimeManager : IConnectionLifetimeManager
    {
        private readonly ConnectionStore _connections = new ConnectionStore();

        private readonly GroupList _groups = new GroupList();

        public ConnectionLifetimeManager()
        {

        }


        public Task<IConnectionContext> GetConnectionContext(string connectionId)
        {
            var connection = _connections[connectionId];
            return Task.FromResult(connection);
        }

        public virtual Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
        {
            if (connectionId == null)
            {
                throw new ArgumentNullException(nameof(connectionId));
            }

            if (groupName == null)
            {
                throw new ArgumentNullException(nameof(groupName));
            }

            var connection = _connections[connectionId];
            if (connection == null)
            {
                return Task.CompletedTask;
            }

            _groups.Add(connection, groupName);

            return Task.CompletedTask;
        }

        public virtual Task OnConnectedAsync(IConnectionContext connection)
        {
            _connections.Add(connection);
            return Task.CompletedTask;
        }

        public virtual Task OnDisconnectedAsync(IConnectionContext connection)
        {
            _connections.Remove(connection);
            _groups.RemoveDisconnectedConnection(connection.ConnectionId);
            return Task.CompletedTask;
        }

        public virtual Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
        {
            if (connectionId == null)
            {
                throw new ArgumentNullException(nameof(connectionId));
            }

            if (groupName == null)
            {
                throw new ArgumentNullException(nameof(groupName));
            }

            var connection = _connections[connectionId];
            if (connection == null)
            {
                return Task.CompletedTask;
            }

            _groups.Remove(connectionId, groupName);

            return Task.CompletedTask;
        }

        public virtual Task SendGroupAsync(string groupName, IPushMessage message, CancellationToken cancellationToken = default)
        {
            if (groupName == null)
            {
                throw new ArgumentNullException(nameof(groupName));
            }

            var group = _groups[groupName];
            if (group != null)
            {
                // Can't optimize for sending to a single connection in a group because
                // group might be modified inbetween checking and sending
                List<Task> tasks = null;

                string serMessage = null;

                SendToGroupConnections(message, group, null, ref tasks, ref serMessage);

                if (tasks != null)
                {
                    return Task.WhenAll(tasks);
                }
            }

            return Task.CompletedTask;
        }

        public virtual Task SendGroupsAsync(IReadOnlyList<string> groupNames, IPushMessage message, CancellationToken cancellationToken = default)
        {
            // Each task represents the list of tasks for each of the writes within a group
            List<Task> tasks = null;

            string serMessage = null;

            foreach (var groupName in groupNames)
            {
                if (string.IsNullOrEmpty(groupName))
                {
                    throw new InvalidOperationException("Cannot send to an empty group name.");
                }

                var group = _groups[groupName];
                if (group != null)
                {
                    SendToGroupConnections(message, group, null, ref tasks,ref serMessage);
                }
            }

            if (tasks != null)
            {
                return Task.WhenAll(tasks);
            }

            return Task.CompletedTask;
        }

        public virtual Task SendGroupExceptAsync(string groupName, IPushMessage message, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default)
        {
            if (groupName == null)
            {
                throw new ArgumentNullException(nameof(groupName));
            }

            var group = _groups[groupName];
            if (group != null)
            {
                List<Task> tasks = null;
                string serMessage = null;
                SendToGroupConnections(message, group, connection => !excludedConnectionIds.Contains(connection.ConnectionId), ref tasks,ref serMessage);

                if (tasks != null)
                {
                    return Task.WhenAll(tasks);
                }
            }

            return Task.CompletedTask;
        }

        private void SendToGroupConnections(IPushMessage message, ConcurrentDictionary<string, IConnectionContext> connections, Func<IConnectionContext, bool> include, ref List<Task> tasks, ref string serializedMessage)
        {
            if (serializedMessage == null)
                serializedMessage = message.GetMessage();
            // foreach over ConcurrentDictionary avoids allocating an enumerator
            foreach (var connection in connections)
            {
                if (include != null && !include(connection.Value))
                {
                    continue;
                }

                var task = connection.Value.WriteAsync(serializedMessage, CancellationToken.None);

                if (!task.IsCompleted)
                {
                    if (tasks == null)
                    {
                        tasks = new List<Task>();
                    }

                    tasks.Add(task);
                }
            }
        }

        public virtual Task SendAllAsync(IPushMessage message, CancellationToken cancellationToken = default)
        {
            return SendToAllConnections(message, null);
        }

        public virtual Task SendAllExceptAsync(IPushMessage message, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default)
        {
            return SendToAllConnections( message, connection => !excludedConnectionIds.Contains(connection.ConnectionId));
        }

        public virtual Task SendConnectionsAsync(IReadOnlyList<string> connectionIds, IPushMessage message, CancellationToken cancellationToken = default)
        {
            return SendToAllConnections(message, connection => connectionIds.Contains(connection.ConnectionId));
        }

        public virtual Task SendUsersAsync(IReadOnlyList<string> userIds, IPushMessage message, CancellationToken cancellationToken = default)
        {
            return SendToAllConnections(message, connection => userIds.Contains(connection.UserIdentifier));
        }

        private Task SendToAllConnections(IPushMessage message, Func<IConnectionContext, bool> include)
        {
            List<Task> tasks = null;

            var serializedMessage = message.GetMessage();

            // foreach over HubConnectionStore avoids allocating an enumerator
            foreach (var connection in _connections)
            {
                if (include != null && !include(connection))
                {
                    continue;
                }

                var task = connection.WriteAsync(serializedMessage, CancellationToken.None);

                if (!task.IsCompleted)
                {
                    tasks.Add(task);
                }
            }

            if (tasks == null)
            {
                return Task.CompletedTask;
            }

            // Some connections are slow
            return Task.WhenAll(tasks);
        }
    }
}
