
using System;

namespace NetworkCommon
{
    // just as an example. you can use any class marked with [Serializable]
    [Serializable]
    public class SerializableData
    {
        public string msg = "Der Server surft";
        public int ans = 42;
        public override string ToString()
        {

            return "msg = " + msg + ", ans = " + ans;
        }
    }
}