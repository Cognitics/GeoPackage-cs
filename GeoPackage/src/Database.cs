
using System;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Linq;

namespace Cognitics.GeoPackage
{
    public class Database
    {
        public readonly SQLiteConnection Connection;
        public readonly SpatialReferenceSystem ApplicationSpatialReferenceSystem;

        public Database(string filename, SpatialReferenceSystem applicationSpatialReferenceSystem = null)
        {
            ApplicationSpatialReferenceSystem = applicationSpatialReferenceSystem;
            Connection = new SQLiteConnection("Data Source=" + filename + ";Version=3;Mode=ReadOnly;");
            Connection.Open();
        }

        ~Database()
        {
            Connection.Close();
        }

        private T GetFieldValue<T>(SQLiteDataReader reader, int ordinal, T defaultValue)
        {
            return reader.IsDBNull(ordinal) ? defaultValue : reader.GetFieldValue<T>(ordinal);
        }

        private byte[] GetFieldValueBlob(SQLiteDataReader reader, int ordinal)
        {
            System.IO.Stream stream = reader.GetStream(ordinal);
            byte[] result = new byte[stream.Length];
            stream.Read(result, 0, (int)stream.Length);
            return result;
        }

        public IEnumerable<SpatialReferenceSystem> SpatialReferenceSystems()
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM gpkg_spatial_ref_sys";
                cmd.CommandType = CommandType.Text;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
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
                        yield return result;
                    }
                }
            }
        }

        public SpatialReferenceSystem SpatialReferenceSystem(long id)
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM gpkg_spatial_ref_sys WHERE srs_id=@srs_id";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new SQLiteParameter("@srs_id", id));
                using (var reader = cmd.ExecuteReader())
                {
                    if(reader.Read())
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
                }
            }
            return null;
        }

        public IEnumerable<Layer> Layers(string dataType = null)
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM gpkg_contents";
                cmd.CommandType = CommandType.Text;
                if (dataType != null)
                {
                    cmd.CommandText += " WHERE data_type=@data_type";
                    cmd.Parameters.Add(new SQLiteParameter("@data_type", dataType));
                }
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string layerType = GetFieldValue(reader, reader.GetOrdinal("data_type"), "");
                        Layer result = null;
                        if (layerType == "features")
                            result = new FeatureLayer(this);
                        if (layerType == "raster")
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
                        yield return result;
                    }
                }
            }
        }

        public IEnumerable<GeometryColumn> GeometryColumns(string tableName = null)
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM gpkg_geometry_columns";
                cmd.CommandType = CommandType.Text;
                if (tableName != null)
                {
                    cmd.CommandText += " WHERE table_name=@table_name";
                    cmd.Parameters.Add(new SQLiteParameter("@table_name", tableName));
                }
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var result = new GeometryColumn
                        {
                            TableName = GetFieldValue(reader, reader.GetOrdinal("table_name"), ""),
                            ColumnName = GetFieldValue(reader, reader.GetOrdinal("column_name"), ""),
                            GeometryTypeName = GetFieldValue(reader, reader.GetOrdinal("geometry_type_name"), ""),
                            SpatialReferenceSystemID = GetFieldValue(reader, reader.GetOrdinal("srs_id"), (long)0),
                            m = GetFieldValue(reader, reader.GetOrdinal("m"), (byte)0),
                            z = GetFieldValue(reader, reader.GetOrdinal("z"), (byte)0)
                        };
                        yield return result;
                    }
                }
            }
        }


    }
}
