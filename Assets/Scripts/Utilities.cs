﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public enum PlayerStates
    {
        Idle,
        Running,
        Attacking,
        Jumping,
        Climbing,
        TakingDamage,
        Dying,
        Dead
    }

    public enum EnemyStates
    {
        Idle,
        Moving,
        LongRangeAttack,
        ShortRangeAttack,
        TakingDamage,
        Dying,
        Dead
    }

    public static class Utilities
    {
        public static bool TryGetValidPlayerMovement(out PlayerMovement playerMovement)
        {
            playerMovement = null;
            var player = GameObject.FindWithTag("Player");
            if (player == null || !player.TryGetComponent<PlayerMovement>(out playerMovement) || playerMovement == null)
                return false;

            return true;
        }
        public static bool TryGetValidPlayerComponent(out Rigidbody2D playerComponent)
        {
            playerComponent = null;
            var player = GameObject.FindWithTag("Player");
            if (player == null || !player.TryGetComponent<Rigidbody2D>(out playerComponent) || playerComponent == null)
                return false;

            return true;
        }
    }
}