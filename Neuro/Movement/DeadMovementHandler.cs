﻿using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Neuro.Events;
using Neuro.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Neuro.Movement;

[RegisterInIl2Cpp]
public sealed class DeadMovementHandler : MonoBehaviour
{
    private PlayerControl followPlayer = null;

    public HauntMenuMinigame minigame { get; set; }

    public static DeadMovementHandler Instance { get; private set; }

    public DeadMovementHandler(IntPtr ptr) : base(ptr)
    {
    }

    private void Awake()
    {
        if (Instance)
        {
            NeuroUtilities.WarnDoubleSingletonInstance();
            Destroy(this);
            return;
        }
        Instance = this;
    }


    public void Move() {
        Console closestConsole = null;
        float closestDistance = 999f;
        
        foreach (NormalPlayerTask task in PlayerControl.LocalPlayer.myTasks.ToArray().OfIl2CppType<NormalPlayerTask>().Where(t => !t.IsComplete))
        {

            foreach (Console console in task.FindConsoles())
            {
                if (!console) continue;
                var distance = Vector2.Distance(console.transform.position, PlayerControl.LocalPlayer.GetTruePosition());
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestConsole = console;
                }
            }
        }
        if (closestConsole) {
            moveToPosition(closestConsole.transform.position);
            return;
        }
        PlayerControl.LocalPlayer.SetRole(RoleTypes.ImpostorGhost);
        if (PlayerControl.LocalPlayer.Data.RoleType is RoleTypes.CrewmateGhost)
        {
            MovementHandler.Instance.ForcedMoveDirection = Vector2.zero;
            PlayerControl.LocalPlayer.Data.Role.UseAbility();
            return;
        }

        // follow random player
        if (followPlayer is null || followPlayer.Data.IsDead)
        {
            List<PlayerControl> alivePlayers = new();
            foreach (PlayerControl player in PlayerControl.AllPlayerControls) 
            {
                if (!player.Data.IsDead) 
                {
                    alivePlayers.Add(player);
                } 
            }
            System.Random random = new();
            int i = random.Next(alivePlayers.Count);
            followPlayer = alivePlayers[i];
            Info("Now following " + followPlayer.name);
        }
        moveToPosition(followPlayer.GetTruePosition());
    }

    private void moveToPosition(Vector2 target, float margin = 0.2f)
    {
        if (Vector2.Distance(target, PlayerControl.LocalPlayer.GetTruePosition()) < margin) MovementHandler.Instance.ForcedMoveDirection = Vector2.zero;
        else MovementHandler.Instance.ForcedMoveDirection = (target - PlayerControl.LocalPlayer.GetTruePosition()).normalized;
        
    }

    [EventHandler(EventTypes.GameStarted)]
    public static void OnGameStarted()
    {
        ShipStatus.Instance.gameObject.AddComponent<DeadMovementHandler>();
    }
}
