
using System;
using System.Linq;
using NUnit.Framework;
using System.Diagnostics;

namespace Cognitics.GeoPackage.test
{
    [TestFixture]
    public class DatabaseTest
    {
        //public static string filename = "D:/GGDM_GeoPackage_Korea4.gpkg";
        //public static string filename = "D:/natural_earth_vector.4.1.0.gpkg";
        public static string filename = "D:/NRL_BlueMarble-3395_GLOBAL_0-6_v1-0_18OCT2017.gpkg";
        Database Database = null;

        [SetUp]
        public void SetUp()
        {
            Database = new Database(filename);
            Database.ApplicationSpatialReferenceSystem = Database.SpatialReferenceSystem(4326);
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void TestFeatures()
        {
            foreach (FeatureLayer layer in Database.FeatureLayers())
            {
                Debug.WriteLine("FEATURE LAYER " + layer.TableName);
                var geometryColumn = layer.GeometryColumn();
                if(geometryColumn != null)
                    Debug.WriteLine("GEOMETRYCOLUMN " + geometryColumn.ColumnName + " (" + geometryColumn.GeometryTypeName + ")");
                foreach (var field in layer.Fields())
                    Debug.WriteLine("FIELD " + field.Item1 + " (" + field.Item2 + ")");
                //foreach (var feature in layer.Features())
                int count = 0;
                foreach (var feature in layer.Features(-80, -70, -90, -80))
                {
                    ++count;
                    Debug.WriteLine("FEATURE " + feature.Attributes["fid"] + ": " + feature.Geometry.ToString());

                    //break;
                }
                Debug.WriteLine("FEATURE COUNT = " + count);

                break;
            }
        }

        [Test]
        public void TestRaster()
        {
            foreach (RasterLayer layer in Database.RasterLayers())
            {
                Debug.WriteLine("RASTER LAYER " + layer.TableName);
                foreach (var tileMatrix in layer.TileMatrices())
                    Debug.WriteLine(string.Format("TILEMATRIX ZoomLevel={0} TilesWide={1} TilesHigh={2} TileWidth={3} TileHeight={4} PixelXSize={5} PixelYSize={6}",
                        tileMatrix.ZoomLevel, tileMatrix.TilesWide, tileMatrix.TilesHigh, tileMatrix.TileWidth, tileMatrix.TileHeight, tileMatrix.PixelXSize, tileMatrix.PixelYSize));
                foreach (var zoomLevel in layer.ZoomLevels())
                    Debug.WriteLine("ZOOMLEVEL " + zoomLevel);
                foreach (var tile in layer.Tiles())
                {
                    Debug.WriteLine(string.Format("Tiles(): TILE ID={0} ZoomLevel={1} TileColumn={2} TileRow={3} Bytes={4}", tile.ID, tile.ZoomLevel, tile.TileColumn, tile.TileRow, tile.Bytes.Length));
                    break;
                }
                foreach (var tile in layer.Tiles(1))
                {
                    Debug.WriteLine(string.Format("Tiles(1): TILE ID={0} ZoomLevel={1} TileColumn={2} TileRow={3} Bytes={4}", tile.ID, tile.ZoomLevel, tile.TileColumn, tile.TileRow, tile.Bytes.Length));
                }

            }
            return;
        }



        /*
        [Test] public void TestDatabase()
        {
            int layerCount = 0;
            foreach (Layer layer in Database.Layers(-180.0f, -170.0f, -90.0f, -80.0f))
            {
                Debug.WriteLine("LAYER " + layer.TableName);
                ++layerCount;
            }

            var srs = Database.SpatialReferenceSystems().ToList();
            var srs_dict = Database.SpatialReferenceSystems().ToDictionary(entry => entry.ID);
            var contents = Database.Layers().ToList();
            var contents_dict = Database.Layers().ToDictionary(entry => entry.TableName);
            var geometryColumns = Database.GeometryColumns().ToList();
            var geometryColumns_dict = Database.GeometryColumns().ToDictionary(entry => new Tuple<string, string>(entry.TableName, entry.ColumnName));
            var geometryColumns_CASTLE_S = Database.GeometryColumns("CASTLE_S").ToList();
            var geometryColumns_CASTLE_S_dict = Database.GeometryColumns("CASTLE_S").ToDictionary(entry => entry.ColumnName);

            Assert.Fail();
        }
        */


    }
}
