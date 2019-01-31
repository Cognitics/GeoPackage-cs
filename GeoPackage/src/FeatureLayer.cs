
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace Cognitics.GeoPackage
{
    public class FeatureLayer : Layer
    {

        public IEnumerable<Feature> Features()
        {
            var geometryColumn = GeometryColumn();
            using (var cmd = Database.Connection.CreateCommand())
            {
                // *** WARNING *** : table name cannot be parameterized ; this is vulnerable to sql injection
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM " + TableName;
                using (var reader = cmd.ExecuteReader())
                {
                    int geometryColumnIndex = (geometryColumn == null) ? -1 : reader.GetOrdinal(geometryColumn.ColumnName);
                    while (reader.Read())
                        yield return Feature(reader, geometryColumnIndex);
                }
            }
        }

        // TODO: this requires rtree
        /*
        public IEnumerable<Feature> Features(double minX, double maxX, double minY, double maxY)
        {
            yield return null;
            var geometryColumn = GeometryColumn();
            using (var cmd = Database.Connection.CreateCommand())
            {
                // *** WARNING *** : table name cannot be parameterized ; this is vulnerable to sql injection
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM " + TableName + " WHERE ";
                cmd.CommandText += "(min_x <= @max_x) AND (max_x >= @min_x) AND ";
                cmd.CommandText += "(min_y <= @max_y) AND (max_y >= @min_y)";
                cmd.Parameters.Add(new SQLiteParameter("@min_x", minX));
                cmd.Parameters.Add(new SQLiteParameter("@max_x", maxX));
                cmd.Parameters.Add(new SQLiteParameter("@min_y", minY));
                cmd.Parameters.Add(new SQLiteParameter("@max_y", maxY));
                using (var reader = cmd.ExecuteReader())
                {
                    int geometryColumnIndex = (geometryColumn == null) ? -1 : reader.GetOrdinal(geometryColumn.ColumnName);
                    while (reader.Read())
                        yield return Feature(reader, geometryColumnIndex);
                }
            }
        }
        */

        #region implementation

        internal FeatureLayer(Database database) : base(database)
        {
        }

        private Feature Feature(SQLiteDataReader reader, int geometryColumnIndex)
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
                    feature.Geometry.Transform(TransformFrom);
                    continue;
                }
                feature.Attributes[reader.GetName(i)] = reader.GetValue(i);
            }
            return feature;
        }

        #endregion

    }
}
