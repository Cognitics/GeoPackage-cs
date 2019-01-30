using System;
using System.Collections.Generic;
using System.Text;

namespace Cognitics.GeoPackage
{
    public class TileMatrix
    {
        string tableName;
        int zoomLevel;
        int tilesWide;
        int tilesHigh;

        int tileWidth;
        int tileHeight;

        double pixelXSize;
        double pixelYSize;

        public string TableName { get => tableName; set => tableName = value; }
        public int ZoomLevel { get => zoomLevel; set => zoomLevel = value; }
        public int TilesWide { get => tilesWide; set => tilesWide = value; }
        public int TilesHigh { get => tilesHigh; set => tilesHigh = value; }
        public int TileWidth { get => tileWidth; set => tileWidth = value; }
        public int TileHeight { get => tileHeight; set => tileHeight = value; }
        public double PixelXSize { get => pixelXSize; set => pixelXSize = value; }
        public double PixelYSize { get => pixelYSize; set => pixelYSize = value; }
    }
}
