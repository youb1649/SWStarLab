using System;
using MNF;
using MNF.Message;
using MNF.Crypt;

namespace MNF_Common
{
	static class AESBinaryMessageBuffer
	{
		public static int MaxMessageSize()
		{
			return 1024 * 32;
		}
	}

    public class AESBinaryMessageSerializer : Serializer<CryptBinaryMessageHeader>
    {
        MD5Ref md5Ref;
        AESRef aesRef;

        public AESBinaryMessageSerializer() : base(AESBinaryMessageBuffer.MaxMessageSize())
        {
            md5Ref = new MD5Ref();
            aesRef = new AESRef();
            aesRef.setKey(AESKey.KEY, AESKey.IV);
        }

        protected override void _Serialize<T>(int messageID, T managedData)
        {
            int serializedSize = 0;
            MarshalHelper.RawSerialize(
                managedData
                , GetSerializedBuffer()
                , SerializedHeaderSize
                , ref serializedSize);

            // SerializedBuffer > EncryptedBuffer
            var encryptedBuffer = aesRef.encrypt(GetSerializedBuffer(), SerializedHeaderSize, serializedSize);
            // EncryptedBuffer > SerializedBuffer
            Buffer.BlockCopy(encryptedBuffer, 0, GetSerializedBuffer(), SerializedHeaderSize, encryptedBuffer.Length);
            SerializedLength = encryptedBuffer.Length;

            // Compute Hash message body
            var encryptedByte = md5Ref.Md5Sum(encryptedBuffer, 0, encryptedBuffer.Length);

            var messageHeader = MessageHeader;
            messageHeader.messageSize = (short)(encryptedBuffer.Length);
            messageHeader.messageID = (ushort)messageID;

            // convert hash to two UInt64
            md5Ref.convertToInteger(encryptedByte, out messageHeader.checksum1, out messageHeader.checksum2);

            MarshalHelper.RawSerialize(messageHeader, GetSerializedBuffer(), 0, ref serializedSize);
            SerializedLength += serializedSize;
        }
    }

    public class AESBinaryMessageDeserializer : Deserializer<CryptBinaryMessageHeader>
    {
        MD5Ref md5Ref;
        AESRef aesRef;
        IntPtr marshalAllocatedBuffer;
        int marshalAllocatedBufferSize;

        public AESBinaryMessageDeserializer() : base(AESBinaryMessageBuffer.MaxMessageSize())
        {
            marshalAllocatedBufferSize = AESBinaryMessageBuffer.MaxMessageSize();
            marshalAllocatedBuffer = MarshalHelper.AllocGlobalHeap(marshalAllocatedBufferSize);
            md5Ref = new MD5Ref();
            aesRef = new AESRef();
            aesRef.setKey(AESKey.KEY, AESKey.IV);
        }

        ~AESBinaryMessageDeserializer()
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

            // SerializedBuffer > decryptedBuffer
            var decryptedBuffer = aesRef.decrypt(SerializedBuffer, 0, messageHeader.messageSize);

            object message = MarshalHelper.RawDeSerialize(
                decryptedBuffer
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