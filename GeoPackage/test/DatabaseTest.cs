
using NUnit.Framework;

namespace Cognitics.GeoPackage.test
{
    public class DatabaseTest
    {
        [Test]
        public void testDatabase()
        {
            string filename = "D:/GGDM_GeoPackage_Korea4.gpkg";
            var db = new Database(filename);
            var srs = db.SpatialReferenceSystems();
            var contents = db.Contents();


            Assert.Fail();
        }


    }
}
