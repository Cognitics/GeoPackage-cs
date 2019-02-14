using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SQLite;
using System.Linq;

namespace Cognitics.GeoPackage
{
    public class RasterLayer : Layer
    {
        public IEnumerable<TileMatrix> TileMatrices()
        {
            using (var cmd = Database.Connection.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM gpkg_tile_matrix WHERE table_name=@table_name";
                cmd.Parameters.Add(new SQLiteParameter("@table_name", TableName));
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        yield return ReadTileMatrix(reader);
            }
        }

        public IEnumerable<long> ZoomLevels()
        {
            foreach (var tileMatrix in TileMatrices())
                yield return tileMatrix.ZoomLevel;
        }

        public IEnumerable<Tile> Tiles()
        {
            using (var cmd = Database.Connection.CreateCommand())
            {
                // *** WARNING *** : table name cannot be parameterized ; this is vulnerable to sql injection
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM " + TableName;
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        yield return ReadTile(reader);
            }
        }

        public IEnumerable<Tile> Tiles(double minX, double maxX, double minY, double maxY)
        {
            using (var cmd = Database.Connection.CreateCommand())
            {
                // *** WARNING *** : table name cannot be parameterized ; this is vulnerable to sql injection
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM " + TableName + " WHERE ";
                cmd.CommandText += "(minx <= @max_x) AND (maxx >= @min_x) AND ";
                cmd.CommandText += "(miny <= @max_y) AND (maxy >= @min_y)";
                cmd.Parameters.Add(new SQLiteParameter("@min_x", minX));
                cmd.Parameters.Add(new SQLiteParameter("@max_x", maxX));
                cmd.Parameters.Add(new SQLiteParameter("@min_y", minY));
                cmd.Parameters.Add(new SQLiteParameter("@max_y", maxY));
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        yield return ReadTile(reader);
            }
        }

        public IEnumerable<Tile> Tiles(long zoomLevel)
        {
            using (var cmd = Database.Connection.CreateCommand())
            {
                // *** WARNING *** : table name cannot be parameterized ; this is vulnerable to sql injection
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM " + TableName + " WHERE zoom_level=@zoom_level";
                cmd.Parameters.Add(new SQLiteParameter("@zoom_level", zoomLevel));
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        yield return ReadTile(reader);
            }
        }

        public IEnumerable<Tile> Tiles(long zoomLevel, double minX, double maxX, double minY, double maxY)
        {
            using (var cmd = Database.Connection.CreateCommand())
            {
                // *** WARNING *** : table name cannot be parameterized ; this is vulnerable to sql injection
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM " + TableName + " WHERE zoom_level=@zoom_level AND ";
                cmd.CommandText += "(minx <= @max_x) AND (maxx >= @min_x) AND ";
                cmd.CommandText += "(miny <= @max_y) AND (maxy >= @min_y)";
                cmd.Parameters.Add(new SQLiteParameter("@zoom_level", zoomLevel));
                cmd.Parameters.Add(new SQLiteParameter("@min_x", minX));
                cmd.Parameters.Add(new SQLiteParameter("@max_x", maxX));
                cmd.Parameters.Add(new SQLiteParameter("@min_y", minY));
                cmd.Parameters.Add(new SQLiteParameter("@max_y", maxY));
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        yield return ReadTile(reader);
            }
        }

        public Tile Tile(long zoomLevel, long row, long column)
        {
            using (var cmd = Database.Connection.CreateCommand())
            {
                // *** WARNING *** : table name cannot be parameterized ; this is vulnerable to sql injection
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM " + TableName + " WHERE zoom_level=@zoom_level AND tile_row=@tile_row AND tile_column=@tile_column";
                cmd.Parameters.Add(new SQLiteParameter("@zoom_level", zoomLevel));
                cmd.Parameters.Add(new SQLiteParameter("@tile_row", row));
                cmd.Parameters.Add(new SQLiteParameter("@tile_column", column));
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        return ReadTile(reader);
            }
            return null;
        }


        #region implementation


        internal RasterLayer(Database database) : base(database)
        {
            UpdateRasterExtentsFromTileMatrixSet();
        }

        /// <summary>
        /// Requirement 18 of the GeoPackage standard requires the extents in gpkg_tile_matrix_set
        /// to be exact, whereas the extents in gpkg_contents are informational only.
        /// This method corrects the extents by reading gpkg_tile_matrix_set for raster layers.
        /// </summary>
        private void UpdateRasterExtentsFromTileMatrixSet()
        {
            using (var cmd = Database.Connection.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM gpkg_tile_matrix_set WHERE table_name=@table_name";
                cmd.Parameters.Add(new SQLiteParameter("@table_name", TableName));
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        MinX = Database.GetFieldValue(reader, reader.GetOrdinal("min_x"), double.MinValue);
                        MinY = Database.GetFieldValue(reader, reader.GetOrdinal("min_y"), double.MinValue);
                        MaxX = Database.GetFieldValue(reader, reader.GetOrdinal("max_x"), double.MaxValue);
                        MaxY = Database.GetFieldValue(reader, reader.GetOrdinal("max_y"), double.MaxValue);
                    }
                }
            }
        }

        private TileMatrix ReadTileMatrix(SQLiteDataReader reader)
        {
            return new TileMatrix
            {
                TableName = Database.GetFieldValue(reader, reader.GetOrdinal("table_name"), ""),
                ZoomLevel = Database.GetFieldValue(reader, reader.GetOrdinal("zoom_level"), (long)0),
                TilesWide = Database.GetFieldValue(reader, reader.GetOrdinal("matrix_width"), (long)0),
                TilesHigh = Database.GetFieldValue(reader, reader.GetOrdinal("matrix_height"), (long)0),
                TileWidth = Database.GetFieldValue(reader, reader.GetOrdinal("tile_width"), (long)0),
                TileHeight = Database.GetFieldValue(reader, reader.GetOrdinal("tile_height"), (long)0),
                PixelXSize = Database.GetFieldValue(reader, reader.GetOrdinal("pixel_x_size"), 0.0),
                PixelYSize = Database.GetFieldValue(reader, reader.GetOrdinal("pixel_y_size"), 0.0),
            };
        }

        private Tile ReadTile(SQLiteDataReader reader)
        {
            return new Tile
            {
                ID = Database.GetFieldValue(reader, reader.GetOrdinal("id"), (long)0),
                ZoomLevel = Database.GetFieldValue(reader, reader.GetOrdinal("zoom_level"), (long)0),
                TileColumn = Database.GetFieldValue(reader, reader.GetOrdinal("tile_column"), (long)0),
                TileRow = Database.GetFieldValue(reader, reader.GetOrdinal("tile_row"), (long)0),
                Bytes = Database.GetFieldValue(reader, reader.GetOrdinal("tile_data"), (byte[])null),
            };
        }

        #endregion

    }
}
