using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivors.Growth
{
    public static class GrowthCandidateGenerator
    {
        public static List<GrowthOption> BuildEligiblePool(
            GrowthContentLibrary content,
            GrowthRuntimeState state)
        {
            List<GrowthOption> pool = new();
            HashSet<string> uniqueIds = new(StringComparer.Ordinal);

            foreach (WeaponDefinition weapon in content.Weapons)
            {
                if (!IsValidDefinition(weapon?.Id, weapon?.DisplayName))
                {
                    continue;
                }

                if (!state.IsWeaponOwned(weapon.Id))
                {
                    if (state.OwnedWeaponCount < state.MaximumOwnedWeapons)
                    {
                        AddUnique(pool, uniqueIds, GrowthOption.ForNewWeapon(weapon));
                    }

                    continue;
                }

                if (state.GetSelectedTraitCount(weapon.Id) >= state.MaximumTraitsPerWeapon)
                {
                    continue;
                }

                foreach (WeaponTraitDefinition trait in weapon.Traits)
                {
                    if (!IsValidDefinition(trait?.Id, trait?.DisplayName)
                        || state.IsTraitSelected(weapon.Id, trait.Id))
                    {
                        continue;
                    }

                    AddUnique(
                        pool,
                        uniqueIds,
                        GrowthOption.ForWeaponTrait(weapon, trait));
                }
            }

            foreach (PassiveDefinition passive in content.Passives)
            {
                if (!IsValidDefinition(passive?.Id, passive?.DisplayName))
                {
                    continue;
                }

                int currentLevel = state.GetPassiveLevel(passive.Id);
                if (currentLevel <= 0)
                {
                    AddUnique(pool, uniqueIds, GrowthOption.ForNewPassive(passive));
                }
                else if (currentLevel < state.MaximumPassiveLevel)
                {
                    AddUnique(
                        pool,
                        uniqueIds,
                        GrowthOption.ForPassiveUpgrade(passive, currentLevel));
                }
            }

            return pool;
        }

        public static List<GrowthOption> DrawWithoutReplacement(
            IReadOnlyList<GrowthOption> pool,
            int maximumCount)
        {
            List<GrowthOption> shuffled = new(pool.Count);
            for (int i = 0; i < pool.Count; i++)
            {
                shuffled.Add(pool[i]);
            }

            int resultCount = Mathf.Min(Mathf.Max(0, maximumCount), shuffled.Count);
            for (int i = 0; i < resultCount; i++)
            {
                int swapIndex = UnityEngine.Random.Range(i, shuffled.Count);
                (shuffled[i], shuffled[swapIndex]) = (shuffled[swapIndex], shuffled[i]);
            }

            if (shuffled.Count > resultCount)
            {
                shuffled.RemoveRange(resultCount, shuffled.Count - resultCount);
            }

            return shuffled;
        }

        private static bool IsValidDefinition(string id, string displayName)
        {
            return !string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(displayName);
        }

        private static void AddUnique(
            ICollection<GrowthOption> pool,
            ISet<string> uniqueIds,
            GrowthOption option)
        {
            if (option != null && uniqueIds.Add(option.Id))
            {
                pool.Add(option);
            }
        }
    }
}
