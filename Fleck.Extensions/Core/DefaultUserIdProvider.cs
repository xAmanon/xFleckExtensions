using Fleck.Extensions.Core.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fleck.Extensions.Core
{
    public class DefaultUserIdProvider : IUserIdProvider
    {
        public string GetUserId(IWebSocketConnectionInfo ConnectionInfo)
        {
            return null;
        }
    }
}
