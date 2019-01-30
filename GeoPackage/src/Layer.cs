using System;
using System.Data;
using System.Collections.Generic;
using ProjNet.CoordinateSystems.Transformations;

namespace Cognitics.GeoPackage
{
    public class Layer
    {
        public string TableName;
        public string DataType;
        public string Identifier;
        public string Description;
        public DateTime LastChange;
        public double MinX;
        public double MinY;
        public double MaxX;
        public double MaxY;
        public long SpatialReferenceSystemID;

        public readonly Database Database;

        public SpatialReferenceSystem SpatialReferenceSystem => Database.SpatialReferenceSystem(SpatialReferenceSystemID);
        public IEnumerable<GeometryColumn> GeometryColumns() => Database.GeometryColumns(TableName);

        internal ICoordinateTransformation TransformFrom = null;
        internal ICoordinateTransformation TransformTo = null;

        internal Layer(Database database)
        {
            Database = database;
            if (Database.ApplicationSpatialReferenceSystem != null)
            {
                var layerSpatialReferenceSystem = SpatialReferenceSystem;
                if (layerSpatialReferenceSystem != null)
                {
                    var layerSRS = SpatialReferenceSystem.ProjNetCoordinateSystem(layerSpatialReferenceSystem.Definition);
                    var appSRS = SpatialReferenceSystem.ProjNetCoordinateSystem(Database.ApplicationSpatialReferenceSystem.Definition);
                    TransformFrom = SpatialReferenceSystem.ProjNetTransform(layerSRS, appSRS);
                    TransformTo = SpatialReferenceSystem.ProjNetTransform(appSRS, layerSRS);
                }
            }
        }

        public IEnumerable<Feature> Features()
        {
            // *** WARNING *** : table name cannot be parameterized ; this is vulnerable to sql injection
            using (var cmd = Database.Connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM " + TableName;
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
