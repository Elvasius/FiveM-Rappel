using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualRappel
{
    public class Raycaster
    {
        private static Vector3 RotationToDirection(Vector3 rotation)
        {
            var adjustedRotation = new Vector3((float)(Math.PI / 180) * rotation.X, (float)(Math.PI / 180) * rotation.Y, (float)(Math.PI / 180) * rotation.Z);
            var direction = new Vector3((float)-Math.Sin(adjustedRotation.Z) * (float)Math.Abs(Math.Cos(adjustedRotation.X)),
                (float)Math.Cos(adjustedRotation.Z) * (float)Math.Abs(Math.Cos(adjustedRotation.X)),
                (float)Math.Sin(adjustedRotation.X)
                );
            return direction;
        }
        public static int RayCastGamePlayCamera(float distance, ref bool hit, ref Vector3 endCoords, ref Vector3 normal, ref int entityHit)
        {
            var camRot = GetGameplayCamRot(2);
            var camCoord = GetGameplayCamCoord();

            var direction = RotationToDirection(camRot);
            var destination = new Vector3(camCoord.X + direction.X * distance, camCoord.Y + direction.Y * distance, camCoord.Z + direction.Z * distance);

            var ret = GetShapeTestResult(StartShapeTestRay(camCoord.X, camCoord.Y, camCoord.Z, destination.X, destination.Y, destination.Z, -1, -1, 1), ref hit, ref endCoords, ref normal, ref entityHit);

            return ret;
        }
    }
}
