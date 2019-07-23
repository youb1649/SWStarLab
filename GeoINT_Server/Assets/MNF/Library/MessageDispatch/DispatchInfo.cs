using System;

namespace MNF
{
    public delegate int onDispatch<T>(T session, object message) where T : new();
    public delegate int onCustomDispatch<T>(T message) where T : new();

    public class DispatchInfo
    {
        public Delegate dispatcher;
        public Type messageType;

        public DispatchInfo(Delegate messageDispatch, Type messageType)
        {
            dispatcher = messageDispatch;
            this.messageType = messageType;
        }

        public DispatchInfo(Delegate messageDispatch)
        {
            dispatcher = messageDispatch;
            messageType = null;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", dispatcher, messageType);
        }
    }
}
