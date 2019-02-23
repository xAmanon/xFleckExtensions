using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Fleck.Extensions
{
    public class ConnectionStore
    {
        private readonly ConcurrentDictionary<string, IConnectionContext> _connections =
            new ConcurrentDictionary<string, IConnectionContext>(StringComparer.Ordinal);

        public IConnectionContext this[string connectionId]
        {
            get
            {
                _connections.TryGetValue(connectionId, out var connection);
                return connection;
            }
        }

        public int Count => _connections.Count;

        public void Add(IConnectionContext connection)
        {
            _connections.TryAdd(connection.ConnectionId, connection);
        }

        public void Remove(IConnectionContext connection)
        {
            _connections.TryRemove(connection.ConnectionId, out _);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public readonly struct Enumerator : IEnumerator<IConnectionContext>
        {
            private readonly IEnumerator<KeyValuePair<string, IConnectionContext>> _enumerator;

            public Enumerator(ConnectionStore hubConnectionList)
            {
                _enumerator = hubConnectionList._connections.GetEnumerator();
            }

            public IConnectionContext Current => _enumerator.Current.Value;

            object IEnumerator.Current => Current;

            public void Dispose() => _enumerator.Dispose();

            public bool MoveNext() => _enumerator.MoveNext();

            public void Reset() => _enumerator.Reset();
        }
    }
}
