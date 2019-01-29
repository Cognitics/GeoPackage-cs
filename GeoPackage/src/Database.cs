
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

        public IEnumerable<Table> Contents()
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM gpkg_contents";
                cmd.CommandType = CommandType.Text;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var result = new Table
                        {
                            TableName = GetFieldValue(reader, reader.GetOrdinal("table_name"), ""),
                            DataType = GetFieldValue(reader, reader.GetOrdinal("data_type"), ""),
                            Identifier = GetFieldValue(reader, reader.GetOrdinal("identifier"), ""),
                            Description = GetFieldValue(reader, reader.GetOrdinal("description"), ""),
                            LastChange = reader.GetDateTime(reader.GetOrdinal("last_change")),
                            MinX = GetFieldValue(reader, reader.GetOrdinal("min_x"), double.MinValue),
                            MinY = GetFieldValue(reader, reader.GetOrdinal("min_y"), double.MinValue),
                            MaxX = GetFieldValue(reader, reader.GetOrdinal("max_x"), double.MaxValue),
                            MaxY = GetFieldValue(reader, reader.GetOrdinal("max_y"), double.MaxValue),
                            SpatialReferenceSystemID = GetFieldValue(reader, reader.GetOrdinal("srs_id"), (long)0)
                        };
                        yield return result;
                    }
                }
            }
        }

        public IEnumerable<GeometryColumn> GeometryColumns()
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM gpkg_geometry_columns";
                cmd.CommandType = CommandType.Text;
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

        public IEnumerable<GeometryColumn> GeometryColumns(string tableName)
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM gpkg_geometry_columns WHERE table_name=@table_name";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new SQLiteParameter("@table_name", tableName));
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






            //- feature tables

                /*
                for (int i = 0; i < reader.FieldCount; ++i)
                    row[reader.GetName(i)] = reader.GetValue(i);
                */

    }
}
