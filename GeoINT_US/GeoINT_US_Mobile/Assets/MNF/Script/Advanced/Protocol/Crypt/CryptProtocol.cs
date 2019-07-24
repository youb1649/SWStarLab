using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MNF;

namespace MNF_Common
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class CryptBinaryMessageHeader
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
    public class CryptJsonMessageHeader
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

    public class CryptBinaryMessageDefine
    {
        public enum ENUM_CS_
        {
            CS_CRYPT_ECHO = 567,
        }
        public enum ENUM_SC_
        {
            SC_CRYPT_ECHO = 678,
        }

        #region CS
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public class PACK_CS_CRYPT_ECHO
        {
            [MarshalAs(UnmanagedType.Bool)]
            public bool boolField;

            [MarshalAs(UnmanagedType.I4)]
            public int intField;

            [MarshalAs(UnmanagedType.I8)]
            public Int64 int64Field;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string stringField;

            public PACK_CS_CRYPT_ECHO()
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
        public class PACK_SC_CRYPT_ECHO
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string stringField = "";

            [MarshalAs(UnmanagedType.I8)]
            public Int64 int64Field = 0;

            [MarshalAs(UnmanagedType.I4)]
            public int intField = 0;

            [MarshalAs(UnmanagedType.Bool)]
            public bool boolField;

            public PACK_SC_CRYPT_ECHO()
            {
                boolField = false;
                intField = 0;
                int64Field = 0;
                stringField = "";
            }
        }
        #endregion
    }

    public class CryptJsonMessageDefine
    {
        public enum ENUM_CS_
        {
            CS_CRYPT_JSON_ECHO = 345,
        }
        public enum ENUM_SC_
        {
            SC_CRYPT_JSON_ECHO = 234,
        }

        [System.Serializable]
        public class PACK_CS_CRYPT_JSON_ECHO
        {
            public List<Sandwich> sandwiches = null;
        }

        [System.Serializable]
        public class PACK_SC_CRYPT_JSON_ECHO
        {
            public List<Sandwich> sandwiches = null;
        }
    }
}