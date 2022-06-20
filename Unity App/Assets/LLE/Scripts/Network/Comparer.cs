using NetworkCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLE.Network
{
    class RequestComparer : IEqualityComparer<Request>
    {
        public bool Equals(Request x, Request y)
        {
            return (x.RequestType.Equals(y.RequestType)) &&
                 (x.Id.Equals(y.Id));
        }

        public int GetHashCode(Request request)
        {
            return request.ToString().GetHashCode();
        }
    }
    class ARIdComparer : IEqualityComparer<ARId>
    {
        public bool Equals(ARId x , ARId y)
        {
            return int.Equals(x.GetHashCode(),y.GetHashCode());
        }

        public int GetHashCode(ARId id)
        {
            return id.GetHashCode();
        }
    }
    class ModelDataComparer: IEqualityComparer<ModelData>
    {
        public bool Equals(ModelData x, ModelData y)
        {
            return int.Equals(x.Id.GetHashCode(), y.Id.GetHashCode());
        }

        public int GetHashCode(ModelData md)
        {
            return md.Id.GetHashCode();
        }
    }
}
