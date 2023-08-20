using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public static bool TryGetLootTable(out LootTable lootTable)
        {
            lootTable = null;
            var lootTableGamobject = GameObject.FindWithTag("Loot Table");
            if (lootTableGamobject == null || !lootTableGamobject.TryGetComponent<LootTable>(out lootTable) || lootTable == null)
                return false;

            return true;
        }

        public static GameObject GetLootReward(int rewardProbability = 100)
        {
            if (!TryGetLootTable(out var lootTable))
                return null;

            var orderedList = lootTable.LootItems.OrderBy(li => li.SpawnRate).Reverse().ToList();
            var totalWeight = lootTable.LootItems.Sum(li => li.SpawnRate);
            if (rewardProbability <= 100 && rewardProbability >= 0)
                totalWeight = totalWeight * 100 / rewardProbability;

            var random = UnityEngine.Random.Range(0f, totalWeight);
            var relativeWeight = 0f;
            foreach (var lootItem in orderedList)
            {
                relativeWeight += lootItem.SpawnRate;
                if (random <= relativeWeight)
                    return lootItem.Item;
            }

            return null;
        }

        public static T ParseEnum<T>(string value) => (T)Enum.Parse(typeof(T), value, true);
    }
}
