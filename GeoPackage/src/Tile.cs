using System;
using System.Collections.Generic;
using System.Text;

namespace Cognitics.GeoPackage
{
    public class Tile
    {
        public long ID;
        public long ZoomLevel;
        public long TileColumn;
        public long TileRow;
        public byte[] Bytes;

    }
}
