using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenearionLibrary.Base
{
    public class MapPoint
    {
        public int X { get; private set; } = 0;

        public int Y { get; private set; } = 0;

        public MapPoint() { }

        public MapPoint(int x, int y) => Set(x, y);

        public void Set(int x, int y)
        {
            X = x;
            Y = y;
        }

        public MapPoint Clone() => new MapPoint(X, Y);

        public bool Compare(MapPoint anotherPoint) => X == anotherPoint.X && Y == anotherPoint.Y;

        public override int GetHashCode()
        {
            return (X & 0xFFFF) | ((Y & 0xFFFF) << 16);
        }

        public long GetUniqCode()
        {
            return (long)(X) | ((long)(Y) << 32);
        }
    }
}
