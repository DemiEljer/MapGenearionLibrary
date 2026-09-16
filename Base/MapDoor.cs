using MapGenearionLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenearionLibrary.Base
{
    public class MapDoor
    {
        public MapPoint Area1 { get; }

        public MapPoint Area2 { get; }

        public CellWallOrientationEnum Orientation { get; }

        public bool IsObstacled { get; internal set; } = false;

        public MapDoor(MapPoint area1, MapPoint area2, CellWallOrientationEnum orientation)
        {
            Area1 = area1;
            Area2 = area2;
            Orientation = orientation;
        }
    }
}
