using MapGenearionLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenearionLibrary.Base
{
    public class MapBuilder
    {
        public static void SetWall(Map field, MapPoint point, CellWallOrientationEnum orientation)
        {
            field.GetCell(point.X, point.Y)?.SetWall(orientation, true);

            if (orientation == CellWallOrientationEnum.Top)
            {
                field.GetCell(point.X, point.Y - 1)?.SetWall(3 - orientation, true);
            }
            else if (orientation == CellWallOrientationEnum.Bottom)
            {
                field.GetCell(point.X, point.Y + 1)?.SetWall(3 - orientation, true);
            }
            else if (orientation == CellWallOrientationEnum.Left)
            {
                field.GetCell(point.X - 1, point.Y)?.SetWall(3 - orientation, true);
            }
            else if (orientation == CellWallOrientationEnum.Right)
            {
                field.GetCell(point.X + 1, point.Y)?.SetWall(3 - orientation, true);
            }
        }

        public static void SetLongWall(Map field, MapPoint point1, MapPoint point2, CellWallOrientationEnum orientation)
        {
            if (point1.X == point2.X)
            {
                var baseX = point1.X;
                var baseY = Math.Min(point1.Y, point2.Y);

                foreach (var i in Enumerable.Range(0, Math.Abs(point1.Y - point2.Y)))
                {
                    SetWall(field, new MapPoint(baseX, baseY + i), orientation);
                }
            }
            else if (point1.Y == point2.Y)
            {
                var baseX = Math.Min(point1.X, point2.X);
                var baseY = point2.Y;

                foreach (var i in Enumerable.Range(0, Math.Abs(point1.X - point2.X)))
                {
                    SetWall(field, new MapPoint(baseX + i, baseY), orientation);
                }
            }
        }

        public static MapPoint[] SetLongWallWithDoors(Map field, Random rnd, int doorsCount, MapPoint point1, MapPoint point2, CellWallOrientationEnum orientation)
        {
            if (point1.X == point2.X)
            {
                var baseX = point1.X;
                var baseY = Math.Min(point1.Y, point2.Y);
                var wallLength = Math.Abs(point1.Y - point2.Y);
                var doorsLocation = Enumerable.Range(0, Math.Max(1, doorsCount)).Select(_ => rnd.Next(wallLength)).GroupBy(v => v).Select(p => p.Key).ToArray();

                var doorPoints = new MapPoint[doorsLocation.Length];
                int doorIndex = 0;

                foreach (var i in Enumerable.Range(0, wallLength))
                {
                    if (!doorsLocation.Contains(i))
                    {
                        SetWall(field, new MapPoint(baseX, baseY + i), orientation);
                    }
                    else
                    {
                        doorPoints[doorIndex] = new MapPoint(baseX, baseY + i);
                        doorIndex++;
                    }
                }

                return doorPoints;
            }
            else if (point1.Y == point2.Y)
            {
                var baseX = Math.Min(point1.X, point2.X);
                var baseY = point1.Y;
                var wallLength = Math.Abs(point1.X - point2.X);
                var doorsLocation = Enumerable.Range(0, Math.Max(1, doorsCount)).Select(_ => rnd.Next(wallLength)).GroupBy(v => v).Select(p => p.Key).ToArray();

                var doorPoints = new MapPoint[doorsLocation.Length];
                int doorIndex = 0;

                foreach (var i in Enumerable.Range(0, wallLength))
                {
                    if (!doorsLocation.Contains(i))
                    {
                        SetWall(field, new MapPoint(baseX + i, baseY), orientation);
                    }
                    else
                    {
                        doorPoints[doorIndex] = new MapPoint(baseX + i, baseY);
                        doorIndex++;
                    }
                }

                return doorPoints;
            }

            return Array.Empty<MapPoint>();
        }

        public static void ResetFieldWalls(Map field) => field.Foreach((point, cell) => cell.ResetWalls());
    }
}
