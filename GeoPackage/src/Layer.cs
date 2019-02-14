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

        public IEnumerable<Tuple<string, string>> Fields()
        {
            using (var tableSchema = Database.Connection.GetSchema("Columns", new string[] { null, null, TableName }))
                foreach(DataRow row in tableSchema.Rows)
                    yield return new Tuple<string, string>(row["COLUMN_NAME"].ToString(), row["DATA_TYPE"].ToString());
        }

        #region implementation

        internal ICoordinateTransformation TransformFrom = null;
        internal ICoordinateTransformation TransformTo = null;

        internal Layer(Database database)
        {
            Database = database;
        }

        #endregion


    }
}
