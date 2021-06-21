using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualRappel
{
    class Main : BaseScript
    {
        private RappelManager rappelManager;
        private int previousTimer = 0;
        private bool pressedWind;
        private bool pressedUnwind;
        private bool pressedShootrappel;
        private bool pressedGunmode;
        private bool pressedDetach;

        public Main()
        {
            rappelManager = new RappelManager();
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
            EventHandlers["ropeAdd"] += new Action<string, float, int, int>(OnOtherClientCreatedRappel);
            EventHandlers["ropeSync"] += new Action<string, float, bool>(OnRopeSync);
            EventHandlers["ropeCut"] += new Action<string>(OnRopeDetached);
        }
        private void OnClientResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;
            RegisterCommand("+rappelUnwind", new Action<int, List<object>, string>((source, args, raw) =>
            {
                pressedUnwind = true;
            }), false);
            RegisterCommand("-rappelUnwind", new Action<int, List<object>, string>((source, args, raw) =>
            {
                pressedUnwind = false;
            }), false);
            RegisterCommand("+rappelWind", new Action<int, List<object>, string>((source, args, raw) =>
            {
                pressedWind = true;
            }), false);
            RegisterCommand("-rappelWind", new Action<int, List<object>, string>((source, args, raw) =>
            {
                pressedWind = false;
            }), false);

            RegisterCommand("+rappelShoot", new Action<int, List<object>, string>((source, args, raw) =>
            {
                pressedShootrappel = true;
            }), false);
            RegisterCommand("-rappelShoot", new Action<int, List<object>, string>((source, args, raw) =>
            {
                pressedShootrappel = false;
            }), false);
            RegisterCommand("+rappelGunmode", new Action<int, List<object>, string>((source, args, raw) =>
            {
                pressedGunmode = true;
            }), false);
            RegisterCommand("-rappelGunmode", new Action<int, List<object>, string>((source, args, raw) =>
            {
                pressedGunmode = false;
            }), false);
            RegisterCommand("+rappelDetach", new Action<int, List<object>, string>((source, args, raw) =>
            {
                pressedDetach = true;
            }), false);
            RegisterCommand("-rappelDetach", new Action<int, List<object>, string>((source, args, raw) =>
            {
                pressedDetach = false;
            }), false);
            RegisterKeyMapping("+rappelUnwind", "Unwind Rappel Rope", "keyboard", "b");
            RegisterKeyMapping("+rappelWind", "Wind Rappel Rope", "keyboard", "lcontrol");
            RegisterKeyMapping("+rappelShoot", "Shoot Rappel Rope", "keyboard", "tab");
            RegisterKeyMapping("+rappelGunmode", "Enter Rappel Gunmode", "keyboard", "lmenu");
            RegisterKeyMapping("+rappelDetach", "Detach Rappel Rope", "keyboard", "e");
            Tick += OnTick;
        }
        private async Task OnTick()
        {
            HandleInput();
        }
        private void HandleInput()
        {

            if (rappelManager.PlayerState.IsUsingRappel)
            {
                // Don't double send a sync message, 
                bool checkForSync = true;
                // Makes winding / unwinding stop if winding / unwiding weren't pressed in the previous 10 frames
                if (rappelManager.HandleWindingStop())
                {
                    var syncState = rappelManager.GetRappelInternalLength();
                    if (syncState != null)
                    {
                        checkForSync = false;
                        TriggerServerEvent("ropeSync", syncState.RopeId, syncState.Length, true);
                    }
                }
                var currentTime = GetGameTimer();
                if (currentTime - previousTimer >= 100 && checkForSync)
                {
                    previousTimer = currentTime;
                    var syncState = rappelManager.GetRappelInternalLength();
                    if (syncState != null)
                    {
                        TriggerServerEvent("ropeSync", syncState.RopeId, syncState.Length, rappelManager.RappelState.IsMovementStopped);
                    }
                }
                var playerPed = Game.PlayerPed.Handle;
                if (HasEntityCollidedWithAnything(playerPed))
                {
                    var collisionNormal = GetCollisionNormalOfLastHitForEntity(playerPed);
                    // Maybe && collisionNormal.Z < 1.0f
                    if (collisionNormal.Z > 0.8f)
                    {
                        rappelManager.DetachFromCurrentRappel();
                        return;
                    }

                }

                if (!IsPedInParachuteFreeFall(playerPed))
                {
                    rappelManager.EnableRappelFreeMovement();
                }
                else
                {
                    rappelManager.StartRappel();
                }




                if (pressedUnwind)
                {
                    rappelManager.StartUnwindingOfRappelRope();
                }
                else if (pressedWind)
                {
                    rappelManager.StartWindingOfRappelRope();
                }
                else if (pressedDetach)
                {
                    var ropeId = rappelManager.DetachFromCurrentRappel();
                    TriggerServerEvent("ropeCut", ropeId);
                }
                else if (pressedGunmode)
                {
                    rappelManager.HandleGunModeInput();
                }
            }
            else
            {
                if (pressedShootrappel)
                {
                    var syncState = rappelManager.ShootRappel();
                    previousTimer = GetGameTimer();

                    TriggerServerEvent("ropeAdd", syncState.RopeId, syncState.Length, syncState.NetPlayerHandle, syncState.NetEntityHandle);
                }
            }
            rappelManager.HandleOtherRopes();
            ResetSinglePressedButtons();
        }
        private void OnOtherClientCreatedRappel(string ropeId, float length, int entity1, int entity2)
        {
            var playerHandle = NetworkGetEntityFromNetworkId(entity1);
            var entityHandle = NetworkGetEntityFromNetworkId(entity2);
            rappelManager.CreateRappelForOtherClient(ropeId, playerHandle, entityHandle, length);

        }
        private void OnRopeSync(string ropeId, float length, bool isShortUpdate)
        {
            rappelManager.SyncRopeLength(ropeId, length, isShortUpdate);
        }
        private void OnRopeDetached(string ropeId)
        {
            rappelManager.SyncDetachState(ropeId);
        }
        private void ResetSinglePressedButtons()
        {
            pressedDetach = false;
            pressedShootrappel = false;
            pressedGunmode = false;
        }

    }
}
