using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MapGenearionLibrary.Base
{
    public static class MapPointOperations
    {
        public static (int x, int y) GetMapCellLocation(this Map map, int x, int y)
        {
            if (map is null 
                || map.Width == 0
                || map.Height == 0)
            {
                return (-1, -1);
            }

            x = x % map.Width;
            y = y % map.Height;

            x = x < 0 ? x + map.Width : x;
            y = y < 0 ? y + map.Height : y;

            return (x, y);
        }

        public static int GetMapCellLocationIndex(this Map map, int x, int y)
        {
            var location = GetMapCellLocation(map, x, y);

            return map.Width * y + x;
        }

        public static double GetDistance(this MapPoint point1, MapPoint point2)
        {
            double deltaX = point1.X - point2.X;
            double deltaY = point1.Y - point2.Y;

            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        public static MapPoint GetDelta(this MapPoint point1, MapPoint point2) =>
            new MapPoint(point1.X - point2.X, point1.Y - point2.Y);

        public static bool AreEqual(this MapPoint point1, MapPoint point2) => 
            point1.X == point2.X && point1.Y == point2.Y;

        public static bool DoesRoomContainsPoint(this MapRoom room, MapPoint point)
        {
            var startX = room.StartX;
            var endX = room.StartX + room.Width;
            var startY = room.StartY;
            var endY = room.StartY + room.Height;

            return point.X >= startX && point.X < endX
                   && point.Y >= startY && point.Y < endY;
        }

        public static bool DoesRoomContainsDoor(this MapRoom room, MapDoor door) =>
            DoesRoomContainsPoint(room, door.Area1) 
            || DoesRoomContainsPoint(room, door.Area2);

        public static MapPoint GetRoomCenter(this MapRoom room) =>
            new MapPoint(room.StartX + room.Width / 2, room.StartY + room.Height / 2);
    }
}
