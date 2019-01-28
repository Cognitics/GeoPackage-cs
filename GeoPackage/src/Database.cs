
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


        public List<Dictionary<string, object>> SpatialReferenceSystems()
        {
            var result = new List<Dictionary<string, object>>();
            var cmd = new SQLiteCommand("SELECT * FROM gpkg_spatial_ref_sys", Connection);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; ++i)
                    row[reader.GetName(i)] = reader.GetValue(i);
                result.Add(row);
            }
            return result;
        }

            //- gpkg_spatial_ref_sys
            //- gpkg_contents
            //- gpkg_geometry_columns
            //- feature tables


    }
}
