using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
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
                    if (layerSpatialReferenceSystem.ID == 0)
                        layerSpatialReferenceSystem = Database.SpatialReferenceSystem(4326);
                    //if (layerSpatialReferenceSystem.Definition != Database.ApplicationSpatialReferenceSystem.Definition)
                    {
                        try
                        {
                            var layerSRS = SpatialReferenceSystem.ProjNetCoordinateSystem(layerSpatialReferenceSystem.Definition);
                            var appSRS = SpatialReferenceSystem.ProjNetCoordinateSystem(Database.ApplicationSpatialReferenceSystem.Definition);
                            TransformFrom = SpatialReferenceSystem.ProjNetTransform(layerSRS, appSRS);
                            TransformTo = SpatialReferenceSystem.ProjNetTransform(appSRS, layerSRS);
                        }
                        catch (ArgumentException) { }
                    }
                }
            }
        }

        public GeometryColumn GeometryColumn()
        {
            foreach (var geometryColumn in GeometryColumns())
                return geometryColumn;
            return null;
        }

        public IEnumerable<Tuple<string, string>> Fields()
        {
            using (var tableSchema = Database.Connection.GetSchema("Columns", new string[] { null, null, TableName }))
            {
                foreach(DataRow row in tableSchema.Rows)
                {
                    yield return new Tuple<string, string>(row["COLUMN_NAME"].ToString(), row["DATA_TYPE"].ToString());
                }
            }
        }



    }
}
