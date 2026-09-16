using MapGenearionLibrary.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MapGenearionLibrary.Navigation
{
    public class MapNavigationGraph
    {
        public MapNavigationHandler NavigationHandler { get; }

        public MapNavigationGraphElement[] Nodes { get; }

        public MapNavigationGraph(MapNavigationHandler navigationHandler)
        {
            NavigationHandler = navigationHandler;

            Nodes = navigationHandler.Rooms.Select(room => new MapNavigationGraphElement(room)).ToArray();

            IEnumerable<(MapNavigationGraphElement to, MapDoor door)> GetAllRoomDoorsPairs()
            {
                foreach (var node in Nodes)
                {
                    foreach (var roomDoorPair in node.Room.Doors.Select(door => (node, door)))
                    {
                        yield return roomDoorPair;
                    }
                }
            }

            foreach (var node in Nodes)
            {
                node.RoomsTo = GetAllRoomDoorsPairs().Where(pair => node.Room.Doors.Contains(pair.door) && node != pair.to).ToArray();
            }
        }

        public MapNavigationGraphElement? GetElement(MapPoint point) => Nodes.FirstOrDefault(node => MapPointOperations.DoesRoomContainsPoint(node.Room, point));

        public MapNavigationGraphPath[] GetPathes(MapPoint pointFrom, MapPoint pointTo)
        {
            MapNavigationGraphElement? roomFrom = GetElement(pointFrom);
            MapNavigationGraphElement? roomTo = GetElement(pointTo);

            if (roomFrom is null || roomTo is null)
            {
                return Array.Empty<MapNavigationGraphPath>();
            }
            else
            {
                List<MapNavigationGraphPath> resultPathes = new();
                List<MapNavigationGraphElement> currentElementPath = new();
                List<(MapPoint point, MapRoom room)> currentPointPath = new();
                double minDistanationDistance = double.MaxValue;
                double currentDistance = 0;

                void _PathFinder(MapNavigationGraphElement currentRoom, MapPoint currentPosition)
                {
                    if (currentDistance > minDistanationDistance
                        || currentElementPath.FirstOrDefault(element => element == currentRoom) != null)
                    {
                        return;
                    }

                    currentElementPath.Add(currentRoom);
                    currentPointPath.Add((currentPosition, currentRoom.Room));

                    if (currentRoom == roomTo)
                    {
                        if (currentDistance < minDistanationDistance)
                        {
                            currentPointPath.Add((currentPosition, currentRoom.Room));

                            resultPathes.Add(new MapNavigationGraphPath()
                            {
                                RoomFrom = roomFrom.Room
                                ,
                                RoomTo = roomTo.Room
                                ,
                                PointFrom = pointFrom
                                ,
                                PointTo = pointTo
                                ,
                                PointsSequence = currentPointPath.ToArray()
                                ,
                                Distance = currentDistance
                            });

                            currentPointPath.RemoveAt(currentPointPath.Count - 1);

                            minDistanationDistance = currentDistance;
                            // Удаление более долгих путей
                            for (int i = 0; i < resultPathes.Count;)
                            {
                                if (resultPathes[i].Distance > currentDistance + 0.5)
                                {
                                    resultPathes.RemoveAt(i);
                                }
                                else
                                {
                                    i++;
                                }
                            }
                        }
                    }
                    else
                    {
                        var nextRooms = currentRoom.RoomsTo
                        .Where(room => !NavigationHandler.Obstacles[room.Door.Area1] && !NavigationHandler.Obstacles[room.Door.Area2])
                        .Select(room =>
                        {
                            if (MapPointOperations.DoesRoomContainsPoint(currentRoom.Room, room.Door.Area1))
                            {
                                return (room.Door.Area1, room.Door.Area2, room, MapPointOperations.GetDistance(currentPosition, room.Door.Area2));
                            }
                            else
                            {
                                return (room.Door.Area2, room.Door.Area1, room, MapPointOperations.GetDistance(currentPosition, room.Door.Area2));
                            }
                        })
                        .GroupBy(tuple => tuple.Item3.Element)
                        .Select(group =>
                        {
                            var minDistance = group.Min(value => MapPointOperations.GetDistance(value.Item2, currentPosition) + MapPointOperations.GetDistance(value.Item2, pointTo));

                            return group.Where(value => MapPointOperations.GetDistance(value.Item2, currentPosition) + MapPointOperations.GetDistance(value.Item2, pointTo) <= minDistance).First();
                        }).OrderBy(tuple => tuple.Item4);

                        foreach (var nextRoom in nextRooms)
                        {
                            currentDistance += nextRoom.Item4;

                            if (currentPointPath.Last().point.Compare(nextRoom.Item1))
                            {
                                _PathFinder(nextRoom.Item3.Element, nextRoom.Item2);
                            }
                            else
                            {
                                currentPointPath.Add((nextRoom.Item1, nextRoom.Item3.Element.Room));

                                _PathFinder(nextRoom.Item3.Element, nextRoom.Item2);

                                currentPointPath.RemoveAt(currentPointPath.Count - 1);
                            }

                            currentDistance -= nextRoom.Item4;
                        }
                    }

                    currentElementPath.RemoveAt(currentElementPath.Count - 1);
                    currentPointPath.RemoveAt(currentPointPath.Count - 1);
                }

                _PathFinder(roomFrom, pointFrom);

                return resultPathes.ToArray();
            }
        }

        public MapPoint[] FindPathInRoom(MapPoint pointFrom, MapPoint pointTo) => FindPathInRoom(
            NavigationHandler.GetRoom(pointFrom)
            ,
            pointFrom
            ,
            pointTo
            );

        public MapPoint[] FindPathInRoom(MapRoom room, MapPoint pointFrom, MapPoint pointTo)
        {
            if (room is null || !(room.DoesRoomContainsPoint(pointFrom) && room.DoesRoomContainsPoint(pointTo)))
            {
                return Array.Empty<MapPoint>();
            }

            List<MapPoint> resultPoints = new List<MapPoint>();

            bool _FindPathInRoom(MapPoint currentPoint)
            {
                if (!room.DoesRoomContainsPoint(currentPoint)
                    || NavigationHandler.Obstacles[currentPoint]
                    || resultPoints.FirstOrDefault(point => point.AreEqual(currentPoint)) is not null)
                {
                    return false;
                }

                resultPoints.Add(currentPoint);

                if (currentPoint.AreEqual(pointTo))
                {
                    return true;
                }

                int deltaX = Math.Clamp(pointTo.X - currentPoint.X, -1, 1);

                if (deltaX == 0)
                {
                    deltaX = 1;
                }

                int deltaY = Math.Clamp(pointTo.Y - currentPoint.Y, -1, 1);

                if (deltaY == 0)
                {
                    deltaY = 1;
                }

                if (deltaX != 0 && deltaY != 0 && !NavigationHandler.Obstacles[currentPoint.X, currentPoint.Y + deltaY] && !NavigationHandler.Obstacles[currentPoint.X + deltaX, currentPoint.Y])
                {
                    if (_FindPathInRoom(new MapPoint(currentPoint.X + deltaX, currentPoint.Y + deltaY)))
                    {
                        return true;
                    }
                }

                if (deltaY != 0 && _FindPathInRoom(new MapPoint(currentPoint.X, currentPoint.Y + deltaY)))
                {
                    return true;
                }

                if (deltaX != 0 && _FindPathInRoom(new MapPoint(currentPoint.X + deltaX, currentPoint.Y)))
                {
                    return true;
                }

                if (deltaX != 0 && deltaY != 0)
                {
                    if (!NavigationHandler.Obstacles[currentPoint.X, currentPoint.Y + deltaY] && !NavigationHandler.Obstacles[currentPoint.X - deltaX, currentPoint.Y])
                    {
                        if (_FindPathInRoom(new MapPoint(currentPoint.X - deltaX, currentPoint.Y + deltaY)))
                        {
                            return true;
                        }
                    }

                    if (!NavigationHandler.Obstacles[currentPoint.X, currentPoint.Y - deltaY] && !NavigationHandler.Obstacles[currentPoint.X + deltaX, currentPoint.Y])
                    {
                        if (_FindPathInRoom(new MapPoint(currentPoint.X + deltaX, currentPoint.Y - deltaY)))
                        {
                            return true;
                        }
                    }
                }

                if (deltaY != 0 && _FindPathInRoom(new MapPoint(currentPoint.X, currentPoint.Y - deltaY)))
                {
                    return true;
                }

                if (deltaX != 0 && _FindPathInRoom(new MapPoint(currentPoint.X - deltaX, currentPoint.Y)))
                {
                    return true;
                }

                if (deltaX != 0 && deltaY != 0 && !NavigationHandler.Obstacles[currentPoint.X, currentPoint.Y - deltaY] && !NavigationHandler.Obstacles[currentPoint.X - deltaX, currentPoint.Y])
                {
                    if (_FindPathInRoom(new MapPoint(currentPoint.X - deltaX, currentPoint.Y - deltaY)))
                    {
                        return true;
                    }
                }

                resultPoints.RemoveAt(resultPoints.Count - 1);

                return false;
            }

            _FindPathInRoom(pointFrom);

            return resultPoints.ToArray();
        }

        public MapRoom[] GetFathestRecursive()
        {
            (MapNavigationGraphElement[] path, double distance) resultPath = (Array.Empty<MapNavigationGraphElement>(), 0);

            bool _DistanceCounter(MapNavigationGraphElement currentElement, List<MapNavigationGraphElement> path, double accumulatedDistance)
            {
                if (path.Contains(currentElement))
                {
                    return false;
                }
                else
                {
                    path.Add(currentElement);
                    var currentRoomCenter = MapPointOperations.GetRoomCenter(currentElement.Room);

                    bool theNextRoomExists = false;

                    foreach (var nextElement in currentElement.RoomsTo.GroupBy(element => element.Element).Select(pair => pair.Key))
                    {
                        var distance = MapPointOperations.GetDistance(currentRoomCenter, MapPointOperations.GetRoomCenter(nextElement.Room));
                        accumulatedDistance += distance;

                        if (_DistanceCounter(nextElement, path, accumulatedDistance))
                        {
                            theNextRoomExists = true;
                        }

                        accumulatedDistance -= distance;
                    }

                    if (!theNextRoomExists)
                    {
                        if (accumulatedDistance > resultPath.distance)
                        {
                            resultPath = (path.ToArray(), accumulatedDistance);
                        }
                    }

                    path.RemoveAt(path.Count - 1);

                    return theNextRoomExists;
                }
            }

            foreach (var room in Nodes)
            {
                _DistanceCounter(room, new(), 0.0);
            }

            if (resultPath.distance > 0)
            {
                return new MapRoom[]
                {
                    resultPath.path.First().Room
                    ,
                    resultPath.path.Last().Room
                };
            }
            else
            {
                return Array.Empty<MapRoom>();
            }
        }

        public MapNavigationGraphPath? GetFathestRoomsByPathes()
        {
            MapNavigationGraphPath? resultPath = null;

            foreach (var nodeFrom in Nodes)
            {
                var pointFrom = MapPointOperations.GetRoomCenter(nodeFrom.Room);

                foreach (var nodeTo in Nodes)
                {
                    var pointTo = MapPointOperations.GetRoomCenter(nodeTo.Room);

                    var pathes = GetPathes(pointFrom, pointTo);

                    if (pathes.Length > 0)
                    {
                        var foundPath = pathes.First();

                        if (resultPath is null
                            || resultPath.Distance < foundPath.Distance)
                        {
                            resultPath = foundPath;
                        }
                    }
                }
            }

            return resultPath;
        }
    }
}
