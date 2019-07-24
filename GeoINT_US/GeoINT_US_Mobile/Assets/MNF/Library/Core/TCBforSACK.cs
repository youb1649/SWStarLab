using System;
using System.Collections.Generic;

namespace MNF
{
    public class TCBforSACK
    {
        static private int BufferSize = 1024;
        private byte[] marshalBuffer = null;

        #region SendQueue variables
        public object SendLock { get; private set; }
        private CircularBuffer sendQueue = null;
        private byte[] sendQueuePopBuffer = null;

        public int SendableSize { get { return sendQueue.ReadableSize; } }
        public int SendUNA { get; private set; }
        public int SendDatagramSeq { get; private set; }
        public int MaxReSendDatagramQueue { get; set; }

        public SortedList<int, UdpDatagramSend> ReSendDatagramQueue { get; private set; }
        #endregion // SendQueue variables

        #region RecvQueue variables
        public object RecvLock { get; private set; }
        private CircularBuffer recvQueue = null;
        private byte[] recvQueuePopBuffer = null;

        public int ReadableSize { get { return recvQueue.ReadableSize; } }
        public long LastAckSendTick { get; set; }
        public int RecvDatagramSeq { get; private set; }

        public List<int> ToAckDatagrams { get; private set; }
        public List<int> AckedDatagrams { get; private set; }
        public SortedList<int, UdpDatagramRecv> RecvDatagramQueue { get; private set; }
        #endregion // RecvQueue variables

        public TCBforSACK(int queuebufferSize, int sendInitNumber)
        {
            marshalBuffer = new byte[BufferSize];

            SendLock = new object();
            sendQueue = new CircularBuffer(queuebufferSize);
            sendQueuePopBuffer = new byte[BufferSize];
            SendUNA = sendInitNumber;
            ReSendDatagramQueue = new SortedList<int, UdpDatagramSend>();
            RecvDatagramQueue = new SortedList<int, UdpDatagramRecv>();
            SendDatagramSeq = 1;
            RecvDatagramSeq = 1;
            MaxReSendDatagramQueue = 1000;

            RecvLock = new object();
            recvQueue = new CircularBuffer(queuebufferSize);
            recvQueuePopBuffer = new byte[BufferSize];

            ToAckDatagrams = new List<int>();
            AckedDatagrams = new List<int>();
            LastAckSendTick = 0;
        }

        public byte[] marshal<T>(T managedData, ref int marshaledSize)
        {
            MarshalHelper.RawSerialize(managedData, marshalBuffer, ref marshaledSize);
            return marshalBuffer;
        }

        public bool pushToSendQueue(byte[] serializedData, int pushSize)
        {
            if (sendQueue.isFull(pushSize) == true)
                return false;

            return sendQueue.push(serializedData, pushSize);
        }

        public bool makeUdpDatagramSend(UdpDatagramSend udpDatagram, int makeSize, long nowTick, int window)
        {
            udpDatagram.UdpDatagramHeader.setControlBit_PSH();
            udpDatagram.UdpDatagramHeader.seqNumber = SendUNA;
            udpDatagram.UdpDatagramHeader.sendSize = (ushort)makeSize;
            udpDatagram.UdpDatagramHeader.sendTick = nowTick;
            udpDatagram.UdpDatagramHeader.window = (ushort)window;
            udpDatagram.UdpDatagramHeader.datagramSequence = SendDatagramSeq;
            udpDatagram.serializeHeader();

            return sendQueue.readToTargetIndex(udpDatagram.SerializedBuffer, makeSize, UdpDatagram.HeaderSerializedSize);
        }

        public bool tryMakeReliableSend(UdpDatagramSend udpDatagram, int makeSize, long nowTick, int window)
        {
            if ((window == 0) || (makeSize == 0))
                return false;

            if (makeSize > window)
                makeSize = window;

            if ((makeSize + UdpDatagram.HeaderSerializedSize) > udpDatagram.SerializedBuffer.Length)
                makeSize = udpDatagram.SerializedBuffer.Length - UdpDatagram.HeaderSerializedSize;

            if (makeSize > sendQueue.ReadableSize)
                makeSize = sendQueue.ReadableSize;
            
            try
            {
                if (MaxReSendDatagramQueue <= ReSendDatagramQueue.Count)
                    return false;

                if (makeUdpDatagramSend(udpDatagram, makeSize, nowTick, window) == false)
                    throw new Exception(string.Format("sendQueue.readToTargetIndex(makeSize:{0}, headerSerializedSize:{1}) failed",
                        makeSize, UdpDatagram.HeaderSerializedSize));

                if (sendQueue.pop(makeSize) == false)
                    throw new Exception(string.Format("sendQueue.pop({0}) failed", makeSize));
                
                if (pushSentUdpDatagram(udpDatagram) == false)
                    throw new Exception(string.Format("pushSentUdpDatagram({0}) failed", SendDatagramSeq));

                ++SendDatagramSeq;
                SendUNA += makeSize;

                return true;
            }
            catch (Exception e)
            {
                LogManager.Instance.WriteException(e, "tryMakeReliableSend({0}, {1}, {2}) failed, SendUNA({3})",
                    makeSize, nowTick, window, SendUNA);

                return false;
            }
        }

