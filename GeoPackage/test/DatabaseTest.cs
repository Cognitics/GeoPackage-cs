
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
            var srs = db.SpatialReferenceSystems();
            var contents = db.Contents();
            var geometryColumns = db.GeometryColumns();


            Assert.Fail();
        }


    }
}
