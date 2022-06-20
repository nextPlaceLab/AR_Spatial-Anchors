using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Network.Server
{
    public class BinarySerializer
    {
        public static byte[] Serialize(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);

            return ms.ToArray();
        }

        public static Object Deserialize(byte[] data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream memStream = new MemoryStream();
            memStream.Write(data, 0, data.Length);
            memStream.Seek(0, SeekOrigin.Begin);

            return (Object)formatter.Deserialize(memStream);
        }
    }
}
