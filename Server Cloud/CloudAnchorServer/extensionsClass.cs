using System;
using NetworkCommon;

namespace extensionsClass
{
    public static class ModelDataExtension
    {
        public static object[] ToObjectArray(this ModelData model)
        {
            object[] parameter = { ((long)model.Id.GetHashCode()), ((int)model.Type), model.Pose.Position.ToArray()
                    , model.Pose.Rotation.ToArray()
                    , model.Message, model.Data, model.Context, model.Id.anchorId, model.Id.name, model.Scale.ToArray() };
            return parameter;
        }

        public static bool Update(this ModelData model, ModelData other)
        {
            bool result = false;

            if (other.Message != model.Message)
            {
                model.Message = other.Message;
                result = true;
            }

            if (other.Context != model.Context)
            {
                model.Context = other.Context;
                result = true;
            }

            if (!model.Pose.Compare(other.Pose))
            {
                model.Pose = other.Pose;
                result = true;
            }

            if (!model.Scale.Compare(other.Scale))
            {
                model.Scale = other.Scale;
                result = true;
            }

            if (other.Data.Length > 0)
            {
                model.Data = other.Data;
                result = true;
            }

            return result;
        }
    }

    public static class SVector3Extension
    {
        public static bool Compare(this SVector3 vec, SVector3 vec2)
        {
            return (vec.x == vec2.x && vec.y == vec2.y && vec.z == vec2.z);
        }

        public static double[] ToArray(this SVector3 vector)
        {
            double[] parameter = { vector.x, vector.y, vector.z };
            return parameter;
        }
    }

    public static class SQuaternionExtension
    {
        public static bool Compare(this SQuaternion quat, SQuaternion quat2)
        {
            return (quat.x == quat2.x && quat.y == quat2.y && quat.z == quat2.z && quat.w == quat2.w);
        }

        public static double[] ToArray(this SQuaternion quat)
        {
            double[] parameter = { quat.x, quat.y, quat.z, quat.w };
            return parameter;
        }
    }

    public static class SPoseExtension
    {
        public static bool Compare(this SPose pose, SPose pose2)
        {
            return (pose.Position.Compare(pose2.Position) && pose.Rotation.Compare(pose2.Rotation));
        }
    }



}
