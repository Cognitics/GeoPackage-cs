using System.Data;
using System.Collections.Generic;

namespace Cognitics.GeoPackage
{
    public abstract class Geometry
    {
    }

    public class Point : Geometry
    {
        public double x;
        public double y;
    }

    public class LineString : Geometry
    {
        public List<Point> points;
    }

    public class Polygon : Geometry
    {
        public List<LineString> rings;
    }

    public class MultiLineString : Geometry
    {
        public List<LineString> geometries;
    }

    public class MultiPolygon : Geometry
    {
        public List<Polygon> geometries;
    }


    public class FeatureLayer : Layer
    {

        internal FeatureLayer(Database database) : base(database)
        {
        }

        public IEnumerable<Feature> Features()
        {
            var geometryColumn = GeometryColumn();
            using (var cmd = Database.Connection.CreateCommand())
            {
                // *** WARNING *** : table name cannot be parameterized ; this is vulnerable to sql injection
                cmd.CommandText = "SELECT * FROM " + TableName;
                cmd.CommandType = CommandType.Text;
                using (var reader = cmd.ExecuteReader())
                {
                    int geometryColumnIndex = (geometryColumn == null) ? -1 : reader.GetOrdinal(geometryColumn.ColumnName);
                    while (reader.Read())
                    {
                        var result = new Feature();
                        for (int i = 0; i < reader.FieldCount; ++i)
                        {
                            if (i == geometryColumnIndex)
                            {
                                result.Geometry = reader.GetValue(i);
                                // TODO: convert from byte[] binary to geometry
                                if (TransformFrom != null)
                                {
                                    // TODO: if layer has transform, use it
                                }
                                continue;
                            }
                            result.Attributes[reader.GetName(i)] = reader.GetValue(i);
                        }
                        yield return result;
                    }
                }
            }
        }


    }
}
