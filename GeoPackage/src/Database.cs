
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

        public Database(string filename)
        {
            Connection = new SQLiteConnection("Data Source=" + filename + ";Version=3;Mode=ReadOnly;");
            Connection.Open();
        }

        ~Database()
        {
            Connection.Close();
        }

        private string GetString(SQLiteDataReader reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? "" : reader.GetString(ordinal);
        }

        public Dictionary<int, SpatialReferenceSystem> SpatialReferenceSystems()
        {
            var result = new Dictionary<int, SpatialReferenceSystem>();
            var cmd = new SQLiteCommand("SELECT * FROM gpkg_spatial_ref_sys", Connection);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var entry = new SpatialReferenceSystem();
                entry.ID = reader.GetInt32(reader.GetOrdinal("srs_id"));
                entry.Name = reader.GetString(reader.GetOrdinal("srs_name"));
                entry.Organization = GetString(reader, reader.GetOrdinal("organization"));
                entry.OrganizationCoordinateSystemID = reader.GetInt32(reader.GetOrdinal("organization_coordsys_id"));
                entry.Definition = GetString(reader, reader.GetOrdinal("definition"));
                entry.Description = GetString(reader, reader.GetOrdinal("description"));
                result[entry.ID] = entry;
            }
            return result;
        }

        public Dictionary<string, Table> Contents()
        {
            var result = new Dictionary<string, Table>();
            var cmd = new SQLiteCommand("SELECT * FROM gpkg_contents", Connection);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var entry = new Table();
                entry.Name = GetString(reader, reader.GetOrdinal("table_name"));
                entry.DataType = GetString(reader, reader.GetOrdinal("data_type"));
                entry.Identifier = GetString(reader, reader.GetOrdinal("identifier"));
                entry.Description = GetString(reader, reader.GetOrdinal("description"));
                entry.LastChange = reader.GetDateTime(reader.GetOrdinal("last_change"));
                entry.MinX = reader.GetDouble(reader.GetOrdinal("min_x"));
                entry.MinY = reader.GetDouble(reader.GetOrdinal("min_y"));
                entry.MaxX = reader.GetDouble(reader.GetOrdinal("max_x"));
                entry.MaxY = reader.GetDouble(reader.GetOrdinal("max_y"));
                entry.SpatialReferenceSystemID = reader.GetInt32(reader.GetOrdinal("srs_id"));
                result[entry.Name] = entry;
            }
            return result;
        }


            //- gpkg_geometry_columns
            //- feature tables

                /*
                for (int i = 0; i < reader.FieldCount; ++i)
                    row[reader.GetName(i)] = reader.GetValue(i);
                */

    }
}
