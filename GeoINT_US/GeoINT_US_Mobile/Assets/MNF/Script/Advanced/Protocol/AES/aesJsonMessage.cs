using System;
using System.Text;
using UnityEngine;
using MNF;
using MNF.Crypt;
using MNF.Message;

namespace MNF_Common
{
	static class AESJsonMessageBuffer
	{
		public static int MaxMessageSize()
		{
			return 1024 * 32;
		}
	}

    public class AESJsonMessageSerializer : Serializer<CryptJsonMessageHeader>
    {
        MD5Ref md5Ref;
        AESRef aesRef;

        public AESJsonMessageSerializer() : base(AESJsonMessageBuffer.MaxMessageSize())
        {
            md5Ref = new MD5Ref();
            aesRef = new AESRef();
            aesRef.setKey(AESKey.KEY, AESKey.IV);
        }

        protected override void _Serialize<T>(int messageID, T managedData)
        {
            var jsonData = JsonUtility.ToJson(managedData);
            var convertedData = Encoding.UTF8.GetBytes(jsonData);

            // SerializedBuffer > EncryptedBuffer
            var encryptedBuffer = aesRef.encrypt(convertedData);

            // Compute Hash message body
            var encryptedByte = md5Ref.Md5Sum(encryptedBuffer);

            var messageHeader = MessageHeader;
            messageHeader.messageSize = (short)encryptedBuffer.Length;
            messageHeader.messageID = (ushort)messageID;
            md5Ref.convertToInteger(encryptedByte, out messageHeader.checksum1, out messageHeader.checksum2);

            int serializedSize = 0;
            MarshalHelper.RawSerialize(messageHeader, GetSerializedBuffer(), 0, ref serializedSize);
            SerializedLength = serializedSize;
            Buffer.BlockCopy(encryptedBuffer, 0, GetSerializedBuffer(), serializedSize, encryptedBuffer.Length);
            SerializedLength += encryptedBuffer.Length;
        }
    }

    public class AESJsonMessageDeserializer : Deserializer<CryptJsonMessageHeader>
    {
        MD5Ref md5Ref;
        AESRef aesRef;
        IntPtr marshalAllocatedBuffer;
        int marshalAllocatedBufferSize = 0;

        public AESJsonMessageDeserializer() : base(AESJsonMessageBuffer.MaxMessageSize())
        {
            md5Ref = new MD5Ref();
            aesRef = new AESRef();
            aesRef.setKey(AESKey.KEY, AESKey.IV);
            marshalAllocatedBufferSize = SerializedHeaderSize;
            marshalAllocatedBuffer = MarshalHelper.AllocGlobalHeap(marshalAllocatedBufferSize);
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
                , ref marshalAllocatedBufferSize) as CryptJsonMessageHeader;

            // check header id
            var dispatchInfo = tcpSession.DispatchHelper.TryGetMessageDispatch(messageHeader.messageID);
            if (dispatchInfo == null)
            {
                parsingResult.parsingResultEnum = ParsingResult.ParsingResultEnum.PARSING_ERROR;
                return;
            }

            // check readalbe body
            if (tcpSession.RecvCircularBuffer.ReadableSize < SerializedHeaderSize + messageHeader.messageSize)
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

            // byte -> string
            var jsonMessage = Encoding.UTF8.GetString(decryptedBuffer);

            // string -> object
            var message = JsonUtility.FromJson(jsonMessage, dispatchInfo.messageType);
            if (message == null)
            {
                parsingResult.parsingResultEnum = ParsingResult.ParsingResultEnum.PARSING_ERROR;
                return;
            }

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