
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
            string filename = "D:/GGDM_GeoPackage_Korea4.gpkg";
            var db = new Database(filename);
            foreach(FeatureLayer layer in db.FeatureLayers())
            {
                var log = new List<string>();
                log.Add("LAYER " + layer.TableName);
                foreach (var geometryColumn in layer.GeometryColumns())
                    log.Add("GEOMETRYCOLUMN " + geometryColumn.ColumnName + " (" + geometryColumn.GeometryTypeName + ")");
                foreach (var field in layer.Fields())
                    log.Add("FIELD " + field.Item1 + " (" + field.Item2 + ")");
                foreach (var feature in layer.Features())
                {
                    log.Add("FEATURE " + feature.Attributes["OBJECTID"]);

                    break;
                }

                break;
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
