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

        public MapSeperationArea[] Seperate(Func<int, int, int, int, CellWallOrientationEnum, MapPoint[]> longWallCreationHandler)
        {
            if (Width <= 1 || Height <= 1)
            {
                return ChildAreas;
            }
            else
            {
                ChildAreas = new MapSeperationArea[2];

                int randowDirection = _Rnd.Next() % 2;

                if (Width > Height
                    || Width == Height && randowDirection == 0)
                {
                    int seperationLinePosition = Math.Max(1, _Rnd.Next(Width));

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
                         || Width == Height && randowDirection == 1)
                {
                    int seperationLinePosition = Math.Max(1, _Rnd.Next(Height));

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
