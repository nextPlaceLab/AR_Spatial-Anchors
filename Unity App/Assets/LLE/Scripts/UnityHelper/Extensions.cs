using LLE.Network;
using NetworkCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LLE.Unity
{
    public static class Extensions
    {
        public static Pose Convert(this SPose pose)
        {
            return UnityConverter.Convert(pose);
        }
        public static SPose Convert(this Pose pose)
        {
            return UnityConverter.Convert(pose);
        }

       
    }
    
}