        public bool pushSentUdpDatagram(UdpDatagramSend udpDatagram)
        {
            if (ReSendDatagramQueue.ContainsKey(udpDatagram.UdpDatagramHeader.datagramSequence) == true)
                return false;

            ReSendDatagramQueue.Add(udpDatagram.UdpDatagramHeader.datagramSequence, udpDatagram);
            return true;
        }

        public UdpDatagramSend ackDatagram(int udpDatagramSequence)
        {
            UdpDatagramSend udpDatagram = null;
            if (ReSendDatagramQueue.TryGetValue(udpDatagramSequence, out udpDatagram) == true)
                ReSendDatagramQueue.Remove(udpDatagramSequence);

            return udpDatagram;
        }

        public bool tryPushRecvReliableDatagram(UdpDatagramRecv udpDatagramRecv)
        {
            try
            {
                int udpDatagramRecvSeq = udpDatagramRecv.UdpDatagramHeader.datagramSequence;
                if (ToAckDatagrams.Contains(udpDatagramRecvSeq) == false)
                    ToAckDatagrams.Add(udpDatagramRecvSeq);

                if (RecvDatagramSeq > udpDatagramRecvSeq)
                    return false;

                if (RecvDatagramQueue.ContainsKey(udpDatagramRecvSeq) == true)
                    return false;

                RecvDatagramQueue.Add(udpDatagramRecvSeq, udpDatagramRecv);

                return true;
            }
            catch (Exception e)
            {
                LogManager.Instance.WriteException(e, "tryPushReliableDatagram({0}, {1}) failed",
                    udpDatagramRecv.UdpDatagramHeader.datagramSequence, udpDatagramRecv.UdpDatagramHeader.sendSize);
                return false;
            }
        }

        public bool tryAssembleReliableDatagram()
        {
            if (RecvDatagramQueue.Count == 0)
                return false;

            try
            {
                bool isIncreaseRecvDatagramSeq = false;
                foreach (var recvDatagram in RecvDatagramQueue)
                {
                    if (RecvDatagramSeq != recvDatagram.Key)
                        break;

                    UdpDatagramHeader udpDatagramHeader = recvDatagram.Value.UdpDatagramHeader;

                    if (recvQueue.capacity() < recvQueue.ReadableSize + udpDatagramHeader.sendSize)
                        return isIncreaseRecvDatagramSeq;

                    if (recvQueue.push(
                            recvDatagram.Value.SerializedBuffer
                            , UdpDatagram.HeaderSerializedSize
                            , udpDatagramHeader.sendSize) == false)
                        return isIncreaseRecvDatagramSeq;

                    ++RecvDatagramSeq;
                    isIncreaseRecvDatagramSeq = true;
                }

                return isIncreaseRecvDatagramSeq;
            }
            catch (Exception e)
            {
                LogManager.Instance.WriteException(e, "tryAssembleReliableDatagram({0}, {1}) failed",
                    RecvDatagramQueue.Count, RecvDatagramSeq);
                return false;
            }
        }

        public bool pushToSendQueue<T>(T managedData, ref int pushedSize)
        {
            var marshaledData = marshal(managedData, ref pushedSize);

            if (sendQueue.isFull(pushedSize) == true)
                return false;

            return sendQueue.push(marshaledData, pushedSize);
        }

        public byte[] readFromSendQueue(int readSize)
        {
            if (sendQueue.ReadableSize < readSize)
                return null;

            if (readSize > sendQueue.ReadableSize)
                return null;

            if (sendQueue.read(sendQueuePopBuffer, readSize) == false)
                return null;

            return sendQueuePopBuffer;
        }

        public bool popFromSendQueue(int popSize)
        {
            return sendQueue.pop(popSize);
        }

        public byte[] readFromRecvQueue(int readSize)
        {
            if (recvQueue.ReadableSize < readSize)
                return null;

            if (recvQueue.read(recvQueuePopBuffer, readSize) == false)
                return null;

            return recvQueuePopBuffer;
        }

        public bool popFromRecvQueue(int popSize)
        {
            return recvQueue.pop(popSize);
        }
    }
}
