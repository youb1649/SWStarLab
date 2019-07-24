using System;
using MNF;

namespace MNF_Common
{
    public class ConsoleLogWriter : ILogWriter
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
#if !NETFX_CORE
            Console.WriteLine(logString);
#endif
            return true;
        }
    }
}