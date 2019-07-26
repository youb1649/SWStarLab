
public class MNFClientManagerBasicMessageDefine
{
    public enum CS
    {
        Hi_Server,
        Hello_Server,
    }

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
