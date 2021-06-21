using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualRappel
{
    public class RopeLengthSyncState
    {
        public readonly string RopeId;
        public readonly float Length;
        public RopeLengthSyncState(string _ropeId, float _length)
        {
            RopeId = _ropeId;
            Length = _length;
        }
    }
}
