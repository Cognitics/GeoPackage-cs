using System;
using System.Collections.Generic;
using System.Text;
using ProjNet.CoordinateSystems;
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



    }
}
