
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
        public SpatialReferenceSystem ApplicationSpatialReferenceSystem;

        public Database(string filename)
        {
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

        public IEnumerable<Layer> FeatureLayers() => Layers("features");

        /// <summary>
        /// Requirement 18 of the GeoPackage standard requires the extents in gpkg_tile_matrix_set
        /// to be exact, whereas the extents in gpkg_contents are informational only.
        /// This method corrects the extents by reading gpkg_tile_matrix_set for raster layers.
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public bool SetRasterExtentsFromTileMatrixSet(RasterLayer layer)
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM gpkg_tile_matrix_set";
                cmd.CommandType = CommandType.Text;
                cmd.CommandText += " WHERE table_name=@table_name";
                cmd.Parameters.Add(new SQLiteParameter("@table_name", layer.TableName));
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        layer.MinX = GetFieldValue(reader, reader.GetOrdinal("min_x"), double.MinValue);
                        layer.MinY = GetFieldValue(reader, reader.GetOrdinal("min_y"), double.MinValue);
                        layer.MaxX = GetFieldValue(reader, reader.GetOrdinal("max_x"), double.MaxValue);
                        layer.MaxY = GetFieldValue(reader, reader.GetOrdinal("max_y"), double.MaxValue);
                    }
                }
            }
            return true;
        }

        public IEnumerable<TileMatrix> TileMatrixSet(string tableName)
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM gpkg_tile_matrix";
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
                        var result = new TileMatrix
                        {
                            TableName = GetFieldValue(reader, reader.GetOrdinal("table_name"), ""),
                            ZoomLevel = GetFieldValue(reader, reader.GetOrdinal("zoom_level"), (int)0),
                            TilesWide = GetFieldValue(reader, reader.GetOrdinal("matrix_width"), (int)0),
                            TilesHigh = GetFieldValue(reader, reader.GetOrdinal("matrix_height"), (int)0),
                            TileWidth = GetFieldValue(reader, reader.GetOrdinal("tile_width"), (int)0),
                            TileHeight = GetFieldValue(reader, reader.GetOrdinal("tile_height"), (int)0),
                            PixelXSize = GetFieldValue(reader, reader.GetOrdinal("pixel_x_size"), 0.0),
                            PixelYSize = GetFieldValue(reader, reader.GetOrdinal("pixel_y_size"), 0.0),
                        };
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

        /// <summary>
        /// Read a single tile
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="zoomLevel"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public Tile Tile(string tableName, int zoomLevel, int row, int col)
        {
            using (var cmd = Connection.CreateCommand())
            {
                // *** WARNING *** : table name cannot be parameterized ; this is vulnerable to sql injection
                cmd.CommandText = "SELECT * FROM " + tableName;
                cmd.CommandType = CommandType.Text;
                if (tableName != null)
                {
                    cmd.CommandText += " WHERE zoom_level=@zoom_level and tile_column=@tile_column and tile_row=@tile_row";
                    cmd.Parameters.Add(new SQLiteParameter("@zoom_level", zoomLevel));
                    cmd.Parameters.Add(new SQLiteParameter("@tile_column", col));
                    cmd.Parameters.Add(new SQLiteParameter("@tile_row", row));
                }
                
                using (var reader = cmd.ExecuteReader())
                {
                    if(reader.Read())
                    {
                        var result = new Tile
                        {
                            Id = GetFieldValue(reader, reader.GetOrdinal("id"), 0),
                            ZoomLevel = GetFieldValue(reader, reader.GetOrdinal("zoom_level"), 0),
                            TileColumn = GetFieldValue(reader, reader.GetOrdinal("tile_column"), 0),
                            TileRow = GetFieldValue(reader, reader.GetOrdinal("tile_row"), 0),
                        };
                        int ordinal = reader.GetOrdinal("tile_data");
                        if (!reader.IsDBNull(ordinal))
                        {
                            System.IO.Stream s = reader.GetStream(ordinal);
                            long stest = s.Length;
                            byte[] buf = new byte[s.Length];
                            s.Read(buf, 0, (int)s.Length);
                            result.Bytes = buf;
                        }
                        return result;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        public IEnumerable<Feature> Features(Layer layer)
        {
            // *** WARNING *** : table name cannot be parameterized ; this is vulnerable to sql injection
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM " + layer.TableName;
                cmd.CommandType = CommandType.Text;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var result = new Feature();
                        for (int i = 0; i < reader.FieldCount; ++i)
                            result.Attributes[reader.GetName(i)] = reader.GetValue(i);
                        yield return result;
                    }
                }
            }
        }


    }
}
