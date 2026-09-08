using MapGenearionLibrary.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenearionLibrary.Navigation
{
    public class MapNavigationGraphPath
    {
        public required MapRoom RoomFrom { get; init; }

        public required MapRoom RoomTo { get; init; }

        public required MapPoint PointFrom { get; init; }

        public required MapPoint PointTo { get; init; }

        public required MapPoint[] PointsSequence { get; init; }

        public double Distance { get; init; }

        internal MapNavigationGraphPath() { }

        public MapPoint[] GetStepPoints()
        {
            List<MapPoint> stepPoints = new();
            int pointIndex = 1;
            MapPoint currentPoint = PointsSequence.First().Clone();

            while (PointsSequence.Length > pointIndex)
            {
                stepPoints.Add(currentPoint.Clone());

                if (currentPoint.Compare(PointsSequence[pointIndex]))
                {
                    pointIndex++;
                }
                else
                {
                    var delta = MapPointOperations.GetDelta(PointsSequence[pointIndex], currentPoint);
                
                    if (Math.Abs(delta.X) > Math.Abs(delta.Y))
                    {
                        currentPoint.Set(currentPoint.X + (delta.X > 0 ? 1 : -1), currentPoint.Y);
                    }
                    else
                    {
                        currentPoint.Set(currentPoint.X, currentPoint.Y + (delta.Y > 0 ? 1 : -1));
                    }
                }
            }

            return stepPoints.ToArray();
        }
    }
}
