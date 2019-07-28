﻿public class MNFServerManagerBasicMessageDefine
{
    public enum CS
    {
        CS_JoystickPosition,
        CS_Num,
    }

    public enum SC
    {
        SC_JoystickPosition,
        SC_Num,
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
