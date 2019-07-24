using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MNF_Common
{
    [Serializable]
    public class Sandwich
    {
        public string name;
        public string bread;
        public float price;
        public List<string> ingredients = new List<string>();

        static Sandwich CreateSandwich(int seed)
        {
            var sandwich = new Sandwich();
            sandwich.name = string.Format("name_{0}", seed);
            sandwich.bread = string.Format("bread_{0}", seed);
            sandwich.price = seed;
            for (int i = 0; i < seed; ++i)
                sandwich.ingredients.Add(string.Format("ingredients_{0}", i + seed));
            return sandwich;
        }

        static public List<Sandwich> CreateSandwichList(int seed)
        {
            var echosandwichList = new List<Sandwich>();
            for (int i = 0; i < seed; ++i)
                echosandwichList.Add(CreateSandwich(i + seed));
            return echosandwichList;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct POINT
    {
        [MarshalAs(UnmanagedType.U4)]
        public int x;
        [MarshalAs(UnmanagedType.U4)]
        public int y;
    }

    public partial class BinaryMessageDefine
    {
        public enum ENUM_CS_
        {
            CS_ECHO = 123,
            CS_HEARTBEAT_RES,   // client sends heartbeat res to server. (2)
        }
        public enum ENUM_SC_
        {
            SC_ECHO = 234,
            SC_HEARTBEAT_REQ,  // server sends heartbeat req to client. (1)
        }

        #region CS
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public class PACK_CS_ECHO
        {
            [MarshalAs(UnmanagedType.Bool)]
            public bool boolField;

            [MarshalAs(UnmanagedType.I4)]
            public int intField;

            [MarshalAs(UnmanagedType.I8)]
            public Int64 int64Field;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public int[] intArrayField;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string stringField;

            public POINT structField;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public POINT[] structArrayField;

            public PACK_CS_ECHO()
            {
                boolField = true;
                intField = 1;
                int64Field = 2;
                intArrayField = new int[10];
                for (int i = 0; i < intArrayField.Length; ++i)
                    intArrayField[i] = i;
                stringField = "1234567890#abcdefghijklmnopqrstuvwxyz";
                structField = new POINT();
                structField.x = 3;
                structField.y = 4;
                structArrayField = new POINT[10];
                for (int i = 0; i < structArrayField.Length; ++i)
                {
                    structArrayField[i] = new POINT();
                    structArrayField[i].x = i;
                    structArrayField[i].y = i + 1;
                }
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public class PACK_CS_HEARTBEAT_RES
        {
            [MarshalAs(UnmanagedType.I4)]
            public int tickCount;

            public PACK_CS_HEARTBEAT_RES()
            {
                tickCount = 0;
            }
        }
        #endregion

        #region SC
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public class PACK_SC_ECHO
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public POINT[] structArrayField;

            public POINT structField;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string stringField = "";

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public int[] intArrayField;

            [MarshalAs(UnmanagedType.I8)]
            public Int64 int64Field;

            [MarshalAs(UnmanagedType.I4)]
            public int intField;

            [MarshalAs(UnmanagedType.Bool)]
            public bool boolField;

            public PACK_SC_ECHO()
            {
                boolField = false;
                intField = 0;
                int64Field = 0;
                intArrayField = new int[10];
                for (int i = 0; i < intArrayField.Length; ++i)
                    intArrayField[i] = 0;
                stringField = "";
                structField = new POINT();
                structField.x = 0;
                structField.x = 0;
                structArrayField = new POINT[10];
                for (int i = 0; i < structArrayField.Length; ++i)
                {
                    structArrayField[i] = new POINT();
                    structArrayField[i].x = 0;
                    structArrayField[i].y = 0;
                }
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public class PACK_SC_HEARTBEAT_REQ
        {
            [MarshalAs(UnmanagedType.I4)]
            public int tickCount;

            public PACK_SC_HEARTBEAT_REQ()
            {
                tickCount = 0;
            }
        }
        #endregion
    }

    public partial class BinaryChatMessageDefine
    {
        public enum ENUM_CS_
        {
            CS_SEND_CHAT_MESSAGE = 123,
            CS_BROADCAST_CHAT_MESSAGE,
        }
        public enum ENUM_SC_
        {
            SC_SEND_CHAT_MESSAGE = 234,
            SC_BROADCAST_CHAT_MESSAGE,
        }

        #region CS
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public class PACK_CS_SEND_CHAT_MESSAGE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string stringField = "";
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public class PACK_CS_BROADCAST_CHAT_MESSAGE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string stringField = "";
        }
        #endregion

        #region SC
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public class PACK_SC_SEND_CHAT_MESSAGE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string stringField = "";
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public class PACK_SC_BROADCAST_CHAT_MESSAGE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string stringField = "";
        }
        #endregion
    }

    public partial class JsonMessageDefine
    {
        public enum ENUM_CS_
        {
            CS_JSON_ECHO = 345,
            CS_JSON_HEARTBEAT_RES,  // client sends heartbeat res to server. (2)
        }
        public enum ENUM_SC_
        {
            SC_JSON_ECHO = 234,
            SC_JSON_HEARTBEAT_REQ,  // server sends heartbeat req to client. (1)
        }

        [Serializable]
        public class PACK_CS_JSON_ECHO
        {
            public List<Sandwich> sandwiches = null;
        }

        [Serializable]
        public class PACK_SC_JSON_ECHO
        {
            public List<Sandwich> sandwiches = null;
        }

        [Serializable]
        public class PACK_CS_JSON_HEARTBEAT_RES
        {
            public int tickCount;
        }

        [Serializable]
        public class PACK_SC_JSON_HEARTBEAT_REQ
        {
            public int tickCount;
        }
    }

    public class FileTransMessageDefine
    {
        public const int MaxSize = 1024 * 50;

        public enum ENUM_CS_
        {
        }
        public enum ENUM_SC_
        {
            SC_FILE_TRANS_START,
            SC_FILE_TRANS_SEND,
            SC_FILE_TRANS_END,
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public class PACK_SC_FILE_TRANS_START
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string fileName = "";

            [MarshalAs(UnmanagedType.I4)]
            public int fileSize;

            public PACK_SC_FILE_TRANS_START()
            {
                fileName = "";
                fileSize = 0;
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public class PACK_SC_FILE_TRANS_SEND
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxSize)]
            public byte[] binary;

            [MarshalAs(UnmanagedType.I4)]
            public int sendSize;

            public PACK_SC_FILE_TRANS_SEND()
            {
                binary = new byte[MaxSize];
                sendSize = 0;
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public class PACK_SC_FILE_TRANS_END
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string fileName = "";

            public PACK_SC_FILE_TRANS_END()
            {
                fileName = "";
            }
        }
    }

    public class ScreenShareMessageDefine
    {
        public const int MaxImageSize = 1024 * 256;

        public enum ENUM_CS_
        {
        }
        public enum ENUM_SC_
        {
            SC_SCREEN_SHARE,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public class PACK_SC_SCREEN_SHARE
        {
            // message max size 200kb
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxImageSize)]
            public byte[] binary;

            [MarshalAs(UnmanagedType.I4)]
            public int sendSize;

            public PACK_SC_SCREEN_SHARE()
            {
                // message max size 200kb
                binary = new byte[MaxImageSize];
                sendSize = 0;
            }
        }
    }

    public class LoginMessageDefine
    {
        public enum ENUM_CS_
        {
            CS_REGIST,
            CS_LOGIN,
        }
        public enum ENUM_SC_
        {
            SC_REGIST,
            SC_LOGIN,
        }

        [Serializable]
        public class PACK_CS_REGIST
        {
            public string id;
            public string pwd;
        }

        [Serializable]
        public class PACK_CS_LOGIN
        {
            public string id;
            public string pwd;
        }

        [Serializable]
        public class PACK_SC_REGIST
        {
            public bool isSuccess;
            public string id;
            public string pwd;
        }

        [Serializable]
        public class PACK_SC_LOGIN
        {
            public bool isSuccess;
            public string id;
            public string pwd;
            public int db_idx;
        }
    }

    public class VideoPlayMessageDefine
    {
        public enum ENUM_CS_
        {
            CS_PING,
        }

        public enum ENUM_SC_
        {
            SC_PONG,
            SC_VIDEO_CONTROL,
        }

        public enum VIDEO_CONTROL
        {
            PLAY,
            PAUSE,
            STOP,
        }

        [Serializable]
        public class PACK_SC_VIDEO_CONTROL
        {
            public VIDEO_CONTROL control;
            public long reservedTime;
            public double playTime;
        }

        [Serializable]
        public class PACK_CS_PING
        {
        }

        [Serializable]
        public class PACK_SC_PONG
        {
        }
    }

    public class MultiChatMessageDefine
    {
        public enum MESSAGES
        {
            CHAT,
        }

        [Serializable]
        public class PACK_CHAT
        {
            public string nickname;
            public string chat;
        }
    }

    public class IPPulseMessageDefine
    {
        public enum MESSAGES
        {
            CHAT,
        }

        [Serializable]
        public class PACK_CHAT
        {
            public string nickname;
            public string chat;
        }
    }
}