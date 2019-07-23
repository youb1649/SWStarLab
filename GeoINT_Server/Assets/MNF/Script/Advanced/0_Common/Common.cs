using UnityEngine;

public enum ENUM_MESSAGE_TYPE
{
    MESSAGE_TYPE_BINARY,
    MESSAGE_TYPE_JSON,
}

public struct MessageDispatchInfo
{
    public int dispatchCount;
    public int dispatchSize;

    public int totalDispatchCount;
    public int totalDispatchSize;

    public void init()
    {
        dispatchCount = 0;
        dispatchSize = 0;
    }
}

public static class UnityUtility
{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
	public static readonly string StreamingPath = "file://" + Application.streamingAssetsPath + "/";
#elif UNITY_ANDROID
    public static readonly string StreamingPath = Application.streamingAssetsPath + "/";
#elif UNITY_IOS
    public static readonly string StreamingPath = "file://" + Application.streamingAssetsPath + "/";
#else
    public static readonly string StreamingPath = "file://" + Application.streamingAssetsPath + "/";
#endif

    public static string GetStreamingPath(string fileName)
	{
        return StreamingPath + fileName;
    }

	public static string GetDataPath(string fileName)
	{
        return Application.dataPath + fileName;
	}
}