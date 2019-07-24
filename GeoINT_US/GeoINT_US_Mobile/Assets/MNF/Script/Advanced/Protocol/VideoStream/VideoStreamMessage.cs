using System;
using MNF;
using MNF.Message;

namespace MNF_Common
{
    static class VideoStreamMessageBuffer
    {
        public static int MaxMessageSize()
        {
            // 1024 kbytes
            return 1024 * 1024 * 100;
        }
    }

    public class VideoStreamMessageSerializer : Serializer<VideoStreamMessageHeader>
    {
        public VideoStreamMessageSerializer() : base(VideoStreamMessageBuffer.MaxMessageSize())
        {
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

            VideoStreamMessageHeader messageHeader = MessageHeader;
            messageHeader.messageSize = serializedSize;
            messageHeader.messageID = (ushort)messageID;
            MarshalHelper.RawSerialize(
                messageHeader
                , GetSerializedBuffer()
                , 0
                , ref serializedSize);
            SerializedLength += serializedSize;
        }
    }

    public class VideoStreamMessageDeserializer : Deserializer<VideoStreamMessageHeader>
    {
        IntPtr marshalAllocatedBuffer;
        int marshalAllocatedBufferSize = 0;

        public VideoStreamMessageDeserializer() : base(VideoStreamMessageBuffer.MaxMessageSize())
        {
            marshalAllocatedBufferSize = VideoStreamMessageBuffer.MaxMessageSize();
            marshalAllocatedBuffer = MarshalHelper.AllocGlobalHeap(marshalAllocatedBufferSize);
        }

        ~VideoStreamMessageDeserializer()
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

            int messageBodySize = BitConverter.ToInt32(SerializedBuffer, 0);
			int messageSize = messageBodySize + SerializedHeaderSize;
            int messageId = BitConverter.ToInt16(SerializedBuffer, 4);

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

            object message = MarshalHelper.RawDeSerialize(
                SerializedBuffer
                , dispatchInfo.messageType
                , 0
                , ref marshalAllocatedBuffer
                , ref marshalAllocatedBufferSize);
            if (message == null)
                throw new Exception(string.Format("Dispatcher({0}), Expect packet size({1})",
                    dispatchInfo, MarshalHelper.GetManagedDataSize(dispatchInfo.messageType)));

            parsingResult.parsingResultEnum = ParsingResult.ParsingResultEnum.PARSING_COMPLETE;
            parsingResult.dispatcher = dispatchInfo.dispatcher;
            parsingResult.message = message;
            parsingResult.messageSize = messageSize;

            // pop dispatched message size
            tcpSession.RecvCircularBuffer.pop(messageSize);
        }
    }

	public class VideoStreamSession : TCPSession<VideoStreamMessageSerializer, VideoStreamMessageDeserializer>
	{
	}
}