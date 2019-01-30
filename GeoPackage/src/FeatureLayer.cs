
using System.Data;
using System.Collections.Generic;

namespace Cognitics.GeoPackage
{
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
                        var feature = new Feature();
                        for (int i = 0; i < reader.FieldCount; ++i)
                        {
                            if (i == geometryColumnIndex)
                            {
                                feature.Geometry = BinaryGeometry.Read(reader.GetStream(i));
                                if (feature.Geometry == null)
                                    continue;
                                if (TransformFrom == null)
                                    continue;

                                // TODO: if layer has transform, use it

                                continue;
                            }
                            feature.Attributes[reader.GetName(i)] = reader.GetValue(i);
                        }
                        yield return feature;
                    }
                }
            }
        }


    }
}
