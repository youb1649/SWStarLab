using UnityEngine;
using MNF;

public class UnityLogWriter : ILogWriter
{
    public override bool OnInit()
    {
        return true;
    }

    public override void OnRelease()
    {
    }

    public override bool OnWrite(ENUM_LOG_TYPE logType, string logString)
    {
        Debug.Log(logType.ToString() + " : " + logString);
        return true;
    }
}
