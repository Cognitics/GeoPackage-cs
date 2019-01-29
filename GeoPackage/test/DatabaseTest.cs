
using System;
using System.Linq;
using NUnit.Framework;

namespace Cognitics.GeoPackage.test
{
    public class DatabaseTest
    {
        [Test]
        public void TestDatabase()
        {
            // TODO: fixture
            string filename = "D:/GGDM_GeoPackage_Korea4.gpkg";
            var db = new Database(filename);
            var srs = db.SpatialReferenceSystems().ToList();
            var srs_dict = db.SpatialReferenceSystems().ToDictionary(entry => entry.ID);
            var contents = db.Layers().ToList();
            var contents_dict = db.Layers("features").ToDictionary(entry => entry.TableName);
            var geometryColumns = db.GeometryColumns().ToList();
            var geometryColumns_dict = db.GeometryColumns().ToDictionary(entry => new Tuple<string, string>(entry.TableName, entry.ColumnName));
            var geometryColumns_CASTLE_S = db.GeometryColumns("CASTLE_S").ToList();
            var geometryColumns_CASTLE_S_dict = db.GeometryColumns("CASTLE_S").ToDictionary(entry => entry.ColumnName);
            var features = db.Features(contents_dict["CASTLE_S"]).ToList();
            var features_dict = db.Features(contents_dict["CASTLE_S"]).ToDictionary(entry => entry.Attributes["OBJECTID"]);


            Assert.Fail();
        }


    }
}
