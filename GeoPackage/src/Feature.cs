
using System.Collections.Generic;

namespace Cognitics.GeoPackage
{
    public class Feature
    {
        public object Geometry = null;
        public Dictionary<string, object> Attributes = new Dictionary<string, object>();
    }
}
