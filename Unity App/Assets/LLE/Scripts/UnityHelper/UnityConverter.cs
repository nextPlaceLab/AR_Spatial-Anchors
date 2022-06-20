using NetworkCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LLE.Network
{
    public class UnityConverter
    {
        public static Pose Convert(Transform transform)
        {
            return new Pose(transform.position, transform.rotation);
        }
        public static Vector3 Convert(SVector3 vector)
        {
            return new Vector3(vector.x, vector.y, vector.z);
        }
        public static SVector3 Convert(Vector3 vector)
        {
            return new SVector3(vector.x, vector.y, vector.z);
        }
        public static Quaternion Convert(SQuaternion q)
        {
            return new Quaternion(q.x, q.y, q.z, q.w);
        }
        public static SQuaternion Convert(Quaternion q)
        {
            return new SQuaternion(q.x, q.y, q.z, q.w);
        }
        public static Pose Convert(SPose pose)
        {
            return new Pose(Convert(pose.Position), Convert(pose.Rotation));
        }
        public static SPose Convert(Pose pose)
        {
            return new SPose(Convert(pose.position), Convert(pose.rotation));
        }
    }
}
