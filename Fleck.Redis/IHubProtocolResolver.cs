using System;
using System.Collections.Generic;
using System.Text;

namespace Fleck.Extensions
{
    /// <summary>
    /// A resolver abstraction for working with <see cref="ISimpleProtocol"/> instances.
    /// </summary>
    public interface IHubProtocolResolver
    {
        /// <summary>
        /// Gets a collection of all available hub protocols.
        /// </summary>
        IReadOnlyList<ISimpleProtocol> AllProtocols { get; }

        /// <summary>
        /// Gets the hub protocol with the specified name, if it is allowed by the specified list of supported protocols.
        /// </summary>
        /// <param name="protocolName">The protocol name.</param>
        /// <param name="supportedProtocols">A collection of supported protocols.</param>
        /// <returns>A matching <see cref="ISimpleProtocol"/> or <c>null</c> if no matching protocol was found.</returns>
        ISimpleProtocol GetProtocol(string protocolName, IReadOnlyList<string> supportedProtocols);
    }
}
