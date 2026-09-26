using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivors.Growth
{
    [Serializable]
    public sealed class WeaponGrowthState
    {
        [SerializeField] private string weaponId;
        [SerializeField] private string[] selectedTraitIds = Array.Empty<string>();

        public string WeaponId => weaponId;
        public string[] SelectedTraitIds => selectedTraitIds ?? Array.Empty<string>();

        public WeaponGrowthState(string weaponId, params string[] selectedTraitIds)
        {
            this.weaponId = weaponId;
            this.selectedTraitIds = selectedTraitIds ?? Array.Empty<string>();
        }
    }

    [Serializable]
    public sealed class PassiveGrowthState
    {
        [SerializeField] private string passiveId;
        [SerializeField, Min(1)] private int level = 1;

        public string PassiveId => passiveId;
        public int Level => level;

        public PassiveGrowthState(string passiveId, int level)
        {
            this.passiveId = passiveId;
            this.level = level;
        }
    }

    public sealed class GrowthRuntimeState : MonoBehaviour
    {
        [Header("Growth Limits")]
        [SerializeField, Min(1)] private int maximumOwnedWeapons = 4;
        [SerializeField, Min(1)] private int maximumTraitsPerWeapon = 4;
        [SerializeField, Min(1)] private int maximumPassiveLevel = 5;

        [Header("Test / Starting State")]
        [SerializeField] private WeaponGrowthState[] ownedWeapons =
        {
            new("bow")
        };
        [SerializeField] private PassiveGrowthState[] ownedPassives =
            Array.Empty<PassiveGrowthState>();

        public int MaximumOwnedWeapons => maximumOwnedWeapons;
        public int MaximumTraitsPerWeapon => maximumTraitsPerWeapon;
        public int MaximumPassiveLevel => maximumPassiveLevel;
        public int OwnedWeaponCount => ownedWeapons?.Length ?? 0;
        public IReadOnlyList<WeaponGrowthState> OwnedWeapons =>
            ownedWeapons ?? Array.Empty<WeaponGrowthState>();
        public IReadOnlyList<PassiveGrowthState> OwnedPassives =>
            ownedPassives ?? Array.Empty<PassiveGrowthState>();

        public event Action Changed;

        public void NotifyChanged()
        {
            Changed?.Invoke();
        }

        public bool IsWeaponOwned(string weaponId)
        {
            if (ownedWeapons == null)
            {
                return false;
            }

            foreach (WeaponGrowthState weapon in ownedWeapons)
            {
                if (weapon != null && string.Equals(
                    weapon.WeaponId, weaponId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        public int GetSelectedTraitCount(string weaponId)
        {
            WeaponGrowthState weapon = FindWeapon(weaponId);
            return weapon?.SelectedTraitIds.Length ?? 0;
        }

        public bool IsTraitSelected(string weaponId, string traitId)
        {
            WeaponGrowthState weapon = FindWeapon(weaponId);
            if (weapon == null)
            {
                return false;
            }

            foreach (string selectedTraitId in weapon.SelectedTraitIds)
            {
                if (string.Equals(selectedTraitId, traitId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        public int GetPassiveLevel(string passiveId)
        {
            if (ownedPassives == null)
            {
                return 0;
            }

            foreach (PassiveGrowthState passive in ownedPassives)
            {
                if (passive != null && string.Equals(
                    passive.PassiveId, passiveId, StringComparison.Ordinal))
                {
                    return Mathf.Max(0, passive.Level);
                }
            }

            return 0;
        }

        private WeaponGrowthState FindWeapon(string weaponId)
        {
            if (ownedWeapons == null)
            {
                return null;
            }

            foreach (WeaponGrowthState weapon in ownedWeapons)
            {
                if (weapon != null && string.Equals(
                    weapon.WeaponId, weaponId, StringComparison.Ordinal))
                {
                    return weapon;
                }
            }

            return null;
        }

        private void OnValidate()
        {
            maximumOwnedWeapons = Mathf.Max(1, maximumOwnedWeapons);
            maximumTraitsPerWeapon = Mathf.Max(1, maximumTraitsPerWeapon);
            maximumPassiveLevel = Mathf.Max(1, maximumPassiveLevel);
            Changed?.Invoke();
        }
    }
}
