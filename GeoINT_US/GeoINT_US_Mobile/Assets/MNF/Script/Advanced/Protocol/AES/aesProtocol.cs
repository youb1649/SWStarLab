using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MNF;
using MNF.Crypt;

namespace MNF_Common
{
    public static class AESKey
    {
        // 32 bytes
        public static string KEY = "Y+3xQDLPWalRKK3U/JuabsJNnuEO91zRiOH5gjgOqck=";
        public static string IV = "15CV1/ZOnVI3rY4wk4INBg==";

        public static bool TestAES()
        {
            try
            {
                for (int i = 0; i < 100; ++i)
                {
                    AESRef aes = new AESRef();
                    aes.setKey(AESKey.KEY, AESKey.IV);

                    var ecshoPacket = new AESBinaryMessageDefine.PACK_CS_AES_ECHO();
                    var ser = MarshalHelper.RawSerialize(ecshoPacket);
                    byte[] en = aes.encrypt(ser);
                    byte[] de = aes.decrypt(en);
                    MarshalHelper.RawDeSerialize(de, typeof(AESBinaryMessageDefine.PACK_CS_AES_ECHO));
                }
                return true;
            }
            catch (Exception e)
            {
#if !NETFX_CORE
                Console.WriteLine(e);
#endif
                return false;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class AESBinaryMessageHeader
    {
        [MarshalAs(UnmanagedType.I2)]
        public short messageSize;

        [MarshalAs(UnmanagedType.U2)]
        public ushort messageID;

        [MarshalAs(UnmanagedType.U8)]
        public UInt64 checksum1;

        [MarshalAs(UnmanagedType.U8)]
        public UInt64 checksum2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class AESJsonMessageHeader
    {
        [MarshalAs(UnmanagedType.I2)]
        public short messageSize;

        [MarshalAs(UnmanagedType.U2)]
        public ushort messageID;

        [MarshalAs(UnmanagedType.U8)]
        public UInt64 checksum1;

        [MarshalAs(UnmanagedType.U8)]
        public UInt64 checksum2;
    }

    public partial class AESBinaryMessageDefine
    {
        public enum ENUM_CS_
        {
            CS_AES_ECHO = 567,
        }
        public enum ENUM_SC_
        {
            SC_AES_ECHO = 678,
        }

        #region CS
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public class PACK_CS_AES_ECHO
        {
            [MarshalAs(UnmanagedType.Bool)]
            public bool boolField;

            [MarshalAs(UnmanagedType.I4)]
            public int intField;

            [MarshalAs(UnmanagedType.I8)]
            public Int64 int64Field;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string stringField;

            public PACK_CS_AES_ECHO()
            {
                boolField = true;
                intField = 1;
                int64Field = 2;
                stringField = "1234567890#abcdefghijklmnopqrstuvwxyz";
            }
        }
        #endregion

        #region SC
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public class PACK_SC_AES_ECHO
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string stringField = "";

            [MarshalAs(UnmanagedType.I8)]
            public Int64 int64Field = 0;

            [MarshalAs(UnmanagedType.I4)]
            public int intField = 0;

            [MarshalAs(UnmanagedType.Bool)]
            public bool boolField;

            public PACK_SC_AES_ECHO()
            {
                boolField = false;
                intField = 0;
                int64Field = 0;
                stringField = "";
            }
        }
        #endregion
    }

    public partial class AESJsonMessageDefine
    {
        public enum ENUM_CS_
        {
            CS_AES_JSON_ECHO = 345,
        }
        public enum ENUM_SC_
        {
            SC_AES_JSON_ECHO = 234,
        }

        [System.Serializable]
        public class PACK_CS_AES_JSON_ECHO
        {
            public List<Sandwich> sandwiches = null;
        }

        [System.Serializable]
        public class PACK_SC_AES_JSON_ECHO
        {
            public List<Sandwich> sandwiches = null;
        }
    }
}