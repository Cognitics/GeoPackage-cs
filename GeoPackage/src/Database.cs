
using System;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;
using ProjNet.CoordinateSystems.Transformations;

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
                        return ReadSpatialReferenceSystem(reader);
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
                        yield return ReadSpatialReferenceSystem(reader);
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

        private SpatialReferenceSystem ReadSpatialReferenceSystem(SQLiteDataReader reader)
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
            Layer layer = null;
            if (layerType == "features")
                layer = new FeatureLayer(this);
            if (layerType == "tiles")
                layer = new RasterLayer(this);
            if (layer == null)
                layer = new Layer(this);
            layer.TableName = GetFieldValue(reader, reader.GetOrdinal("table_name"), "");
            layer.DataType = GetFieldValue(reader, reader.GetOrdinal("data_type"), "");
            layer.Identifier = GetFieldValue(reader, reader.GetOrdinal("identifier"), "");
            layer.Description = GetFieldValue(reader, reader.GetOrdinal("description"), "");
            layer.LastChange = reader.GetDateTime(reader.GetOrdinal("last_change"));
            layer.MinX = GetFieldValue(reader, reader.GetOrdinal("min_x"), double.MinValue);
            layer.MinY = GetFieldValue(reader, reader.GetOrdinal("min_y"), double.MinValue);
            layer.MaxX = GetFieldValue(reader, reader.GetOrdinal("max_x"), double.MaxValue);
            layer.MaxY = GetFieldValue(reader, reader.GetOrdinal("max_y"), double.MaxValue);
            layer.SpatialReferenceSystemID = GetFieldValue(reader, reader.GetOrdinal("srs_id"), (long)0);
            /*
            TODO: transformation handling for all queries and layers
                gpkg_contents has srs (for min/max)
                gpkg_geometry_columns has srs (for geometry)
                gpkg_tile_matrix_set has srs (for zoomlevel bounds and matrix pixel size)


            if (ApplicationSpatialReferenceSystem != null)
            {
                var layerSpatialReferenceSystem = layer.SpatialReferenceSystem;
                if (layerSpatialReferenceSystem != null)
                {
                    if (layerSpatialReferenceSystem.ID == 0)
                        layerSpatialReferenceSystem = SpatialReferenceSystem(4326);
                    if (layerSpatialReferenceSystem.Definition != ApplicationSpatialReferenceSystem.Definition)
                    {
                        try
                        {
                            var layerSRS = GeoPackage.SpatialReferenceSystem.ProjNetCoordinateSystem(layerSpatialReferenceSystem.Definition);
                            var appSRS = GeoPackage.SpatialReferenceSystem.ProjNetCoordinateSystem(ApplicationSpatialReferenceSystem.Definition);
                            layer.TransformFrom = GeoPackage.SpatialReferenceSystem.ProjNetTransform(layerSRS, appSRS);
                            layer.TransformTo = GeoPackage.SpatialReferenceSystem.ProjNetTransform(appSRS, layerSRS);
                        }
                        catch (ArgumentException) { }
                    }
                }
                if (layer.TransformFrom != null)
                {
                    {
                        var xy1 = new double[2] { layer.MinX, layer.MinY };
                        var xy2 = layer.TransformFrom.MathTransform.Transform(xy1);
                        layer.MinX = xy2[0];
                        layer.MinY = xy2[1];
                    }
                    {
                        var xy1 = new double[2] { layer.MaxX, layer.MaxY };
                        var xy2 = layer.TransformFrom.MathTransform.Transform(xy1);
                        layer.MaxX = xy2[0];
                        layer.MaxY = xy2[1];
                    }
                }
            }
            */
            return layer;
        }

        #endregion


    }
}
