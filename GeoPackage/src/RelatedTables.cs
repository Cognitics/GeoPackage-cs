using System;
using System.Collections.Generic;
using System.Text;

namespace Cognitics.GeoPackage
{
    public class RelatedTablesRelationship
    {
        public string baseTableName;
        public string baseTableColumn;
        public string relatedTableName;
        public string relatedTableColumn;
        public string relationshipName;
        public string mappingTableName;
    }

    class RelatedTables
    {

        private Database database;
        public RelatedTables(string fileName)
        {
            database = new Database(fileName);
        }
        public Boolean CreateSchema()
        {
            // First see if the schema already exists.
            var statement = database.Connection.Prepare("select * from gpkg_extensions WHERE extension_name='related_tables'");
            statement.Execute();
            if (!statement.Reader.HasRows)
            {
                database.Connection.Execute("CREATE TABLE IF NOT EXISTS 'gpkgext_relations' "
                    + "( id INTEGER PRIMARY KEY AUTOINCREMENT, base_table_name TEXT NOT NULL, "
                    + "base_primary_column TEXT NOT NULL DEFAULT 'id', related_table_name TEXT NOT NULL, "
                    + "related_primary_column TEXT NOT NULL DEFAULT 'id', relation_name TEXT NOT NULL, "
                    + "mapping_table_name TEXT NOT NULL UNIQUE )");
            }
            
            return true;
        }
        public void AddMediaTableIfNotExists(String name)
        {
            // Create the mapping table
            string query = "CREATE TABLE IF NOT EXISTS '" + name
                + "' ( id INTEGER PRIMARY KEY AUTOINCREMENT, data BLOB NOT NULL, content_type TEXT NOT NULL )";
            database.Connection.Execute(query);
        }

        /**
        *
        * @param mediaTable Name of the table to insert into
        * @param blob Binary data to insert into the 'data' column
        * @param contentType The type of content
        * @return
        */
        public long AddMedia(String mediaTable, byte[] blob, String contentType)
        {
            var statement = database.Connection.Prepare("INSERT INTO " + mediaTable + " (data,content_type) VALUES(@data,@value)");
            statement.AddParameter("@data", blob);
            statement.AddParameter("@value", contentType);
            if (statement.Next())
            {
                return statement.Value("id", (long)0);
            }
            return 0;
        }

        public IEnumerable<long> GetRelatedFeatureIds(string mappingTable, long featureId)
        {
            using (var statement = database.Connection.Prepare("SELECT * FROM " + mappingTable + "where base_id=@base_id"))
            {
                statement.AddParameter("@base_id", featureId);
                statement.Execute();
                while (statement.Next())
                    yield return statement.Value("related_id", 0);
            }
        }

        public IEnumerable<RelatedTablesRelationship> GetRelationships(string layer)
        {
            string query = "select * from gpkgext_relations WHERE base_table_name='" + layer + "'";

            using (var statement = database.Connection.Prepare(query))
            {
                
                statement.Execute();
                while (statement.Next())
                {
                    RelatedTablesRelationship relationship = new RelatedTablesRelationship();
                    relationship.baseTableColumn = statement.Value("base_primary_column", "");
                    relationship.relatedTableName = statement.Value("related_table_name", "");
                    relationship.relatedTableColumn = statement.Value("related_primary_column", "");
                    relationship.relationshipName = statement.Value("relation_name", "");
                    relationship.mappingTableName = statement.Value("mapping_table_name", "");
                    yield return relationship;
                }
            }
        }

        public void AddRelationship(RelatedTablesRelationship relationship)
        {
            // Create the mapping table
            string query = "CREATE TABLE IF NOT EXISTS '" + relationship.mappingTableName 
                + "' ( base_id INTEGER NOT NULL, related_id INTEGER NOT NULL )";
            database.Connection.Execute(query);
            // Add to the gpkgext_relationships table
            query = "INSERT INTO gpkgext_relations (base_table_name, base_primary_column," +
                "related_table_name,related_primary_column,relation_name,mapping_table_name)" +
                "VALUES(@base_table_name,@base_primary_column,@related_table_name," +
                "@related_primary_column,@relation_name,@mapping_table_name)";
            var statement = database.Connection.Prepare(query);
            statement.AddParameter("@base_table_name", relationship.baseTableName);
            statement.AddParameter("@base_primary_column", relationship.baseTableColumn);
            statement.AddParameter("@related_table_name", relationship.relatedTableName);
            statement.AddParameter("@related_primary_column", relationship.relatedTableColumn);
            statement.AddParameter("@relation_name", relationship.relationshipName);
            statement.AddParameter("@mapping_table_name", relationship.mappingTableName);
            statement.Execute();
        }

        public void AddFeatureRelationship(RelatedTablesRelationship relationship, long baseFID, long relatedFID)
        {
            string query = "INSERT INTO " + relationship.mappingTableName + " (base_id,related_id) VALUES(@base_id,@related_id)";
            var statement = database.Connection.Prepare(query);
            statement.AddParameter("@base_table_name", baseFID);
            statement.AddParameter("@base_primary_column", relatedFID);
            statement.Execute();
        }
    }
}
