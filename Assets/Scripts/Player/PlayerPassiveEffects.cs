using System;
using Survivors.Growth;
using UnityEngine;

namespace Survivors.Player
{
    [RequireComponent(typeof(GrowthContentLibrary), typeof(GrowthRuntimeState))]
    public sealed class PlayerPassiveEffects : MonoBehaviour
    {
        private const float MinimumWeaponCooldown = 0.01f;
        private const float MaximumCombinedCooldownReduction = 0.99f;

        private GrowthContentLibrary contentLibrary;
        private GrowthRuntimeState growthState;

        public event Action ModifiersChanged;

        public float MaxHealthMultiplier => GetMultiplier(PassiveIds.MaxHealth);
        public float MoveSpeedMultiplier => GetMultiplier(PassiveIds.MoveSpeed);
        public float PickupRangeMultiplier => GetMultiplier(PassiveIds.PickupRange);
        public float HealthRegenerationMultiplier => GetMultiplier(PassiveIds.HealthRegeneration);
        public float ExperienceGainMultiplier => GetMultiplier(PassiveIds.ExperienceGain);
        public float InvulnerabilityDurationMultiplier => GetMultiplier(PassiveIds.Invulnerability);
        public float GlobalWeaponDamageBonus => GetBonus(PassiveIds.GlobalDamage);
        public float GlobalWeaponCooldownReduction => GetBonus(PassiveIds.GlobalAttackSpeed);

        private void Awake()
        {
            EnsureReferences();
        }

        private void OnEnable()
        {
            EnsureReferences();
            if (growthState != null)
            {
                growthState.Changed += HandleGrowthChanged;
            }
        }

        private void Start()
        {
            ModifiersChanged?.Invoke();
        }

        public float CalculateWeaponDamage(float baseDamage, float weaponSpecificDamageBonus = 0f)
        {
            float combinedBonus = GlobalWeaponDamageBonus + weaponSpecificDamageBonus;
            return Mathf.Max(0f, baseDamage * (1f + combinedBonus));
        }

        public float CalculateWeaponCooldown(
            float baseCooldown,
            float weaponSpecificCooldownReduction = 0f)
        {
            float combinedReduction = Mathf.Clamp(
                GlobalWeaponCooldownReduction + weaponSpecificCooldownReduction,
                0f,
                MaximumCombinedCooldownReduction);
            return Mathf.Max(
                MinimumWeaponCooldown,
                baseCooldown * (1f - combinedReduction));
        }

        private float GetMultiplier(string passiveId)
        {
            return 1f + GetBonus(passiveId);
        }

        private float GetBonus(string passiveId)
        {
            EnsureReferences();
            if (growthState == null || contentLibrary == null)
            {
                return 0f;
            }

            int level = growthState.GetPassiveLevel(passiveId);
            return level * contentLibrary.GetPassiveBonusPerLevel(passiveId);
        }

        private void EnsureReferences()
        {
            contentLibrary ??= GetComponent<GrowthContentLibrary>();
            growthState ??= GetComponent<GrowthRuntimeState>();
        }

        private void HandleGrowthChanged()
        {
            ModifiersChanged?.Invoke();
        }

        private void OnDisable()
        {
            if (growthState != null)
            {
                growthState.Changed -= HandleGrowthChanged;
            }
        }
    }
}
