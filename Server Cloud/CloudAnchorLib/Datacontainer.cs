using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkCommon
{
    // structs do not work with unity/serialization?!

    [Serializable]
    public class SVector3
    {
        public float x { get; private set; }
        public float y { get; private set; }
        public float z { get; private set; }

        public SVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static SVector3 Value(int x)
        {
            return new SVector3(x, x, x);
        }

        public override string ToString()
        {
            return string.Format("vec({0},{1},{2})", x, y, z);
        }
    }
    [Serializable]
    public class SQuaternion
    {
        public float x { get; private set; }
        public float y { get; private set; }
        public float z { get; private set; }
        public float w { get; private set; }
        public SQuaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
        public static SQuaternion Identity()
        {
            return new SQuaternion(0, 0, 0, 1);
        }
        public override string ToString()
        {
            return string.Format("q({0},{1},{2},{3})", x, y, z, w);
        }
    }
    [Serializable]
    public class SPose
    {
        public SVector3 Position { get; private set; }
        public SQuaternion Rotation { get; private set; }
        public SPose(float x, float y, float z, float i, float j, float k, float w) : this(new SVector3(x, y, z), new SQuaternion(i, j, k, w)) { }
        public SPose(SVector3 pos, SQuaternion rot)
        {
            Position = pos;
            Rotation = rot;
        }
        public static SPose Identity()
        {
            return new SPose(SVector3.Value(0), SQuaternion.Identity());
        }
        public override string ToString()
        {
            return string.Format("Pose({0}, {1})", Position, Rotation);
        }

    }

    [Serializable]
    public class ARId
    {
        public string name { get; private set; }
        public string anchorId { get; private set; }

        private int hash;
        public ARId(string name, string anchorId)
        {
            this.name = name;
            this.anchorId = anchorId;
            this.hash = (name + anchorId).GetHashCode();
        }
        public override int GetHashCode()
        {
            return hash;
        }
        public override string ToString()
        {
            return String.Format("ArId: {0}, {1}", name, anchorId);
        }
    }

    [Serializable]
    public class ModelData
    {
        public enum ModelType { ART, TXT, IMG }

        public ARId Id { get; private set; }
        public ModelType Type { get; private set; }
        public SPose Pose { get; set; }

        public SVector3 Scale { get; set; }

        public string Message { get; set; }
        public byte[] Data { get; set; }
        public string Context { get; set; }


        public ModelData(string name, string anchorId, SPose pose = null, SVector3 scale = null, ModelType type = ModelType.ART, string msg = "", byte[] data = null, string context = "")
        : this(new ARId(name, anchorId), pose, scale, type, msg, data, context) { }
        public ModelData(ARId arId, SPose pose = null, SVector3 scale = null, ModelType type = ModelType.ART, string msg = "", byte[] data = null, string context = "")
        {
            this.Id = arId;
            this.Pose = pose ?? SPose.Identity();
            this.Scale = scale ?? SVector3.Value(1);
            this.Type = type;
            this.Message = msg;
            this.Data = data ?? new byte[0];
            this.Context = context;
        }

        public override string ToString()
        {
            return string.Format("Model: {0}, {1}, {2}, {3}, {4}", Id, Type, Pose, Message, Context);
        }

    }

}
