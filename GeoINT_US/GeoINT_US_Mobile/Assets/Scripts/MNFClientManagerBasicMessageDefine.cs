
public class MNFClientManagerBasicMessageDefine
{
    public enum CS
    {
        CS_JoystickPosition,
    }

    public enum SC
    {
        SC_JoystickPosition,
    }

    [System.Serializable]
    public class PACK_CS_JoystickPosition
    {
        public string msg;
    }

    [System.Serializable]
    public class PACK_SC_JoystickPosition
    {
        public string msg;
    }
}
