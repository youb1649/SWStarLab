public class MNFServerManagerBasicMessageDefine
{
    // The CS enum is the message that the client sends to the server.
    public enum CS
    {
        Hi_Server,
        Hello_Server,
    }

    // The SC enum is the message that the server sends to the client.
    public enum SC
    {
        Hi_Client,
        Hello_Client,
    }

    [System.Serializable]
    public class PACK_Hi_Server
    {
        public string msg;
    }

    [System.Serializable]
    public class PACK_Hello_Server
    {
        public string msg;
    }

    [System.Serializable]
    public class PACK_Hi_Client
    {
        public string msg;
    }

    [System.Serializable]
    public class PACK_Hello_Client
    {
        public string msg;
    }
}
