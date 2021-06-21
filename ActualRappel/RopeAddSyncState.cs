using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualRappel
{
    public class RopeAddSyncState
    {
        public readonly string RopeId;
        public readonly float Length;
        public readonly int NetPlayerHandle;
        public readonly int NetEntityHandle;
        public RopeAddSyncState(string _ropeId, float _length, int _netPlayerHandle, int _netEntityHandle)
        {
            RopeId = _ropeId;
            Length = _length;
            NetPlayerHandle = _netPlayerHandle;
            NetEntityHandle = _netEntityHandle;
        }
    }
}
