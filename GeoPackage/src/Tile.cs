using System;
using System.Collections.Generic;
using System.Text;

namespace Cognitics.GeoPackage
{
    public class Tile
    {
        int id;
        int zoomLevel;
        int tileColumn;
        int tileRow;
        Byte[] bytes;

        public int Id { get => id; set => id = value; }
        public int ZoomLevel { get => zoomLevel; set => zoomLevel = value; }
        public int TileColumn { get => tileColumn; set => tileColumn = value; }
        public int TileRow { get => tileRow; set => tileRow = value; }
        public Byte[] Bytes { get => bytes; set => bytes = value; }
    }
}
