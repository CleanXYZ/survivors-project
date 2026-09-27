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

        public bool TryAddTrait(string traitId)
        {
            if (string.IsNullOrWhiteSpace(traitId))
            {
                return false;
            }

            foreach (string selectedTraitId in SelectedTraitIds)
            {
                if (string.Equals(selectedTraitId, traitId, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            selectedTraitIds ??= Array.Empty<string>();
            Array.Resize(ref selectedTraitIds, selectedTraitIds.Length + 1);
            selectedTraitIds[^1] = traitId;
            return true;
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

        public bool TryIncreaseLevel(int maximumLevel)
        {
            if (level >= maximumLevel)
            {
                return false;
            }

            level++;
            return true;
        }
    }

    public sealed class GrowthRuntimeState : MonoBehaviour
    {
        [Header("Growth Limits")]
        [SerializeField, Min(1)] private int maximumOwnedWeapons = 4;
        [SerializeField, Min(1)] private int maximumTraitsPerWeapon = 4;
        [SerializeField, Min(1)] private int maximumPassiveLevel = 5;

        [Header("Run Rerolls")]
        [SerializeField, Min(0)] private int initialRerollCount = 3;

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
        public int RemainingRerollCount { get; private set; }
        public int OwnedWeaponCount => ownedWeapons?.Length ?? 0;
        public IReadOnlyList<WeaponGrowthState> OwnedWeapons =>
            ownedWeapons ?? Array.Empty<WeaponGrowthState>();
        public IReadOnlyList<PassiveGrowthState> OwnedPassives =>
            ownedPassives ?? Array.Empty<PassiveGrowthState>();

        public event Action Changed;
        public event Action<int> RerollCountChanged;

        private void Awake()
        {
            RemainingRerollCount = initialRerollCount;
        }

        public void NotifyChanged()
        {
            Changed?.Invoke();
        }

        public bool TryRegister(GrowthOption option)
        {
            if (option == null)
            {
                return false;
            }

            bool changed = option.Category switch
            {
                GrowthOptionCategory.NewWeapon => TryRegisterWeapon(option.WeaponId),
                GrowthOptionCategory.WeaponTrait =>
                    TryRegisterTrait(option.WeaponId, option.TraitId),
                GrowthOptionCategory.NewPassive => TryRegisterPassive(option.PassiveId),
                GrowthOptionCategory.PassiveUpgrade => TryUpgradePassive(option.PassiveId),
                _ => false
            };

            if (changed)
            {
                Changed?.Invoke();
            }

            return changed;
        }

        public bool TryConsumeReroll()
        {
            if (RemainingRerollCount <= 0)
            {
                return false;
            }

            RemainingRerollCount--;
            RerollCountChanged?.Invoke(RemainingRerollCount);
            return true;
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

        private PassiveGrowthState FindPassive(string passiveId)
        {
            if (ownedPassives == null)
            {
                return null;
            }

            foreach (PassiveGrowthState passive in ownedPassives)
            {
                if (passive != null && string.Equals(
                    passive.PassiveId, passiveId, StringComparison.Ordinal))
                {
                    return passive;
                }
            }

            return null;
        }

        private bool TryRegisterWeapon(string weaponId)
        {
            if (string.IsNullOrWhiteSpace(weaponId)
                || IsWeaponOwned(weaponId)
                || OwnedWeaponCount >= maximumOwnedWeapons)
            {
                return false;
            }

            ownedWeapons ??= Array.Empty<WeaponGrowthState>();
            Array.Resize(ref ownedWeapons, ownedWeapons.Length + 1);
            ownedWeapons[^1] = new WeaponGrowthState(weaponId);
            return true;
        }

        private bool TryRegisterTrait(string weaponId, string traitId)
        {
            WeaponGrowthState weapon = FindWeapon(weaponId);
            if (weapon == null
                || weapon.SelectedTraitIds.Length >= maximumTraitsPerWeapon)
            {
                return false;
            }

            return weapon.TryAddTrait(traitId);
        }

        private bool TryRegisterPassive(string passiveId)
        {
            if (string.IsNullOrWhiteSpace(passiveId) || FindPassive(passiveId) != null)
            {
                return false;
            }

            ownedPassives ??= Array.Empty<PassiveGrowthState>();
            Array.Resize(ref ownedPassives, ownedPassives.Length + 1);
            ownedPassives[^1] = new PassiveGrowthState(passiveId, 1);
            return true;
        }

        private bool TryUpgradePassive(string passiveId)
        {
            PassiveGrowthState passive = FindPassive(passiveId);
            return passive != null && passive.TryIncreaseLevel(maximumPassiveLevel);
        }

        private void OnValidate()
        {
            maximumOwnedWeapons = Mathf.Max(1, maximumOwnedWeapons);
            maximumTraitsPerWeapon = Mathf.Max(1, maximumTraitsPerWeapon);
            maximumPassiveLevel = Mathf.Max(1, maximumPassiveLevel);
            initialRerollCount = Mathf.Max(0, initialRerollCount);
            Changed?.Invoke();
        }
    }
}
