using MapGenearionLibrary.Enums;
using MapGenerationLibrary.Enums;
using MapGenerationLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

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

            return map.Width * location.y + location.x;
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

        public static double GetRoomsDistance(this MapRoom room1, MapRoom room2)
        {
            var center1 = GetRoomCenter(room1);
            var center2 = GetRoomCenter(room2);

            var deltaX = center1.X - center2.X;
            var deltaY = center1.Y - center2.Y;

            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        public static bool DoesDoorContainsPoint(this MapDoor door, MapPoint point) =>
            door.Area1.AreEqual(point) || door.Area2.AreEqual(point);

        internal static int GetLocatibleLeaveIndex(this ILocatiable baseObject, ILocatiable comparingObject, SeperationParameterEnum parameter)
        {
            int baseStartLocation = 0;
            int baseEndLocation = 0;
            int comparingStartLocation = 0;
            int comparingEndLocation = 0;

            if (parameter == SeperationParameterEnum.X)
            {
                baseStartLocation = baseObject.StartLocation.X;
                comparingStartLocation = comparingObject.StartLocation.X;
                baseEndLocation = baseObject.StartLocation.X + baseObject.Size.X - 1;
                comparingEndLocation = comparingObject.StartLocation.X + comparingObject.Size.X - 1;
            }
            else if (parameter == SeperationParameterEnum.Y)
            {
                baseStartLocation = baseObject.StartLocation.Y;
                comparingStartLocation = comparingObject.StartLocation.Y;
                baseEndLocation = baseObject.StartLocation.Y + baseObject.Size.Y - 1;
                comparingEndLocation = comparingObject.StartLocation.Y + comparingObject.Size.Y - 1;
            }

            if (comparingStartLocation < baseStartLocation)
            {
                if (comparingEndLocation < baseStartLocation)
                {
                    return 0;
                }
                else if (comparingEndLocation <= baseEndLocation)
                {
                    return 1;
                }
                else
                {
                    return 2;
                }
            }
            else if (comparingStartLocation > baseEndLocation)
            {
                return 5;
            }
            else
            {
                if (comparingEndLocation <= baseEndLocation)
                {
                    return 3;
                }
                else
                {
                    return 4;
                }
            }
        }

        internal static IEnumerable<int> GetLocatibleLeaveIndexCollection(this ILocatiable baseObject, MapPoint point, SeperationParameterEnum parameter)
        {
            int baseStartLocation = 0;
            int baseEndLocation = 0;
            int pointValue = 0;

            if (parameter == SeperationParameterEnum.X)
            {
                baseStartLocation = baseObject.StartLocation.X;
                baseEndLocation = baseObject.StartLocation.X + baseObject.Size.X - 1;
                pointValue = point.X;
            }
            else if (parameter == SeperationParameterEnum.Y)
            {
                baseStartLocation = baseObject.StartLocation.Y;
                baseEndLocation = baseObject.StartLocation.Y + baseObject.Size.Y - 1;
                pointValue = point.Y;
            }

            if (pointValue < baseStartLocation)
            {
                yield return 0;
                yield return 1;
                yield return 2;
            }
            else if (pointValue > baseEndLocation)
            {
                yield return 2;
                yield return 4;
                yield return 5;
            }
            else
            {
                yield return 1;
                yield return 2;
                yield return 3;
                yield return 4;
            }
        }

        public static bool DoesLocatibleContainsPoint(this ILocatiable baseObject, MapPoint point)
        {
            var startX = baseObject.StartLocation.X;
            var endX = baseObject.StartLocation.X + baseObject.Size.X;
            var startY = baseObject.StartLocation.Y;
            var endY = baseObject.StartLocation.Y + baseObject.Size.Y;

            return point.X >= startX && point.X < endX
                   && point.Y >= startY && point.Y < endY;
        }

        public static string GetString(this ILocatiable locatible) => $"<{locatible.StartLocation}, [{locatible.StartLocation.X + locatible.Size.X - 1}, {locatible.StartLocation.Y + locatible.Size.Y - 1}]>";
    }
}
