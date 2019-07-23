using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MNF;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class BinaryMessageHeader
{
    [MarshalAs(UnmanagedType.I2)]
    public short messageSize;
    [MarshalAs(UnmanagedType.U2)]
    public ushort messageID;
    [MarshalAs(UnmanagedType.I4)]
    public int sequence;
    [MarshalAs(UnmanagedType.I4)]
    public int checkSum;
}

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public class PACK_SC_ECHO
{
    // 12
    public IntPtr header_;

    // 148
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
    public string stringField;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public int[] intArrayField;

    [MarshalAs(UnmanagedType.I4)]
    public int intField;

    [MarshalAs(UnmanagedType.Bool)]
    public bool boolField;

    public PACK_SC_ECHO()
    {
        var tempHeader = new BinaryMessageHeader();
        tempHeader.messageSize = 1;
        tempHeader.messageID = 2;
        tempHeader.sequence = 3;
        tempHeader.checkSum = 4;

        int tempHeaderLen = Marshal.SizeOf(tempHeader);
        header_ = Marshal.AllocCoTaskMem(tempHeaderLen);
        try
        {
            Marshal.StructureToPtr(tempHeader, header_, true);
        }
        finally
        {
            Marshal.FreeCoTaskMem(header_);
        }
        intArrayField = new int[10];
        for (int i = 0; i < intArrayField.Length; ++i)
            intArrayField[i] = i + 1;
        stringField = "abcdefg";

        intField = 5;
        boolField = true;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public class PACK_SC_ECHO_Inher : BinaryMessageHeader
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
    public string stringField;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public int[] intArrayField;

    [MarshalAs(UnmanagedType.I4)]
    public int intField;

    [MarshalAs(UnmanagedType.Bool)]
    public bool boolField;

    public PACK_SC_ECHO_Inher()
    {
        messageSize = 1;
        messageID = 2;
        sequence = 3;
        checkSum = 4;

        intArrayField = new int[10];
        for (int i = 0; i < intArrayField.Length; ++i)
            intArrayField[i] = i + 1;
        stringField = "abcdefg";

        intField = 5;
        boolField = true;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BinaryMessageHeaderStr
{
    [MarshalAs(UnmanagedType.I2)]
    public short messageSize;
    [MarshalAs(UnmanagedType.U2)]
    public ushort messageID;
    [MarshalAs(UnmanagedType.I4)]
    public int sequence;
    [MarshalAs(UnmanagedType.I4)]
    public int checkSum;
}

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public class PACK_SC_ECHO_Has
{
    BinaryMessageHeaderStr binaryMessageHeaderStr;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
    public string stringField;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public int[] intArrayField;

    [MarshalAs(UnmanagedType.I4)]
    public int intField;

    [MarshalAs(UnmanagedType.Bool)]
    public bool boolField;

    public PACK_SC_ECHO_Has()
    {
        binaryMessageHeaderStr = new BinaryMessageHeaderStr();
        binaryMessageHeaderStr.messageSize = 1;
        binaryMessageHeaderStr.messageID = 2;
        binaryMessageHeaderStr.sequence = 3;
        binaryMessageHeaderStr.checkSum = 4;

        intArrayField = new int[10];
        for (int i = 0; i < intArrayField.Length; ++i)
            intArrayField[i] = i + 1;
        stringField = "abcdefg";

        intField = 5;
        boolField = true;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public class PACK_SC_ECHO_Has_Inhert : BinaryMessageHeader
{
    BinaryMessageHeaderStr binaryMessageHeaderStr;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public BinaryMessageHeaderStr[] binaryMessageHeaderStrList;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
    public string stringField;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public int[] intArrayField;

    [MarshalAs(UnmanagedType.I4)]
    public int intField;

    [MarshalAs(UnmanagedType.Bool)]
    public bool boolField;

    public PACK_SC_ECHO_Has_Inhert()
    {
        messageSize = 1;
        messageID = 2;
        sequence = 3;
        checkSum = 4;

        binaryMessageHeaderStr = new BinaryMessageHeaderStr();
        binaryMessageHeaderStr.messageSize = 1;
        binaryMessageHeaderStr.messageID = 2;
        binaryMessageHeaderStr.sequence = 3;
        binaryMessageHeaderStr.checkSum = 4;

        binaryMessageHeaderStrList = new BinaryMessageHeaderStr[10];
        for (int i = 0; i < binaryMessageHeaderStrList.Length; ++i)
        {
            binaryMessageHeaderStrList[i] = new BinaryMessageHeaderStr();
            binaryMessageHeaderStrList[i].messageSize = 1;
            binaryMessageHeaderStrList[i].messageID = 2;
            binaryMessageHeaderStrList[i].sequence = 3;
            binaryMessageHeaderStrList[i].checkSum = 4;
        }

        intArrayField = new int[10];
        for (int i = 0; i < intArrayField.Length; ++i)
            intArrayField[i] = i + 1;
        stringField = "abcdefg";

        intField = 5;
        boolField = true;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct stVector3
{
    [MarshalAs(UnmanagedType.R4)]
    public float x;
    [MarshalAs(UnmanagedType.R4)]
    public float y;
    [MarshalAs(UnmanagedType.R4)]
    public float z;

    public stVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public class VectorCollection
{
    public Vector2 v2;
    public Vector3 v3;
    public Vector4 v4;
    public stVector3 stV3;

    public VectorCollection()
    {
        v2 = new Vector2();
        v2.x = 1;
        v2.y = 2;
        v3 = new Vector3();
        v3.x = 3;
        v3.y = 4;
        v3.z = 5;
        v4 = new Vector4();
        v4.x = 6;
        v4.y = 7;
        v4.z = 8;
        v4.w = 9;
        stV3 = new stVector3();
        stV3.x = 10;
        stV3.y = 11;
        stV3.z = 12;
    }
}

public interface IJsonMessageHeader_TEST
{
    short getMessageID();
}

[System.Serializable]
public class PACK_LOG_SERVER_CONNECT : IJsonMessageHeader_TEST
{
    public string fromID;
    public string fromInfo;

    short IJsonMessageHeader_TEST.getMessageID()
    {
        return (short)0;
    }
}

public class IL2CPP_Test : MonoBehaviour {
    public Text output;

    void outputLog<T>(ref int outputCount, T log, int size)
    {
        ++outputCount;
        var resultStr = outputCount + " : " + log + ", " + size + "\n";
        output.text += resultStr;
        Debug.Log(resultStr);
    }

	// Use this for initialization
	void Start () {
        int outputCount = 0;

        try
        {
            byte[] binarySer = null;
            {
                var binary = new BinaryMessageHeader();
                binarySer = MarshalHelper.RawSerialize(binary);
                outputLog(ref outputCount, binarySer, Utility.Sizeof(ref binary));
            }
            var binaryDeser = MarshalHelper.RawDeSerialize(binarySer, typeof(BinaryMessageHeader));
            outputLog(ref outputCount, binaryDeser, Utility.Sizeof(ref binaryDeser));
        }
        catch(Exception e)
        {
            outputLog(ref outputCount, e, 0);
        }

        try
        {
            byte[] echoSer = null;
            {
                var echo = new PACK_SC_ECHO();
                echoSer = MarshalHelper.RawSerialize(echo);
                outputLog(ref outputCount, echoSer, Utility.Sizeof(ref echo));
            }
            var echoDeSer = MarshalHelper.RawDeSerialize(echoSer, typeof(PACK_SC_ECHO));
            outputLog(ref outputCount, echoDeSer, Utility.Sizeof(ref echoDeSer));
        }
        catch (Exception e)
        {
            outputLog(ref outputCount, e, 0);
        }

        try
        {
            byte[] echoSer = null;
            {
                var echo = new PACK_SC_ECHO_Inher();
                echoSer = MarshalHelper.RawSerialize(echo);
                outputLog(ref outputCount, echoSer, Utility.Sizeof(ref echo));
            }
            var echoDeSer = MarshalHelper.RawDeSerialize(echoSer, typeof(PACK_SC_ECHO_Inher));
            outputLog(ref outputCount, echoDeSer, Utility.Sizeof(ref echoDeSer));
        }
        catch (Exception e)
        {
            outputLog(ref outputCount, e, 0);
        }

        try
        {
            byte[] echoSer = null;
            {
                var echo = new PACK_SC_ECHO_Has();
                echoSer = MarshalHelper.RawSerialize(echo);
                outputLog(ref outputCount, echoSer, Utility.Sizeof(ref echo));
            }
            var echoDeSer = MarshalHelper.RawDeSerialize(echoSer, typeof(PACK_SC_ECHO_Has));
            outputLog(ref outputCount, echoDeSer, Utility.Sizeof(ref echoDeSer));
        }
        catch (Exception e)
        {
            outputLog(ref outputCount, e, 0);
        }

        try
        {
            byte[] echoSer = null;
            {
                var echo = new PACK_SC_ECHO_Has_Inhert();
                echoSer = MarshalHelper.RawSerialize(echo);
                outputLog(ref outputCount, echoSer, Utility.Sizeof(ref echo));
            }
            var echoDeSer = MarshalHelper.RawDeSerialize(echoSer, typeof(PACK_SC_ECHO_Has_Inhert));
            outputLog(ref outputCount, echoDeSer, Utility.Sizeof(ref echoDeSer));
        }
        catch (Exception e)
        {
            outputLog(ref outputCount, e, 0);
        }

        try
        {
            var con = new MNF_Common.JsonMessageDefine.PACK_CS_JSON_ECHO();
            con.sandwiches = MNF_Common.Sandwich.CreateSandwichList(3);
            var jsonData = JsonUtility.ToJson(con);
            outputLog(ref outputCount, jsonData, 0);
            var message = JsonUtility.FromJson(jsonData, typeof(MNF_Common.JsonMessageDefine.PACK_CS_JSON_ECHO));
            outputLog(ref outputCount, message, 0);
        }
        catch (Exception e)
        {
            outputLog(ref outputCount, e, 0);
        }

        try
        {
            var vector = new stVector3();
            byte[] vectorSer = MarshalHelper.RawSerialize(vector);
            var vectorDes = MarshalHelper.RawDeSerialize(vectorSer, typeof(stVector3));
            outputLog(ref outputCount, vector, Utility.Sizeof(ref vectorDes));
        }
        catch (Exception e)
        {
            outputLog(ref outputCount, e, 0);
        }

        try
        {
            var vector = new Vector3();
            byte[] vectorSer = MarshalHelper.RawSerialize(vector);
            var vectorDes = MarshalHelper.RawDeSerialize(vectorSer, typeof(Vector3));
            outputLog(ref outputCount, vector, Utility.Sizeof(ref vectorDes));
        }
        catch (Exception e)
        {
            outputLog(ref outputCount, e, 0);
        }

        try
        {
            var vectorCol = new VectorCollection();
            byte[] vectorSer = MarshalHelper.RawSerialize(vectorCol);
            var vectorDes = MarshalHelper.RawDeSerialize(vectorSer, typeof(VectorCollection));
            outputLog(ref outputCount, vectorCol, Utility.Sizeof(ref vectorDes));
        }
        catch (Exception e)
        {
            outputLog(ref outputCount, e, 0);
        }

        try
        {
            var packet = new UdpMessageDefine.PACK_SPAWN_OBJECT();
            byte[] vectorSer = MarshalHelper.RawSerialize(packet);
            var vectorDes = MarshalHelper.RawDeSerialize(vectorSer, typeof(UdpMessageDefine.PACK_SPAWN_OBJECT));
            outputLog(ref outputCount, packet, Utility.Sizeof(ref vectorDes));
        }
        catch (Exception e)
        {
            outputLog(ref outputCount, e, 0);
        }
    }

	void OnApplicationQuit()
	{
		Debug.Log("Application ending after " + Time.time + " seconds");
		TcpHelper.Instance.Stop();
		LogManager.Instance.Release();
	}
}