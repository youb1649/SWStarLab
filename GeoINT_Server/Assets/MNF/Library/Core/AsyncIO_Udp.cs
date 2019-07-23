using System;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace MNF
{
    // Singleton class
    class AsyncIO_Udp
    {
        internal static bool SendMe<T>(UdpSession session, ref T managedData)
        {
            var udpMessageHeader = managedData as UdpMessageHeader;
            var dispatchInfo = session.DispatchHelper.TryGetMessageDispatch(udpMessageHeader.messageID);

            UdpMessageInfo udpMessageInfo = new UdpMessageInfo();
            udpMessageInfo.UdpMessage = managedData;
            udpMessageInfo.RemoteIP = new IPEndPoint(0, 0);
            udpMessageInfo.IsSendMe = true;

            var pushMessage = session.DispatchHelper.CreateMessage(session, dispatchInfo.dispatcher, udpMessageInfo);
            return DispatcherCollection.Instance.PushMessage(DISPATCH_TYPE.DISPATCH_UDP, pushMessage);
        }

		internal static bool UnreliableSendTo<T>(UdpSession session, EndPoint endPoint, ref T managedData)
        {
			var udpDatagram = session.UdpDatagramSendPool.alloc(session);
            try
            {
                udpDatagram.UdpDatagramHeader.setUnreliable();
                udpDatagram.UdpDatagramHeader.seqNumber = session.UnreliableSequence++;
                udpDatagram.serializeDatagram(ref managedData);
                udpDatagram.IsSending = true;
				udpDatagram.EndPoint = (IPEndPoint)endPoint;

                SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
                sendEventArgs.SetBuffer(udpDatagram.SerializedBuffer, 0, udpDatagram.SerializedSize);
                sendEventArgs.RemoteEndPoint = endPoint;
                sendEventArgs.UserToken = udpDatagram;
                sendEventArgs.Completed += UnreliableSendToCallback;

                session.Socket.SendToAsync(sendEventArgs);
                bool willRaiseEvent = session.Socket.SendAsync(sendEventArgs);
                if (willRaiseEvent == false)
                {
                    UnreliableSendToCallback(session.Socket, sendEventArgs);
                }

                return true;
            }
            catch (Exception e)
            {
                LogManager.Instance.WriteException(e, "UdpSession({0}) packet({1}) unreliable send to ({2}) begin failed",
                    session.ToString(), managedData.ToString(), endPoint.ToString());
                udpDatagram.IsSending = false;
                session.UdpDatagramSendPool.free(udpDatagram);
                return false;
            }
        }
        static void UnreliableSendToCallback(object sender, SocketAsyncEventArgs e)
        {
			var udpDatagram = e.UserToken as UdpDatagramSend;
            var session = udpDatagram.LinkedSession;
            try
            {
                udpDatagram.IsSending = false;
                session.UdpDatagramSendPool.free(udpDatagram);
                
                // Complete sending the readData to the remote device.
                int bytesSent = e.BytesTransferred;

                if (LogManager.Instance.IsWriteLog(ENUM_LOG_TYPE.LOG_TYPE_SYSTEM_DEBUG) == true)
                {
                    LogManager.Instance.WriteSystemDebug("UdpSession({0}) sent({1}) bytes to server",
                        session.EndPoint.ToString(), bytesSent);
                }

                if (bytesSent == 0)
                {
                    LogManager.Instance.WriteError("UdpSession({0}) send Zero byte", session.EndPoint.ToString());
                }
            }
            catch (Exception exception)
            {
				LogManager.Instance.WriteException(exception, "UdpSession({0}) send callback proc failed",
                    e.GetType().Name);
            }
        }

        internal static bool RequestReliableSendTo<T>(UdpSession session, EndPoint endPoint, ref T managedData)
        {
            try
            {
                TCBforSACK tcpForSACK = null;
                session.tryGetTCBforSACK(endPoint, out tcpForSACK);

                lock (tcpForSACK.SendLock)
                {
                    int pushedSize = 0;
                    if (tcpForSACK.pushToSendQueue(managedData, ref pushedSize) == false)
                        return false;
                }

                UdpHelper.Instance.NotifySendThread();

                return true;
            }
            catch (Exception e)
            {
                LogManager.Instance.WriteException(e, "UdpSession({0}) packet({1}) reliable send to ({2}) begin failed",
                    session.ToString(), managedData.ToString(), endPoint.ToString());
                return false;
            }
        }

        internal static bool ReliableSendTo(UdpSession session, EndPoint endPoint, TCBforSACK tcbForSACK)
        {
            Debug.Assert(session != null);

            long nowTick = DateTime.Now.Ticks;
            int makeSize = 1024;
            int window = 1024;

            UdpDatagramSend udpDatagram = null;
            try
            {
                lock (tcbForSACK.SendLock)
                {
                    udpDatagram = session.UdpDatagramSendPool.alloc(session);

                    if (tcbForSACK.tryMakeReliableSend(udpDatagram, makeSize, nowTick, window) == false)
                    {
                        session.UdpDatagramSendPool.free(udpDatagram);
                        return false;
                    }

                    udpDatagram.IsSending = true;
					udpDatagram.EndPoint = (IPEndPoint)endPoint;
					
                    SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
                    sendEventArgs.SetBuffer(udpDatagram.SerializedBuffer, 0, udpDatagram.SerializedSize);
                    sendEventArgs.RemoteEndPoint = endPoint;
                    sendEventArgs.UserToken = udpDatagram;
                    sendEventArgs.Completed += ReliableSendToCallback;

                    session.Socket.SendToAsync(sendEventArgs);
                    bool willRaiseEvent = session.Socket.SendAsync(sendEventArgs);
                    if (willRaiseEvent == false)
                    {
                        ReliableSendToCallback(session.Socket, sendEventArgs);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                LogManager.Instance.WriteException(e, "UdpSession({0}) reliable send to ({1}) begin failed",
                                session.ToString(), endPoint.ToString());

                udpDatagram.IsSending = false;
                session.UdpDatagramSendPool.free(udpDatagram);

                return false;
            }
        }

        internal static bool ReliableSendAckDatagram(UdpSession session, EndPoint endPoint, TCBforSACK tcbForSACK, long nowTicks)
        {
            // ackTimeout은 RTT로 계산해서 보내자
            var ackTimeout = TimeSpan.TicksPerMillisecond * 30;
            if ((tcbForSACK.LastAckSendTick + ackTimeout) > nowTicks)
                return false;

            tcbForSACK.LastAckSendTick = nowTicks;

            lock (tcbForSACK.RecvLock)
            {
                int maxCopySize = UdpDatagram.SerializedBufferLength - UdpDatagram.HeaderSerializedSize;
                if (maxCopySize % 4 != 0)
                    maxCopySize -= maxCopySize % 4;

                while (tcbForSACK.ToAckDatagrams.Count > 0)
                {
                    var recvDatagrams = tcbForSACK.ToAckDatagrams.ToArray();

                    int copySize = tcbForSACK.ToAckDatagrams.Count * sizeof(int);
                    if (copySize > maxCopySize)
                        copySize = maxCopySize;

					UdpDatagramSend udpDatagram = session.UdpDatagramSendPool.alloc(session);
                    udpDatagram.UdpDatagramHeader.setControlBit_ACK();
                    udpDatagram.UdpDatagramHeader.sendSize = (ushort)copySize;
                    udpDatagram.UdpDatagramHeader.sendTick = nowTicks;
                    udpDatagram.serializeHeader();
                    Buffer.BlockCopy(recvDatagrams, 0, udpDatagram.SerializedBuffer, UdpDatagram.HeaderSerializedSize, copySize);

                    try
                    {
                        udpDatagram.IsSending = true;

                        SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
                        sendEventArgs.SetBuffer(udpDatagram.SerializedBuffer, 0, udpDatagram.SerializedSize);
                        sendEventArgs.RemoteEndPoint = endPoint;
                        sendEventArgs.UserToken = udpDatagram;
                        sendEventArgs.Completed += ReliableSendAckDatagramCallback;

                        session.Socket.SendToAsync(sendEventArgs);
                        bool willRaiseEvent = session.Socket.SendAsync(sendEventArgs);
                        if (willRaiseEvent == false)
                        {
                            ReliableSendAckDatagramCallback(session.Socket, sendEventArgs);
                        }
                    }
                    catch (Exception e)
                    {
                        LogManager.Instance.WriteException(e, "UdpSession({0}) reliable send to ({1}) begin failed",
                            session.ToString(), endPoint.ToString());

                        udpDatagram.IsSending = false;
                        session.UdpDatagramSendPool.free(udpDatagram);

                        return false;
                    }

                    tcbForSACK.ToAckDatagrams.RemoveRange(0, (copySize / sizeof(int)));
                }

                return true;
            }
        }

        internal static bool ReliableReSendTo(UdpSession session, EndPoint endPoint, TCBforSACK tcbForSACK, long nowTicks)
        {
            // resendTimeout은 RTT로 계산해서 보내자
            var resendTimeout = TimeSpan.TicksPerMillisecond * 100;
            int maxResendCount = 30;
            lock (tcbForSACK.SendLock)
            {
                tcbForSACK.AckedDatagrams.Sort();
                for (int i = 0; i < tcbForSACK.AckedDatagrams.Count; ++i)
                {
                    UdpDatagramSend resendUdpDatagram = null;
                    if (tcbForSACK.ReSendDatagramQueue.TryGetValue(tcbForSACK.AckedDatagrams[i], out resendUdpDatagram) == false)
                        continue;

                    if (resendUdpDatagram.IsSending == true)
                    {
                        resendUdpDatagram.UdpDatagramHeader.sendTick = TimeSpan.TicksPerHour + nowTicks;
                        continue;
                    }

                    tcbForSACK.ReSendDatagramQueue.Remove(tcbForSACK.AckedDatagrams[i]);
                    tcbForSACK.AckedDatagrams.RemoveAt(i);
                    session.UdpDatagramSendPool.free(resendUdpDatagram);
                }

                int resendCount = 0;
                foreach (var resendDatagramPair in tcbForSACK.ReSendDatagramQueue)
                {
                    if (resendCount++ >= maxResendCount)
                        break;

                    var resendDatagram = resendDatagramPair.Value;
                    Debug.Assert(resendDatagram.UdpDatagramHeader.sendTick > 0);
                    Debug.Assert(resendDatagram.LinkedSession != null);

                    if ((resendDatagram.UdpDatagramHeader.sendTick + resendTimeout) > nowTicks)
                        break;

                    try
                    {
                        resendDatagram.IsSending = true;

                        SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
                        sendEventArgs.SetBuffer(resendDatagram.SerializedBuffer, 0, resendDatagram.SerializedSize);
                        sendEventArgs.RemoteEndPoint = endPoint;
                        sendEventArgs.UserToken = resendDatagram;
                        sendEventArgs.Completed += ReliableReSendToCallback;

                        session.Socket.SendToAsync(sendEventArgs);
                        bool willRaiseEvent = session.Socket.SendAsync(sendEventArgs);
                        if (willRaiseEvent == false)
                        {
                            ReliableReSendToCallback(session.Socket, sendEventArgs);
                        }
                    }
                    catch (Exception e)
                    {
                        LogManager.Instance.WriteException(e, "UdpSession({0}) reliable send to ({1}) begin failed",
                            session.ToString(), endPoint.ToString());

                        return false;
                    }

                    resendDatagram.UdpDatagramHeader.sendTick = nowTicks;
                }
            }

            return true;
        }

        static void ReliableSendToCallback(object sender, SocketAsyncEventArgs e)
        {
            var udpDatagramSend = e.UserToken as UdpDatagramSend;
            var session = udpDatagramSend.LinkedSession;
            try
            {
                udpDatagramSend.IsSending = false;

                if (session == null)
                    throw new Exception(string.Format("Send session({0}) is invalid", e.GetType().Name));

                // Complete sending the readData to the remote device.
                int bytesSent = e.BytesTransferred;

                if (LogManager.Instance.IsWriteLog(ENUM_LOG_TYPE.LOG_TYPE_SYSTEM_DEBUG) == true)
                {
                    LogManager.Instance.WriteSystemDebug("UdpSession({0}) sent({1}) bytes to server",
                        session.EndPoint.ToString(), bytesSent);
                }

                if (bytesSent == 0)
                {
                    LogManager.Instance.WriteError("UdpSession({0}) send Zero byte", session.EndPoint.ToString());
                }
            }
            catch (Exception exception)
            {
				LogManager.Instance.WriteException(exception, "UdpSession({0}) reliableSendToCallback() failed",
                    udpDatagramSend.LinkedSession);
            }
        }

        static void ReliableReSendToCallback(object sender, SocketAsyncEventArgs e)
        {
			var udpDatagram = e.UserToken as UdpDatagramSend;
            var session = udpDatagram.LinkedSession;
            try
            {
                udpDatagram.IsSending = false;

                if (session == null)
                    throw new Exception(string.Format("Send session({0}) is invalid", e.GetType().Name));

                // Complete sending the readData to the remote device.
                int bytesSent = e.BytesTransferred;

                if (LogManager.Instance.IsWriteLog(ENUM_LOG_TYPE.LOG_TYPE_SYSTEM_DEBUG) == true)
                {
                    LogManager.Instance.WriteSystemDebug("UdpSession({0}) sent({1}) bytes to server",
                        session.EndPoint.ToString(), bytesSent);
                }

                if (bytesSent == 0)
                {
                    LogManager.Instance.WriteError("UdpSession({0}) send Zero byte", session.EndPoint.ToString());
                }
            }
            catch (Exception exception)
            {
				LogManager.Instance.WriteException(exception, "UdpSession({0}) reliableReSendToCallback() failed",
                    udpDatagram.LinkedSession);
            }
        }

        static void ReliableSendAckDatagramCallback(object sender, SocketAsyncEventArgs e)
        {
            var udpUdpDatagram = e.UserToken as UdpDatagramSend;
			var session = udpUdpDatagram.LinkedSession;
            try
            {
                udpUdpDatagram.IsSending = false;

                if (session == null)
                    throw new Exception(string.Format("Send session({0}) is invalid", e.GetType().Name));

                session.UdpDatagramSendPool.free(udpUdpDatagram);

                // Complete sending the readData to the remote device.
                int bytesSent = e.BytesTransferred;

                if (LogManager.Instance.IsWriteLog(ENUM_LOG_TYPE.LOG_TYPE_SYSTEM_DEBUG) == true)
                {
                    LogManager.Instance.WriteSystemDebug("UdpSession({0}) sent({1}) bytes to server",
                        session.EndPoint.ToString(), bytesSent);
                }

                if (bytesSent == 0)
                {
                    LogManager.Instance.WriteError("UdpSession({0}) send Zero byte", session.EndPoint.ToString());
                }
            }
            catch (Exception exception)
            {
				LogManager.Instance.WriteException(exception, "UdpSession({0}) reliableSendAckDatagramCallback() failed",
                    udpUdpDatagram.LinkedSession);
            }
        }

        internal static bool RecvFrom(UdpSession session)
        {
            var udpDatagram = session.UdpDatagramRecvPool.alloc(session);
            try
            {
                SocketAsyncEventArgs recvEventArgs = new SocketAsyncEventArgs();
                recvEventArgs.SetBuffer(udpDatagram.SerializedBuffer, 0, udpDatagram.SerializedBuffer.Length);
                recvEventArgs.RemoteEndPoint = session.EndPoint;
                recvEventArgs.UserToken = udpDatagram;
                recvEventArgs.Completed += RecvFromCallback;

                bool willRaiseEvent = session.Socket.ReceiveFromAsync(recvEventArgs);
                if (willRaiseEvent == false)
                {
                    RecvFromCallback(session.Socket, recvEventArgs);
                }

                return true;
            }
            catch (Exception e)
            {
                LogManager.Instance.WriteException(e, "UdpSession({0}) receive failed", session);
                session.UdpDatagramRecvPool.free(udpDatagram);
                return false;
            }
        }

        static void RecvFromCallback(object sender, SocketAsyncEventArgs e)
        {
            var udpDatagram = e.UserToken as UdpDatagramRecv;
            UdpSession session = udpDatagram.LinkedSession;
            try
            {
                if (udpDatagram == null)
                    throw new Exception(string.Format("Recv udpDatagram({0}) is invalid", e.GetType().Name));

                EndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, 0);
                int bytesRead = e.BytesTransferred;
                if (LogManager.Instance.IsWriteLog(ENUM_LOG_TYPE.LOG_TYPE_SYSTEM_DEBUG) == true)
                {
                    LogManager.Instance.WriteSystemDebug("UdpSession({0}) Recv({1}) bytes from {2}.",
                        session.EndPoint.ToString(), bytesRead, senderEndPoint);
                }

                udpDatagram.deSerializeUdpDatagramHeader();

                switch (udpDatagram.UdpDatagramHeader.datagramType)
                {
                    case (byte)ENUM_UDP_DATAGRAM_TYPE.UDP_DATAGRAM_TYPE_UNRELIABLE:
                        {
                            ProcUnreliableUdpDatagram(session, senderEndPoint, udpDatagram);
                            session.UdpDatagramRecvPool.free(udpDatagram);
                        }
                        break;

                    case (byte)ENUM_UDP_DATAGRAM_TYPE.UDP_DATAGRAM_TYPE_RELIABLE:
                        {
                            if (UdpControllHelper.Instance.IsDrop(UdpControllHelper.ENUM_DROP_TYPE.DROP_TYPE_RECV) == true)
                                return;

                            ProcReliableUdpDatagram(session, senderEndPoint, udpDatagram);
                        }
                        break;
                }
            }
            catch (Exception exception)
            {
                LogManager.Instance.WriteException(exception, "UdpSession({0}) recv callback proc failed", e.GetType().Name);

                var socketException = exception as SocketException;
                if (socketException != null)
                {
                    int errroCode = (int)socketException.SocketErrorCode;
                    if (errroCode == 10054)
                    {
#if !NETFX_CORE
                        session.Socket.Close();
#else
                    session.Socket.Dispose();
#endif
                        session.Socket = UdpHelper.Instance.AllocUdpSocket(session.EndPoint);
                    }
                }
            }
            finally
            {
                RecvFrom(session);
            }
        }

        static void ProcUnreliableUdpDatagram(UdpSession session, EndPoint senderEndPoint, UdpDatagramRecv udpDatagram)
        {
            try
            {
                int messageID = BitConverter.ToInt16(udpDatagram.SerializedBuffer, UdpDatagramHeader.getHeaderSize() + 2);
                DeSerialize(session, messageID, udpDatagram.SerializedBuffer, UdpDatagramHeader.getHeaderSize(), senderEndPoint, udpDatagram);
            }
            catch (Exception e)
            {
                LogManager.Instance.WriteException(e, "Deserialize and dispatch failed");
            }
        }

        static void ProcReliableUdpDatagram(UdpSession session, EndPoint senderEndPoint, UdpDatagramRecv udpDatagram)
        {
            TCBforSACK tcbForSACK = null;
            session.tryGetTCBforSACK(senderEndPoint, out tcbForSACK);
            if (tcbForSACK == null)
                return;

            if (udpDatagram.UdpDatagramHeader.controlbit == (byte)ENUM_UDP_CONTROL_FLAG.UDP_CONTROL_FLAG_ACK)
            {
                if (udpDatagram.UdpDatagramHeader.sendSize == 0)
                    return;

                int ackCount = udpDatagram.UdpDatagramHeader.sendSize / sizeof(int);
                for (int i = 0; i < ackCount; ++i)
                {
                    int beginIndex = UdpDatagram.HeaderSerializedSize + (i * sizeof(int));
                    int ackDatagramSequence = BitConverter.ToInt32(udpDatagram.SerializedBuffer, beginIndex);

                    lock(tcbForSACK.SendLock)
                    {
                        if (tcbForSACK.AckedDatagrams.Contains(ackDatagramSequence) == false)
                            tcbForSACK.AckedDatagrams.Add(ackDatagramSequence);
                    }
                }
                session.UdpDatagramRecvPool.free(udpDatagram);
                return;
            }

            lock(tcbForSACK.RecvLock)
            {
                if (tcbForSACK.tryPushRecvReliableDatagram(udpDatagram) == false)
                    return;

                int prevRecvDatagramSeq = tcbForSACK.RecvDatagramSeq;
                if (tcbForSACK.tryAssembleReliableDatagram() == false)
                    return;

                for (int i = prevRecvDatagramSeq; i < tcbForSACK.RecvDatagramSeq; ++i)
                    tcbForSACK.RecvDatagramQueue.Remove(i);

                while (tcbForSACK.ReadableSize >= UdpMessageHeader.getHeaderSize())
                {
                    var readBuffer = tcbForSACK.readFromRecvQueue(UdpMessageHeader.getHeaderSize());
                    if (readBuffer == null)
                        return;

                    int messageSize = BitConverter.ToInt16(readBuffer, 0);
                    if (messageSize > tcbForSACK.ReadableSize)
                        return;

                    readBuffer = tcbForSACK.readFromRecvQueue(messageSize);
                    if (readBuffer == null)
                        return;

                    int messageID = BitConverter.ToInt16(readBuffer, 2);
                    try
                    {
                        DeSerialize(session, messageID, readBuffer, 0, senderEndPoint, udpDatagram);
                        tcbForSACK.popFromRecvQueue(messageSize);
                    }
                    catch (Exception e)
                    {
                        LogManager.Instance.WriteException(e, "Deserialize and dispatch failed, messageID({0})", messageID);
                    }
                }
            }
        }

        static void DeSerialize(UdpSession session, int messageID, byte[] readBuffer, int beginInex, EndPoint senderEndPoint, UdpDatagramRecv udpDatagram)
        {
            var dispatchInfo = session.DispatchHelper.TryGetMessageDispatch(messageID);
            object message = MarshalHelper.RawDeSerialize(
                readBuffer
                , dispatchInfo.messageType
                , beginInex
                , ref udpDatagram.marshalAllocatedBuffer
                , ref udpDatagram.marshalAllocatedBufferSize);
            if (message == null)
                throw new Exception(string.Format("Dispatcher({0}), Expect packet size({1})",
                    dispatchInfo, MarshalHelper.GetManagedDataSize(dispatchInfo.messageType)));

            UdpMessageInfo udpMessageInfo = new UdpMessageInfo();
            udpMessageInfo.UdpMessage = message;
            udpMessageInfo.RemoteIP = senderEndPoint;

            var pushMessage = session.DispatchHelper.CreateMessage(session, dispatchInfo.dispatcher, udpMessageInfo);
            DispatcherCollection.Instance.PushMessage(DISPATCH_TYPE.DISPATCH_UDP, pushMessage);
        }
    }
}
