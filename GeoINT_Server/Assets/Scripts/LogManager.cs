using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeoINT_Server
{
    public class LogManager : MonoBehaviour
    {
        public int iLogSize = 20;
        private Queue<string> queLog = new Queue<string>();
        private string szLocalMsg = "";

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        void OnGUI()
        {
            szLocalMsg = "";
            foreach (string szTmp in queLog)
            {
                szLocalMsg += szTmp;
                szLocalMsg += "\n";
            }

            szLocalMsg = GUILayout.TextArea(szLocalMsg, 600);
        }

        public void InsertLog(string szLog)
        {
            if (iLogSize < queLog.Count)
                queLog.Dequeue();

            queLog.Enqueue(szLog);
        }
    }
}
