﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fleck.Extensions
{
    internal class GroupList : IReadOnlyCollection<ConcurrentDictionary<string, IConnectionContext>>
    {
        private readonly ConcurrentDictionary<string, GroupConnectionList> _groups =
            new ConcurrentDictionary<string, GroupConnectionList>(StringComparer.Ordinal);

        private static readonly GroupConnectionList EmptyGroupConnectionList = new GroupConnectionList();

        public ConcurrentDictionary<string, IConnectionContext> this[string groupName]
        {
            get
            {
                _groups.TryGetValue(groupName, out var group);
                return group;
            }
        }

        public void Add(IConnectionContext connection, string groupName)
        {
            CreateOrUpdateGroupWithConnection(groupName, connection);
        }

        public void Remove(string connectionId, string groupName)
        {
            if (_groups.TryGetValue(groupName, out var connections))
            {
                if (connections.TryRemove(connectionId, out var _) && connections.IsEmpty)
                {
                    // If group is empty after connection remove, don't need empty group in dictionary.
                    // Why this way? Because ICollection.Remove implementation of dictionary checks for key and value. When we remove empty group,
                    // it checks if no connection added from another thread.
                    var groupToRemove = new KeyValuePair<string, GroupConnectionList>(groupName, EmptyGroupConnectionList);
                    ((ICollection<KeyValuePair<string, GroupConnectionList>>)(_groups)).Remove(groupToRemove);
                }
            }
        }

        public void RemoveDisconnectedConnection(string connectionId)
        {
            var groupNames = _groups.Where(x => x.Value.Keys.Contains(connectionId)).Select(x => x.Key);
            foreach (var groupName in groupNames)
            {
                Remove(connectionId, groupName);
            }
        }

        public int Count => _groups.Count;

        public IEnumerator<ConcurrentDictionary<string, IConnectionContext>> GetEnumerator()
        {
            return _groups.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void CreateOrUpdateGroupWithConnection(string groupName, IConnectionContext connection)
        {
            _groups.AddOrUpdate(groupName, _ => AddConnectionToGroup(connection, new GroupConnectionList()),
                (key, oldCollection) =>
                {
                    AddConnectionToGroup(connection, oldCollection);
                    return oldCollection;
                });
        }

        private static GroupConnectionList AddConnectionToGroup(
            IConnectionContext connection, GroupConnectionList group)
        {
            group.AddOrUpdate(connection.ConnectionId, connection, (_, __) => connection);
            return group;
        }
    }

    internal class GroupConnectionList : ConcurrentDictionary<string, IConnectionContext>
    {
        public override bool Equals(object obj)
        {
            if (obj is ConcurrentDictionary<string, IConnectionContext> list)
            {
                return list.Count == Count;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
