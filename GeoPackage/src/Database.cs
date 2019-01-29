
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

        private T GetFieldValue<T>(SQLiteDataReader reader, int ordinal, T defaultValue)
        {
            return reader.IsDBNull(ordinal) ? defaultValue : reader.GetFieldValue<T>(ordinal);
        }

        public Dictionary<long, SpatialReferenceSystem> SpatialReferenceSystems()
        {
            var result = new Dictionary<long, SpatialReferenceSystem>();
            var cmd = new SQLiteCommand("SELECT * FROM gpkg_spatial_ref_sys", Connection);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var entry = new SpatialReferenceSystem();
                entry.ID = GetFieldValue(reader, reader.GetOrdinal("srs_id"), (long)0);
                entry.Name = GetFieldValue(reader, reader.GetOrdinal("srs_name"), "");
                entry.Organization = GetFieldValue(reader, reader.GetOrdinal("organization"), "");
                entry.OrganizationCoordinateSystemID = GetFieldValue(reader, reader.GetOrdinal("organization_coordsys_id"), (long)0);
                entry.Definition = GetFieldValue(reader, reader.GetOrdinal("definition"), "");
                entry.Description = GetFieldValue(reader, reader.GetOrdinal("description"), "");
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
                entry.Name = GetFieldValue(reader, reader.GetOrdinal("table_name"), "");
                entry.DataType = GetFieldValue(reader, reader.GetOrdinal("data_type"), "");
                entry.Identifier = GetFieldValue(reader, reader.GetOrdinal("identifier"), "");
                entry.Description = GetFieldValue(reader, reader.GetOrdinal("description"), "");
                entry.LastChange = reader.GetDateTime(reader.GetOrdinal("last_change"));
                entry.MinX = GetFieldValue(reader, reader.GetOrdinal("min_x"), double.MinValue);
                entry.MinY = GetFieldValue(reader, reader.GetOrdinal("min_y"), double.MinValue);
                entry.MaxX = GetFieldValue(reader, reader.GetOrdinal("max_x"), double.MaxValue);
                entry.MaxY = GetFieldValue(reader, reader.GetOrdinal("max_y"), double.MaxValue);
                entry.SpatialReferenceSystemID = GetFieldValue(reader, reader.GetOrdinal("srs_id"), (long)0);
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
