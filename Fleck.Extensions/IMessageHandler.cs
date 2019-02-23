using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fleck.Extensions
{
    public interface IMessageHandler
    {
        Task<Response> HandleMessage(Message message, string messageContent, IConnectionContext context);
    }

}
