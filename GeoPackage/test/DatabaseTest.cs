
using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;

namespace Cognitics.GeoPackage.test
{
    public class DatabaseTest
    {
        // TODO: fixture

        [Test]
        public void TestDatabase()
        {
            //string filename = "D:/GGDM_GeoPackage_Korea4.gpkg";
            string filename = "D:/natural_earth_vector.4.1.0.gpkg";
            var db = new Database(filename);
            db.ApplicationSpatialReferenceSystem = db.SpatialReferenceSystem(4326);
            if (false)
            {
                int layerCount = 0;
                foreach (Layer layer in db.Layers(-180.0f, -170.0f, -90.0f, -80.0f))
                {
                    System.Diagnostics.Debug.WriteLine("LAYER " + layer.TableName);
                    ++layerCount;
                }
            }
            if (true)
            {
                foreach (FeatureLayer layer in db.FeatureLayers())
                {
                    System.Diagnostics.Debug.WriteLine("LAYER " + layer.TableName);
                    foreach (var geometryColumn in layer.GeometryColumns())
                        System.Diagnostics.Debug.WriteLine("GEOMETRYCOLUMN " + geometryColumn.ColumnName + " (" + geometryColumn.GeometryTypeName + ")");
                    foreach (var field in layer.Fields())
                        System.Diagnostics.Debug.WriteLine("FIELD " + field.Item1 + " (" + field.Item2 + ")");
                    //foreach (var feature in layer.Features())
                    int count = 0;
                    foreach (var feature in layer.Features(-80, -70, -90, -80))
                    {
                        ++count;
                        System.Diagnostics.Debug.WriteLine("FEATURE " + feature.Attributes["fid"] + ": " + feature.Geometry.ToString());

                        //break;
                    }
                    System.Diagnostics.Debug.WriteLine("FEATURE COUNT = " + count);

                    break;
                }
            }







            var srs = db.SpatialReferenceSystems().ToList();
            var srs_dict = db.SpatialReferenceSystems().ToDictionary(entry => entry.ID);
            var contents = db.Layers().ToList();
            var contents_dict = db.Layers().ToDictionary(entry => entry.TableName);
            var geometryColumns = db.GeometryColumns().ToList();
            var geometryColumns_dict = db.GeometryColumns().ToDictionary(entry => new Tuple<string, string>(entry.TableName, entry.ColumnName));
            var geometryColumns_CASTLE_S = db.GeometryColumns("CASTLE_S").ToList();
            var geometryColumns_CASTLE_S_dict = db.GeometryColumns("CASTLE_S").ToDictionary(entry => entry.ColumnName);






            Assert.Fail();
        }


    }
}
