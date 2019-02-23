using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fleck.Extensions
{
    public class DefaultHubProtocolResolver : IHubProtocolResolver
    {
        private readonly List<ISimpleProtocol> _hubProtocols;
        private readonly Dictionary<string, ISimpleProtocol> _availableProtocols;

        public IReadOnlyList<ISimpleProtocol> AllProtocols => _hubProtocols;

        public DefaultHubProtocolResolver(IEnumerable<ISimpleProtocol> availableProtocols)
        {
            _availableProtocols = new Dictionary<string, ISimpleProtocol>(StringComparer.OrdinalIgnoreCase);

            // We might get duplicates in _hubProtocols, but we're going to check it and throw in just a sec.
            _hubProtocols = availableProtocols.ToList();
            foreach (var protocol in _hubProtocols)
            {
                if (_availableProtocols.ContainsKey(protocol.Name))
                {
                    throw new InvalidOperationException($"Multiple Hub Protocols with the name '{protocol.Name}' were registered.");
                }
                _availableProtocols.Add(protocol.Name, protocol);
            }
        }

        public virtual ISimpleProtocol GetProtocol(string protocolName, IReadOnlyList<string> supportedProtocols)
        {
            protocolName = protocolName ?? throw new ArgumentNullException(nameof(protocolName));

            if (_availableProtocols.TryGetValue(protocolName, out var protocol) && (supportedProtocols == null || supportedProtocols.Contains(protocolName, StringComparer.OrdinalIgnoreCase)))
            {
                return protocol;
            }

            // null result indicates protocol is not supported
            // result will be validated by the caller
            return null;
        }
    }
}
