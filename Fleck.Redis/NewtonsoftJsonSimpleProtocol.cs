using Fleck.Extensions.Shared;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Fleck.Extensions
{
    /// <summary>
    /// Implements the SignalR Hub Protocol using Newtonsoft.Json.
    /// </summary>
    public class NewtonsoftJsonSimpleProtocol : ISimpleProtocol
    {
        private const string ResultPropertyName = "result";
        private const string ItemPropertyName = "item";
        private const string InvocationIdPropertyName = "invocationId";
        private const string StreamIdsPropertyName = "streamIds";
        private const string TypePropertyName = "type";
        private const string ErrorPropertyName = "error";
        private const string TargetPropertyName = "target";
        private const string ArgumentsPropertyName = "arguments";
        private const string HeadersPropertyName = "headers";

        private static readonly string ProtocolName = "json";
        private static readonly int ProtocolVersion = 1;
        private static readonly int ProtocolMinorVersion = 0;

        /// <summary>
        /// Gets the serializer used to serialize invocation arguments and return values.
        /// </summary>
        public JsonSerializer PayloadSerializer { get; }

        public IMessageTypeMapper MessageTypeMapper { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewtonsoftJsonSimpleProtocol"/> class.
        /// </summary>
        public NewtonsoftJsonSimpleProtocol(IMessageTypeMapper messageTypeMapper)
        {
            MessageTypeMapper = messageTypeMapper;
            PayloadSerializer = JsonSerializer.Create(CreateDefaultSerializerSettings());
        }


        public string Name => ProtocolName;

        public bool TryParseMessage(string text, out Message message)
        {
            message = null;
            try
            {
                var jtoken = JObject.Parse(text);

                var op = JsonUtils.GetOptionalProperty<string>(jtoken, "op");

                if (!string.IsNullOrEmpty(op))
                {
                    Type type = MessageTypeMapper.GetMapType(op) ?? typeof(Message);

                    if (type != null)
                    {
                        message = (Message)jtoken.ToObject(type);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return false;
        }

        public bool TryParseMessage<TMessage>(string text, out TMessage message) where TMessage : Message
        {
            message = JsonConvert.DeserializeObject<TMessage>(text);
            return true;
        }

        public string GetMessageText(Message message)
        {
            StringBuilder sb = new StringBuilder();
            using (TextWriter writer = new StringWriter(sb))
            {
                PayloadSerializer.Serialize(writer, message);
                writer.Flush();;
            }
            return sb.ToString();
        }

        internal static JsonSerializerSettings CreateDefaultSerializerSettings()
        {
            return new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
        }
    }
}
