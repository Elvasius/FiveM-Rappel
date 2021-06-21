using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualRappel 
{   
    public struct PlayerState 
    {
        public bool IsAiming { get { return isAiming; } }
        public bool IsPlaying { get { return isPlaying; } }
        public bool IsUsingRappel { get { return isUsingRappel; } }
        public bool HasStartedRappel { get { return hasStartedRappel;  } }
        private bool isAiming;
        private bool isPlaying;
        private bool isUsingRappel;
        private bool hasStartedRappel { get; set; }
        public bool HasParachuteBeforeRappel { get; set; }

        // State Transitions
        public void ShootRappel()
        {
            isAiming = false;
            isPlaying = false;
            isUsingRappel = true;
            hasStartedRappel = false;
        }
        public void DetachRappel()
        {
            isAiming = false;
            isPlaying = false;
            isUsingRappel = false;
            hasStartedRappel = false;
        }
        public void StartRappel()
        {
            hasStartedRappel = true;
            isPlaying = true;
        }
        public void StartGunMode()
        {
            isAiming = true;
        }
        public void ExitGunMode()
        {
            isAiming = false;
        }
    }
    public struct RappelState
    {
        public bool IsMovementStopped;
    }
    public class RappelManager 
    {
        private Dictionary<String, RappelRope> otherRopesDict;
        private List<RopeUpdateState> otherRopesThatNeedClientsideUpdate;
        private RappelRope currentRappelRope;
        private const float MAX_ROPE_LENGTH = 300;
        
      
        private class RopeUpdateState
        {
            public string RopeId;
            public int FrameCounter = 0;

            public RopeUpdateState(string _ropeId)
            {
                RopeId = _ropeId;
            }
        }
        public PlayerState PlayerState { get { return playerState; } }
        private PlayerState playerState;
        public RappelState RappelState;
        private bool stop;
        private int rappelRopeCounter;
        public RappelManager()
        {
            otherRopesDict = new Dictionary<string, RappelRope>();
            otherRopesThatNeedClientsideUpdate = new List<RopeUpdateState>();
            // Load animations and model in memory
            RequestModel((uint)GetHashKey("prop_fncsec_02pole"));
            RequestAnimDict("missrappel");
            RequestAnimDict("combat@chg_stance");
        }

        public RopeAddSyncState ShootRappel()
        {
            // State transitions
            playerState.ShootRappel();

            RappelState.IsMovementStopped = true;

            // Raycast to location
            bool didHit = false;
            Vector3 endCoords = new Vector3();
            Vector3 normal = new Vector3();

            int entityHitHandle = 0;
            Raycaster.RayCastGamePlayCamera(1000, ref didHit, ref endCoords, ref normal, ref entityHitHandle);
            
            var ped = Game.PlayerPed;
            var coords = ped.Position;
            int entityHandle = 0;
            entityHandle = CreateObject(GetHashKey("prop_fncsec_02pole"), endCoords.X, endCoords.Y, endCoords.Z + 2, true, true, false);

            SetEntityRotation(
                entityHandle,
                0.0f,
                 180.0f,
                 0.0f,
                0,
                 true
           );
            var dist = Vdist(coords.X, coords.Y, coords.Z, endCoords.X, endCoords.Y, endCoords.Z + 2);
            if (dist > MAX_ROPE_LENGTH)
            {
                return null;
            }
            

            RappelRope rope = new RappelRope(dist, coords);
            currentRappelRope = rope;
            stop = true;
            rope.AttachEntities(ped.Handle, entityHandle, coords, endCoords+ new Vector3(0, 0, 2));

            // Vehicle attachment happens separately from rope attachment so we can just do it here
            if (entityHitHandle > 0 && IsEntityAVehicle(entityHitHandle))
            {
                AttachEntityToEntity(entityHandle, entityHitHandle, 0, 0f, 0f, 0f, 0f, 0f, 0f, true, false, true, false, 0, false);
            }
            playerState.HasParachuteBeforeRappel = HasPedGotWeapon(ped.Handle, (uint)GetHashKey("gadget_parachute"), false);
            if(!playerState.HasParachuteBeforeRappel)
            {
                GiveWeaponToPed(ped.Handle, (uint)GetHashKey("gadget_parachute"), 1, false, false);
            }
            

            return new RopeAddSyncState(currentRappelRope.Id, dist, NetworkGetNetworkIdFromEntity(Game.PlayerPed.Handle), NetworkGetNetworkIdFromEntity(entityHandle));
        }
        public string DetachFromCurrentRappel()
        {
            // State transitions
            playerState.DetachRappel();
            RappelState.IsMovementStopped = true;
            string ropeId = currentRappelRope.Id;
            currentRappelRope.DetachEntity(Game.PlayerPed.Handle);
            currentRappelRope = null;
            // Keep rope in world or delete?
            //----------

            // Smoother animation transition
            TaskSkyDive(Game.PlayerPed.Handle);
            ClearPedTasksImmediately(Game.PlayerPed.Handle);
            if(!playerState.HasParachuteBeforeRappel)
            {
                RemoveWeaponFromPed(Game.PlayerPed.Handle, (uint)GetHashKey("gadget_parachute"));
            }
            return ropeId;
        }
        public RopeLengthSyncState GetRappelInternalLength()
        {
            var currentLength = currentRappelRope.InternalLength;

            if (Math.Abs(currentLength - currentRappelRope.OldLength) > 0.1f)
            {            
                // Only update length after big change
                currentRappelRope.OldLength = currentLength;
                return new RopeLengthSyncState(currentRappelRope.Id, currentLength);
            }
            return null;
        }
        public void EnableRappelFreeMovement()
        {
            if (!(playerState.IsAiming) && (playerState.HasStartedRappel || GetEntityHeightAboveGround(Game.PlayerPed.Handle) > 3f))
            {
  
                TaskSkyDive(Game.PlayerPed.Handle);
                StartRappel();
               
            }
        }
        // Start or restart
        public void StartRappel()
        {
            if (!playerState.IsPlaying)
            {
                TaskPlayAnim(Game.PlayerPed.Handle, "missrappel", "rappel_idle", 8, 0, -1, 33, 0, false, false, false);
            }
            playerState.StartRappel();
        }
        private void ResetWindingInputCheck()
        {
            stop = true;
            rappelRopeCounter = 0;
        }
        public bool HandleWindingStop()
        {
            if (stop && rappelRopeCounter++ >= 10)
            {
                currentRappelRope.StopAllWindingActions();
                ResetWindingInputCheck();
                RappelState.IsMovementStopped = true;
                return true;
            }
            return false;
        }
        public void StartUnwindingOfRappelRope()
        {
            currentRappelRope.StartUnwiding();
            RappelState.IsMovementStopped = false;
            ResetWindingInputCheck();
        }
        public void StartWindingOfRappelRope()
        {
            currentRappelRope.StartWinding();
            RappelState.IsMovementStopped = false;
            ResetWindingInputCheck();
        }
        public void HandleGunModeInput()
        {
            if (!playerState.IsAiming)
            {
                EnableGunMode();
            }
            else
            {
                ExitGunMode();
            }
        }
        private void EnableGunMode()
        {
            ClearPedTasksImmediately(Game.PlayerPed.Handle);
            TaskPlayAnim(Game.PlayerPed.Handle, "combat@chg_stance", "crouch", 8, 0, -1, 33, 0, false, false, false);
            playerState.StartGunMode();
        }
        private void ExitGunMode()
        {
            TaskPlayAnim(GetPlayerPed(-1), "missrappel", "rappel_idle", 8, 0, -1, 33, 0, false, false, false);
            TaskSkyDive(Game.PlayerPed.Handle);
            playerState.ExitGunMode();
        }
        public void HandleOtherRopes()
        {
          
            foreach (var ropeState in otherRopesThatNeedClientsideUpdate)
            {
                RappelRope otherRope;
                if (ropeState.FrameCounter++ >= 10 && otherRopesDict.TryGetValue(ropeState.RopeId, out otherRope))
                {
                    otherRope.StopWiding();
                   
                    ropeState.FrameCounter = 0;
                   // otherRopesThatNeedClientsideUpdate.Remove(ropeState);
                }
            }
        }
        public void CreateRappelForOtherClient(string ropeId, int playerHandle, int entityHandle, float length)
        {
            SetPedCanRagdoll(playerHandle, false);
            var coords = GetEntityCoords(playerHandle, true);
            var eCoords = GetEntityCoords(entityHandle, true);

            RappelRope rope = new RappelRope(length, coords, ropeId);
            otherRopesDict[ropeId] = rope;
            otherRopesThatNeedClientsideUpdate.Add(new RopeUpdateState(ropeId));
            rope.AttachEntities(playerHandle, entityHandle, coords, eCoords);    
        }
        public void SyncRopeLength(string ropeId, float length, bool isShortUpdate)
        {
            RappelRope rope;
            Debug.WriteLine(ropeId);
            Debug.WriteLine(length.ToString());
            Debug.WriteLine(isShortUpdate.ToString());
            if (otherRopesDict.TryGetValue(ropeId, out rope))
            {
                rope.Length = length;
                otherRopesThatNeedClientsideUpdate.First(e => e.RopeId == ropeId).FrameCounter = isShortUpdate ? 9 : 0;
                //otherRopesThatNeedClientsideUpdate.Add(new RopeUpdateState(ropeId));
            }
        }
        public void SyncDetachState(string ropeId)
        {
            RappelRope rope;
            if (otherRopesDict.TryGetValue(ropeId, out rope))
            {
                rope.DetachEntity(rope.EntityHandleBottom);
                otherRopesThatNeedClientsideUpdate.RemoveAll(s => s.RopeId == ropeId);
            }
        }
    }
}
