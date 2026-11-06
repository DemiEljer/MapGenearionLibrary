using MapGenearionLibrary.Base;
using MapGenearionLibrary.Enums;
using MapGenerationLibrary.Enums;
using MapGenerationLibrary.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerationLibrary.Base
{
    public class MapLocationTree<TValue> : IEnumerable<TValue>
        where TValue : class
    {
        public class LocationTreeElement : ILocatiable
        {
            internal LocationTreeElement[] Leaves { get; } = new LocationTreeElement[6];

            internal LocationTreeElement Parent { get; set; } = null;

            internal int Index { get; }

            public MapPoint StartLocation { get; } = new();

            public MapPoint Size { get; } = new();

            public TValue Value { get; internal set; } = default(TValue);

            public SeperationParameterEnum Parameter { get; }

            public LocationTreeElement(int index, MapPoint point, MapPoint size, SeperationParameterEnum parameter, TValue value)
            {
                Index = index;
                point.CopyTo(StartLocation);
                size.CopyTo(Size);
                Parameter = parameter;
                Value = value;
            }
        }

        public LocationTreeElement Head { get; private set; } = null;

        public TValue[] Values { get; private set; } = Array.Empty<TValue>();

        public TValue this[int index]
        {
            get => Values[index];
        }

        public int Count => Values.Length;

        internal MapLocationTree() { }


        public TValue GetValue(MapPoint point)
        {
            LinkedList<LocationTreeElement> searchingStack = new();

            searchingStack.AddLast(Head);

            bool _HandleElement(LocationTreeElement currentElement)
            {
                if (currentElement.DoesLocatibleContainsPoint(point))
                {
                    return true;
                }
                else
                {
                    foreach (var leaveIndex in currentElement.GetLocatibleLeaveIndexCollection(point, currentElement.Parameter))
                    {
                        if (currentElement.Leaves[leaveIndex] is not null)
                        {
                            searchingStack.AddLast(currentElement.Leaves[leaveIndex]);
                        }
                    }

                    return false;
                }
            }

            while (searchingStack.Count > 0)
            {
                int currentWaveElementsCount = searchingStack.Count;

                for (int i = 0; i < currentWaveElementsCount; i++)
                {
                    if (_HandleElement(searchingStack.First.Value))
                    {
                        return searchingStack.First.Value.Value;
                    }

                    searchingStack.RemoveFirst();
                }
            }

            return null;
        }

        public static MapLocationTree<TValue> CreateTree(MapSeperationAreaIterator iterator, Func<MapSeperationArea, int, TValue> valueCreatingDelegate)
        {
            if (iterator is not null)
            {
                MapLocationTree<TValue> newTree = new();
                LinkedList<TValue> createdObjects = new();

                (LocationTreeElement Parent, int LeaveIndex, SeperationParameterEnum Parameter) _FindElementLocation(MapSeperationArea area)
                {
                    if (newTree.Head is null)
                    {
                        return (null, 0, SeperationParameterEnum.None);
                    }
                    else
                    {
                        var currentElement = newTree.Head;
                        var parameter = newTree.Head.Parameter;
                        var leaveIndex = 0;

                        while (currentElement is not null)
                        {
                            leaveIndex = currentElement.GetLocatibleLeaveIndex(area, currentElement.Parameter);

                            if (currentElement.Leaves[leaveIndex] is not null)
                            {
                                currentElement = currentElement.Leaves[leaveIndex];
                            }
                            else
                            {
                                break;
                            }
                        }

                        var newParameter = (currentElement.Parameter == SeperationParameterEnum.X ? SeperationParameterEnum.Y : SeperationParameterEnum.X);

                        return (currentElement, leaveIndex, newParameter);
                    }
                }

                void _AppendArea(MapSeperationArea area)
                {
                    var areaParent = _FindElementLocation(area);
                    TValue valueObject = default(TValue);

                    valueObject = valueCreatingDelegate?.Invoke(area, createdObjects.Count);
                    createdObjects.AddLast(valueObject);

                    if (areaParent.Parent is null)
                    {
                        newTree.Head = new LocationTreeElement(createdObjects.Count - 1, area.StartLocation, area.Size, SeperationParameterEnum.X, valueObject);
                    }
                    else
                    {
                        areaParent.Parent.Leaves[areaParent.LeaveIndex] = new LocationTreeElement(createdObjects.Count - 1, area.StartLocation, area.Size, areaParent.Parameter, valueObject);
                        areaParent.Parent.Leaves[areaParent.LeaveIndex].Parent = areaParent.Parent;
                    }
                }

                foreach (var roomArea in iterator.RoomsAreas)
                {
                    _AppendArea(roomArea);
                }

                newTree.Values = createdObjects.ToArray();

                return newTree;
            }
            else
            {
                return null;
            }
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            foreach (var value in Values)
            {
                yield return value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            List<StringBuilder> lines = new();

            void _ValueAdder(int lineIndex, int parentIndex, int leaveIndex, LocationTreeElement element)
            {
                var value = $"'{element.Index}'[{lineIndex}, {parentIndex}, {leaveIndex}, {element.Parameter}]{element.GetString()}\t";

                if (lineIndex == lines.Count)
                {
                    lines.Add(new());
                }

                lines[lineIndex].Append(value);

                for (int _leaveIndex = 0; _leaveIndex < element.Leaves.Length; _leaveIndex++)
                {
                    if (element.Leaves[_leaveIndex] is not null)
                    {
                        _ValueAdder(lineIndex + 1, element.Index, _leaveIndex, element.Leaves[_leaveIndex]);
                    }
                }
            }

            if (Head is not null)
            {
                _ValueAdder(0, -1, -1, Head);
            }

            foreach (var line in lines)
            {
                sb.AppendLine(line.ToString());
            }

            return sb.ToString();
        }
    }
}
