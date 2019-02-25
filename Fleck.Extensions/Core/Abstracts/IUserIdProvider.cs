using System;
using System.Collections.Generic;
using System.Text;

namespace Fleck.Extensions.Core.Abstracts
{
    public interface IUserIdProvider
    {
        string GetUserId(IWebSocketConnectionInfo ConnectionInfo);
    }
}
