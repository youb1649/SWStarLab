using System;
using MNF;
using MNF.Message;
using MNF.Crypt;

namespace MNF_Common
{
	static class CryptBinaryMessageBuffer
	{
		public static int MaxMessageSize()
		{
			return 1024 * 32;
		}
	}

    public class CryptBinaryMessageSerializer : Serializer<CryptBinaryMessageHeader>
    {
        MD5Ref md5Ref;

        public CryptBinaryMessageSerializer() : base(CryptBinaryMessageBuffer.MaxMessageSize())
        {
            md5Ref = new MD5Ref();
        }

        protected override void _Serialize<T>(int messageID, T managedData)
        {
            int serializedSize = 0;
            MarshalHelper.RawSerialize(
                managedData
                , GetSerializedBuffer()
                , SerializedHeaderSize
                , ref serializedSize);
            SerializedLength = serializedSize;

            // Compute Hash message body
            var encryptedByte = md5Ref.Md5Sum(GetSerializedBuffer(), SerializedHeaderSize, serializedSize);

            var messageHeader = MessageHeader;
            messageHeader.messageSize = (short)(serializedSize);
            messageHeader.messageID = (ushort)messageID;

            // convert hash to two UInt64
            md5Ref.convertToInteger(encryptedByte, out messageHeader.checksum1, out messageHeader.checksum2);

            MarshalHelper.RawSerialize(messageHeader, GetSerializedBuffer(), 0, ref serializedSize);
            SerializedLength += serializedSize;
        }
    }

    public class CryptBinaryMessageDeserializer : Deserializer<CryptBinaryMessageHeader>
    {
        MD5Ref md5Ref;
        IntPtr marshalAllocatedBuffer;
        int marshalAllocatedBufferSize = 0;

        public CryptBinaryMessageDeserializer() : base(CryptBinaryMessageBuffer.MaxMessageSize())
        {
            md5Ref = new MD5Ref();
            marshalAllocatedBufferSize = CryptBinaryMessageBuffer.MaxMessageSize();
            marshalAllocatedBuffer = MarshalHelper.AllocGlobalHeap(marshalAllocatedBufferSize);
        }

        ~CryptBinaryMessageDeserializer()
        {
            MarshalHelper.DeAllocGlobalHeap(marshalAllocatedBuffer);
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

            var messageHeader = MarshalHelper.RawDeSerialize(
                SerializedBuffer
                , HeaderType
                , 0
                , ref marshalAllocatedBuffer
                , ref marshalAllocatedBufferSize) as CryptBinaryMessageHeader;

            // check header id
            var dispatchInfo = tcpSession.DispatchHelper.TryGetMessageDispatch(messageHeader.messageID);
            if (dispatchInfo == null)
            {
                parsingResult.parsingResultEnum = ParsingResult.ParsingResultEnum.PARSING_ERROR;
                return;
            }

            // check readalbe body
            if (tcpSession.RecvCircularBuffer.ReadableSize < (SerializedHeaderSize + messageHeader.messageSize))
            {
                parsingResult.parsingResultEnum = ParsingResult.ParsingResultEnum.PARSING_INCOMPLETE;
                return;
            }

            // read body
            if (tcpSession.RecvCircularBuffer.read(SerializedBuffer, SerializedHeaderSize, messageHeader.messageSize) == false)
            {
                parsingResult.parsingResultEnum = ParsingResult.ParsingResultEnum.PARSING_ERROR;
                return;
            }

            // convert hash to two UInt64
            var encryptedByte = md5Ref.Md5Sum(SerializedBuffer, 0, messageHeader.messageSize);
            UInt64 checksum1 = 0, checksum2 = 0;
            md5Ref.convertToInteger(encryptedByte, out checksum1, out checksum2);

            // compare checksum
            if ((checksum1 != messageHeader.checksum1) || (checksum2 != messageHeader.checksum2))
            {
                parsingResult.parsingResultEnum = ParsingResult.ParsingResultEnum.PARSING_ERROR;
                return;
            }

            object message = MarshalHelper.RawDeSerialize(
                SerializedBuffer
                , dispatchInfo.messageType
                , 0
                , ref marshalAllocatedBuffer
                , ref marshalAllocatedBufferSize);
            if (message == null)
                throw new Exception(string.Format("Dispatcher({0}), Expect packet size({1})",
                    dispatchInfo, MarshalHelper.GetManagedDataSize(dispatchInfo.messageType)));

            int messageSize = messageHeader.messageSize + SerializedHeaderSize;
            parsingResult.parsingResultEnum = ParsingResult.ParsingResultEnum.PARSING_COMPLETE;
            parsingResult.dispatcher = dispatchInfo.dispatcher;
            parsingResult.message = message;
            parsingResult.messageSize = messageSize;

            // pop dispatched message size
            tcpSession.RecvCircularBuffer.pop(messageSize);
        }
    }
}