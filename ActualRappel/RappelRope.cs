using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualRappel
{
    public class RappelRope : Rope
    {
        public int EntityHandleBottom { get { return entityHandleBottom; } }
        private int entityHandleBottom;
        private int entityHandleTop;
        public RappelRope(float _length, Vector3 _startPos, String _id = null, float _maxLength = 300, float _minLength = 1, float _startWindingSpeed = 5, int _ropeType = 4) : base(_length, _startPos, _id, _maxLength, _minLength, _startWindingSpeed, _ropeType)
        {
        }
        public void AttachEntities(int _entityHandleBottom, int _entityHandleTop, Vector3 _bottomCoords, Vector3 _topCoords)
        {
            if(!IsEntityAPed(_entityHandleBottom)) {
                Debug.WriteLine("Bottom Entity should be a ped");
                return;
            }
            if(IsEntityAPed(_entityHandleTop))
            {
                // In realitity it should always be the predefined object but this allows for later extension of the code
                Debug.WriteLine("Top Entity should not be a ped: i.e. it should be an object or vehicle");
                return;
            }
            AttachEntitiesToRope(internalId, _entityHandleBottom, _entityHandleTop, 
                _bottomCoords.X, _bottomCoords.Y, _bottomCoords.Z, _topCoords.X, _topCoords.Y, _topCoords.Z,
                length, true, true, "0", "5");

            SetPedCanRagdoll(_entityHandleBottom, false);
            FreezeEntityPosition(_entityHandleTop, true);
            entityHandleBottom = _entityHandleBottom;
            entityHandleTop = _entityHandleTop;

        }
        public void DetachEntity(int _entityHandle)
        {
            if (_entityHandle != entityHandleTop && _entityHandle != entityHandleBottom)
            {
                Debug.WriteLine("Detached entity should be top or bottom entity");
                return;
            }
            DetachRopeFromEntity(internalId, _entityHandle);
            SetPedCanRagdoll(entityHandleBottom, true);
        }
       

    }
}
