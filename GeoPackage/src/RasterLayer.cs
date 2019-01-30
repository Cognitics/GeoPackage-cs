using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using System.Linq;

namespace Cognitics.GeoPackage
{
    public class RasterLayer : Layer
    {
        private Dictionary<int, TileMatrix> zoomLevels = null;
        internal RasterLayer(Database database) : base(database)
        {
            Database.SetRasterExtentsFromTileMatrixSet(this);
            zoomLevels = new Dictionary<int, TileMatrix>();
            var tileSet = database.TileMatrixSet(TableName);
            foreach (TileMatrix tm in tileSet)
            {
                zoomLevels[tm.ZoomLevel] = tm;
            }
        }
    }
}
