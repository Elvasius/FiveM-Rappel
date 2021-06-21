using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualRappel
{
    public class Rope
    {
        public readonly String Id;
        public float Length {
            get { return length; }
            set {
                length = value;
                StartRopeWinding(internalId);
                RopeForceLength(internalId, length);
            
            }
        }
        // Internal length is used to get the length after winding / unwinding
        public float InternalLength
        {
            get
            {
                var internalLength = GetRopeLength(internalId);
                length = internalLength;
                return internalLength;
            }
        }
        public float OldLength;
        public readonly Vector3 StartPos;

        protected String id;
        protected float length;
        protected int internalId;
        public Rope(float _length, Vector3 _startPos, String _id = null, float _maxLength = 300, float _minLength = 1, float _startWindingSpeed = 5, int _ropeType = 4)
        {
            if(_id == null)
            {
                Id = Guid.NewGuid().ToString();
            } else
            {
                Id = _id;
            }
            
            Length = _length;
            StartPos = _startPos;

            int unk = 0;
            internalId = AddRope(StartPos.X, StartPos.Y, StartPos.Z, 0f, 0f, 0f, 
                _maxLength, _ropeType, _maxLength, _minLength, _startWindingSpeed, 
                false, false, true, 5f, false, ref unk);
            StartRopeWinding(internalId);
            RopeForceLength(internalId, _length);
            RopeLoadTextures();
        }
        public void StopAllWindingActions()
        {
            StopWiding();
            StopRopeUnwindingFront(internalId);
        }
        public void StartUnwiding()
        {
            StopWiding();
            StartRopeUnwindingFront(internalId);
        }

        public void StartWinding()
        {
            StartRopeWinding(internalId);
        }
        public void StopWiding()
        {
            StopRopeWinding(internalId);
        }
    }
}
