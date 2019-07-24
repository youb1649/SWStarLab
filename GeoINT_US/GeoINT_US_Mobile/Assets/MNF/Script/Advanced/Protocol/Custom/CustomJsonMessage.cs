using System;
using System.Text;
using UnityEngine;
using MNF;
using MNF.Message;

namespace MNF_Common
{
    static class CustomJsonMessageBuffer
    {
        public static int MaxMessageSize()
        {
            return 1024 * 32;
        }
    }
    
    public class CustomJsonMessageSerializer : Serializer<CustomJsonMessageHeader>
    {
        public CustomJsonMessageSerializer() : base(CustomJsonMessageBuffer.MaxMessageSize())
        {
        }

        public byte[] convertToByte(CustomJsonMessageHeader header)
        {
            int byteArray = 0;
            byteArray = header.messageID;
            byteArray <<= 16;
            byteArray |= (int)header.messageSize;

            return BitConverter.GetBytes(byteArray);
        }

        protected override void _Serialize<T>(int messageID, T managedData)
        {
            var jsonData = JsonUtility.ToJson(managedData);
            var convertedData = Encoding.UTF8.GetBytes(jsonData);
            SerializedLength = convertedData.Length;

            MessageHeader.messageSize = (short)convertedData.Length;
            MessageHeader.messageID = (ushort)messageID;

            var convertedHeader = convertToByte(MessageHeader);
            SerializedLength += convertedHeader.Length;

            Buffer.BlockCopy(convertedHeader, 0, GetSerializedBuffer(), 0, convertedHeader.Length);
            Buffer.BlockCopy(convertedData, 0, GetSerializedBuffer(), convertedHeader.Length, convertedData.Length);
        }
    }

    public class CustomJsonMessageDeserializer : Deserializer<CustomJsonMessageHeader>
    {
        public CustomJsonMessageDeserializer() : base(CustomJsonMessageBuffer.MaxMessageSize())
        {
        }

        protected override void _Deserialize(SessionBase session, ref ParsingResult parsingResult)
        {
            var tcpSession = session as TCPSession;

            // check readable header
            if (tcpSession.RecvCircularBuffer.ReadableSize < SerializedHeaderSize)
            {
                parsingResult.parsingResultEnum = ParsingResult.ParsingResultEnum.PARSING_INCOMPLETE;
                return;
            }

            // read header
            if (tcpSession.RecvCircularBuffer.read(SerializedBuffer, SerializedHeaderSize) == false)
            {
                parsingResult.parsingResultEnum = ParsingResult.ParsingResultEnum.PARSING_ERROR;
                return;
            }

            int messageBodySize = (int)(SerializedBuffer[1] * 256) + (int)SerializedBuffer[0];
            int messageSize = messageBodySize + SerializedHeaderSize;
            int messageId = (int)(SerializedBuffer[3] * 256) + (int)SerializedBuffer[2];

            // check header id
            var dispatchInfo = tcpSession.DispatchHelper.TryGetMessageDispatch(messageId);
            if (dispatchInfo == null)
            {
                parsingResult.parsingResultEnum = ParsingResult.ParsingResultEnum.PARSING_ERROR;
                return;
            }

            // check readalbe body
            if (tcpSession.RecvCircularBuffer.ReadableSize < SerializedHeaderSize + messageBodySize)
            {
                parsingResult.parsingResultEnum = ParsingResult.ParsingResultEnum.PARSING_INCOMPLETE;
                return;
            }

            // read body
            if (tcpSession.RecvCircularBuffer.read(SerializedBuffer, SerializedHeaderSize, messageBodySize) == false)
            {
                parsingResult.parsingResultEnum = ParsingResult.ParsingResultEnum.PARSING_ERROR;
                return;
            }

            // byte -> string
            var jsonMessage = Encoding.UTF8.GetString(SerializedBuffer);

            // string -> object
            var message = JsonUtility.FromJson(jsonMessage, dispatchInfo.messageType);
            if (message == null)
            {
                parsingResult.parsingResultEnum = ParsingResult.ParsingResultEnum.PARSING_ERROR;
                return;
            }

            parsingResult.parsingResultEnum = ParsingResult.ParsingResultEnum.PARSING_COMPLETE;
            parsingResult.dispatcher = dispatchInfo.dispatcher;
            parsingResult.message = message;
            parsingResult.messageSize = messageSize;

            // pop dispatched message size
            tcpSession.RecvCircularBuffer.pop(messageSize);
        }
    }
}