using MapGenearionLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenearionLibrary.Base
{
    public class MapSeperationArea : MapRoom
    {
        private Random _Rnd { get; }

        public MapSeperationArea? Parent { get; private set; } = null;

        public MapSeperationArea[] ChildAreas { get; private set; } = Array.Empty<MapSeperationArea>();

        public MapSeperationArea(Random rnd)
        {
            _Rnd = rnd;
        }

        public MapSeperationArea[] Seperate(Func<int, int, int, int, CellWallOrientationEnum, MapPoint[]> longWallCreationHandler, int widthLimitation, int heightLimitation)
        {
            if (Width <= 1 || Height <= 1)
            {
                return ChildAreas;
            }
            else
            {
                int randomDirection = _Rnd.Next() % 2;

                if (Width > Height
                    || Width == Height && randomDirection == 0)
                {
                    if (Width <= widthLimitation)
                    {
                        return ChildAreas;
                    }

                    ChildAreas = new MapSeperationArea[2];

                    int seperationLinePosition = Math.Max(widthLimitation, _Rnd.Next(Width) - widthLimitation + 1);

                    var doorPoins = longWallCreationHandler(
                        StartX + seperationLinePosition
                        ,
                        StartY
                        ,
                        StartX + seperationLinePosition
                        ,
                        StartY + Height
                        ,
                        CellWallOrientationEnum.Left);

                    SetDoors(doorPoins
                        .Select(p => new MapDoor(p, new MapPoint(p.X - 1, p.Y)))
                        .ToArray());

                    ChildAreas[0] = new MapSeperationArea(_Rnd)
                    {
                        StartX = StartX
                        ,
                        StartY = StartY
                        ,
                        Width = seperationLinePosition
                        ,
                        Height = Height
                        ,
                        Parent = this
                    };
                    ChildAreas[1] = new MapSeperationArea(_Rnd)
                    {
                        StartX = StartX + seperationLinePosition
                        ,
                        StartY = StartY
                        ,
                        Width = Width - seperationLinePosition
                        ,
                        Height = Height
                        ,
                        Parent = this
                    };
                }
                else if (Width < Height
                         || Width == Height && randomDirection == 1)
                {
                    if (Height <= heightLimitation)
                    {
                        return ChildAreas;
                    }

                    ChildAreas = new MapSeperationArea[2];

                    int seperationLinePosition = Math.Max(heightLimitation, _Rnd.Next(Height) - heightLimitation + 1);

                    var doorPoins = longWallCreationHandler(
                        StartX
                        ,
                        StartY + seperationLinePosition
                        ,
                        StartX + Width
                        ,
                        StartY + seperationLinePosition
                        ,
                        CellWallOrientationEnum.Top);

                    SetDoors(doorPoins
                        .Select(p => new MapDoor(p, new MapPoint(p.X, p.Y - 1)))
                        .ToArray());

                    ChildAreas[0] = new MapSeperationArea(_Rnd)
                    {
                        StartX = StartX
                        ,
                        StartY = StartY
                        ,
                        Width = Width
                        ,
                        Height = seperationLinePosition
                        ,
                        Parent = this
                    };
                    ChildAreas[1] = new MapSeperationArea(_Rnd)
                    {
                        StartX = StartX
                        ,
                        StartY = StartY + seperationLinePosition
                        ,
                        Width = Width
                        ,
                        Height = Height - seperationLinePosition
                        ,
                        Parent = this
                    };
                }

                return ChildAreas;
            }
        }

        internal MapRoom GetRoom() => new MapRoom()
        {
            StartX = StartX
            ,
            StartY = StartY
            ,
            Width = Width
            ,
            Height = Height
        };
    }
}
