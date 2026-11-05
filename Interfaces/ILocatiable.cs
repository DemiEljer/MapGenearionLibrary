using MapGenearionLibrary.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerationLibrary.Interfaces
{
    public interface ILocatiable
    {
        public MapPoint StartLocation { get; }

        public MapPoint Size { get; }
    }
}
