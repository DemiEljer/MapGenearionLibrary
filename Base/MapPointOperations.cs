using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenearionLibrary.Base
{
    public static class MapPointOperations
    {
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
