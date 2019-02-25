using Fleck.Extensions.Core.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fleck.Extensions.Handlers
{
    public class ObjectPushMessage : IPushMessage
    {
        private Message _data;
        private string _serializedText;
        private ISimpleProtocol _protocol;

        public ObjectPushMessage(Message data,ISimpleProtocol protocol)
        {
            this._data = data;
            this._protocol = protocol;
        }

        public string GetMessage()
        {
            if (_data != null)
            {
                if (string.IsNullOrEmpty(_serializedText))
                {
                    _serializedText = _protocol.GetMessageText(_data);
                }
                return _serializedText;
            }
            return string.Empty;
        }
    }
}
