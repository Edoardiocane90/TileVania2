using System;
using System.Collections;
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

    public enum XorloxFacing
    {
        Right = -1,
        Left = 1
    }

    public enum HeartState
    {
        Empty,
        Full
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

        public static bool TryGetValidPlayerRigidBodyComponent(out Rigidbody2D playerComponent)
        {
            playerComponent = null;
            var player = GameObject.FindWithTag("Player");
            if (player == null || !player.TryGetComponent<Rigidbody2D>(out playerComponent) || playerComponent == null)
                return false;

            return true;
        }

        public static T ParseEnum<T>(string value) => (T)Enum.Parse(typeof(T), value, true);
    }
}
