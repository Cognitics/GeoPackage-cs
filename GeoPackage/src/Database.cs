
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;

namespace Cognitics.GeoPackage
{
    public class Database
    {
        public readonly SQLiteConnection Connection;
        public SpatialReferenceSystem ApplicationSpatialReferenceSystem;

        public Database(string filename)
        {
            Connection = new SQLiteConnection("Data Source=" + filename + ";Version=3;Mode=ReadOnly;");
            Connection.Open();
        }

        public SpatialReferenceSystem SpatialReferenceSystem(long id)
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM gpkg_spatial_ref_sys WHERE srs_id=@srs_id";
                cmd.Parameters.Add(new SQLiteParameter("@srs_id", id));
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        return SpatialReferenceSystem(reader);
            }
            return null;
        }

        public IEnumerable<SpatialReferenceSystem> SpatialReferenceSystems()
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM gpkg_spatial_ref_sys";
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        yield return SpatialReferenceSystem(reader);
            }
        }

        public IEnumerable<Layer> Layers()
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM gpkg_contents";
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        yield return ReadLayer(reader);
            }
        }

        public IEnumerable<Layer> Layers(string dataType)
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM gpkg_contents WHERE data_type=@data_type";
                cmd.Parameters.Add(new SQLiteParameter("@data_type", dataType));
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        yield return ReadLayer(reader);
            }
        }

        public IEnumerable<Layer> Layers(double minX, double maxX, double minY, double maxY)
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM gpkg_contents WHERE ";
                cmd.CommandText += "(min_x <= @max_x) AND (max_x >= @min_x) AND ";
                cmd.CommandText += "(min_y <= @max_y) AND (max_y >= @min_y)";
                cmd.Parameters.Add(new SQLiteParameter("@min_x", minX));
                cmd.Parameters.Add(new SQLiteParameter("@max_x", maxX));
                cmd.Parameters.Add(new SQLiteParameter("@min_y", minY));
                cmd.Parameters.Add(new SQLiteParameter("@max_y", maxY));
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        yield return ReadLayer(reader);
            }
        }

        public IEnumerable<Layer> Layers(string dataType, double minX, double maxX, double minY, double maxY)
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM gpkg_contents WHERE data_type=@data_type";
                cmd.CommandText += "(min_x <= @max_x) AND (max_x >= @min_x) AND ";
                cmd.CommandText += "(min_y <= @max_y) AND (max_y >= @min_y)";
                cmd.Parameters.Add(new SQLiteParameter("@data_type", dataType));
                cmd.Parameters.Add(new SQLiteParameter("@min_x", minX));
                cmd.Parameters.Add(new SQLiteParameter("@max_x", maxX));
                cmd.Parameters.Add(new SQLiteParameter("@min_y", minY));
                cmd.Parameters.Add(new SQLiteParameter("@max_y", maxY));
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        yield return ReadLayer(reader);
            }
        }

        public IEnumerable<Layer> FeatureLayers() => Layers("features");
        public IEnumerable<Layer> FeatureLayers(double minX, double maxX, double minY, double maxY)
            => Layers("features", minX, maxX, minY, maxY);
        public IEnumerable<Layer> RasterLayers() => Layers("tiles");
        public IEnumerable<Layer> RasterLayers(double minX, double maxX, double minY, double maxY)
            => Layers("tiles", minX, maxX, minY, maxY);

        #region implementation

        ~Database()
        {
            Connection.Close();
        }

        internal T GetFieldValue<T>(SQLiteDataReader reader, int ordinal, T defaultValue)
        {
            return reader.IsDBNull(ordinal) ? defaultValue : reader.GetFieldValue<T>(ordinal);
        }

        private SpatialReferenceSystem SpatialReferenceSystem(SQLiteDataReader reader)
        {
            var result = new SpatialReferenceSystem
            {
                ID = GetFieldValue(reader, reader.GetOrdinal("srs_id"), (long)0),
                Name = GetFieldValue(reader, reader.GetOrdinal("srs_name"), ""),
                Organization = GetFieldValue(reader, reader.GetOrdinal("organization"), ""),
                OrganizationCoordinateSystemID = GetFieldValue(reader, reader.GetOrdinal("organization_coordsys_id"), (long)0),
                Definition = GetFieldValue(reader, reader.GetOrdinal("definition"), ""),
                Description = GetFieldValue(reader, reader.GetOrdinal("description"), "")
            };
            return result;
        }

        private Layer ReadLayer(SQLiteDataReader reader)
        {
            string layerType = GetFieldValue(reader, reader.GetOrdinal("data_type"), "");
            Layer result = null;
            if (layerType == "features")
                result = new FeatureLayer(this);
            if (layerType == "tiles")
                result = new RasterLayer(this);
            if (result == null)
                result = new Layer(this);
            result.TableName = GetFieldValue(reader, reader.GetOrdinal("table_name"), "");
            result.DataType = GetFieldValue(reader, reader.GetOrdinal("data_type"), "");
            result.Identifier = GetFieldValue(reader, reader.GetOrdinal("identifier"), "");
            result.Description = GetFieldValue(reader, reader.GetOrdinal("description"), "");
            result.LastChange = reader.GetDateTime(reader.GetOrdinal("last_change"));
            result.MinX = GetFieldValue(reader, reader.GetOrdinal("min_x"), double.MinValue);
            result.MinY = GetFieldValue(reader, reader.GetOrdinal("min_y"), double.MinValue);
            result.MaxX = GetFieldValue(reader, reader.GetOrdinal("max_x"), double.MaxValue);
            result.MaxY = GetFieldValue(reader, reader.GetOrdinal("max_y"), double.MaxValue);
            result.SpatialReferenceSystemID = GetFieldValue(reader, reader.GetOrdinal("srs_id"), (long)0);
            return result;
        }

        #endregion


    }
}
