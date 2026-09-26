using System;

namespace Survivors.Growth
{
    public enum GrowthOptionCategory
    {
        NewWeapon,
        WeaponTrait,
        NewPassive,
        PassiveUpgrade
    }

    [Serializable]
    public sealed class GrowthOption
    {
        public GrowthOptionCategory Category { get; }
        public string Id { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public string WeaponId { get; }
        public string TraitId { get; }
        public string PassiveId { get; }
        public int CurrentPassiveLevel { get; }
        public int TargetPassiveLevel { get; }

        private GrowthOption(
            GrowthOptionCategory category,
            string id,
            string displayName,
            string description,
            string weaponId = null,
            string traitId = null,
            string passiveId = null,
            int currentPassiveLevel = 0,
            int targetPassiveLevel = 0)
        {
            Category = category;
            Id = id;
            DisplayName = displayName;
            Description = description;
            WeaponId = weaponId;
            TraitId = traitId;
            PassiveId = passiveId;
            CurrentPassiveLevel = currentPassiveLevel;
            TargetPassiveLevel = targetPassiveLevel;
        }

        public static GrowthOption ForNewWeapon(WeaponDefinition weapon)
        {
            return new GrowthOption(
                GrowthOptionCategory.NewWeapon,
                $"weapon:{weapon.Id}",
                weapon.DisplayName,
                weapon.Description,
                weaponId: weapon.Id);
        }

        public static GrowthOption ForWeaponTrait(
            WeaponDefinition weapon,
            WeaponTraitDefinition trait)
        {
            return new GrowthOption(
                GrowthOptionCategory.WeaponTrait,
                $"weapon:{weapon.Id}:trait:{trait.Id}",
                $"{weapon.DisplayName} - {trait.DisplayName}",
                trait.Description,
                weapon.Id,
                trait.Id);
        }

        public static GrowthOption ForNewPassive(PassiveDefinition passive)
        {
            return new GrowthOption(
                GrowthOptionCategory.NewPassive,
                $"passive:{passive.Id}:level:1",
                passive.DisplayName,
                passive.Description,
                passiveId: passive.Id,
                targetPassiveLevel: 1);
        }

        public static GrowthOption ForPassiveUpgrade(
            PassiveDefinition passive,
            int currentLevel)
        {
            int targetLevel = currentLevel + 1;
            return new GrowthOption(
                GrowthOptionCategory.PassiveUpgrade,
                $"passive:{passive.Id}:level:{targetLevel}",
                $"{passive.DisplayName} {targetLevel}단계",
                $"{passive.Description}\n현재 {currentLevel}단계 → {targetLevel}단계",
                passiveId: passive.Id,
                currentPassiveLevel: currentLevel,
                targetPassiveLevel: targetLevel);
        }
    }
}
