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
        public MapRoom RoomFrom { get; internal set; }

        public MapRoom RoomTo { get; internal set; }

        public MapPoint PointFrom { get; internal set; }

        public MapPoint PointTo { get; internal set; }

        public MapPoint[] PointsSequence { get; internal set; }

        public double Distance { get; set; }

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
