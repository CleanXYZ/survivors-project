using System;
using UnityEngine;

namespace Survivors.Player
{
    [RequireComponent(typeof(PlayerHealth))]
    public sealed class PlayerExperience : MonoBehaviour
    {
        [Header("Starting State")]
        [SerializeField, Min(1)] private int startingLevel = 1;
        [SerializeField, Min(0)] private int startingExperience;

        [Header("Level Requirement")]
        [SerializeField, Min(1)] private int baseExperienceRequirement = 10;
        [SerializeField, Min(0)] private int experienceGrowthPerLevel = 2;

        [Header("Pickup")]
        [SerializeField, Min(0f)] private float pickupRadius = 3f;

        private PlayerHealth health;

        public event Action<int, int, int> ExperienceChanged;
        public event Action<int> LeveledUp;

        public int Level { get; private set; }
        public int CurrentExperience { get; private set; }
        public int ExperienceToNextLevel => CalculateExperienceRequirement(Level);
        public float PickupRadius => pickupRadius;
        public bool CanGainExperience => !health.IsDead;

        private void Awake()
        {
            health = GetComponent<PlayerHealth>();
            Level = startingLevel;
            CurrentExperience = 0;
            ApplyExperience(startingExperience);
        }

        private void Start()
        {
            NotifyChanged();
        }

        public void AddExperience(int amount)
        {
            TryAddExperience(amount);
        }

        public bool TryAddExperience(int amount)
        {
            if (amount <= 0 || !CanGainExperience)
            {
                return false;
            }

            ApplyExperience(amount);
            NotifyChanged();
            return true;
        }

        private void ApplyExperience(int amount)
        {
            CurrentExperience += Mathf.Max(0, amount);

            while (CurrentExperience >= ExperienceToNextLevel)
            {
                CurrentExperience -= ExperienceToNextLevel;
                Level++;
                LeveledUp?.Invoke(Level);
            }
        }

        private int CalculateExperienceRequirement(int level)
        {
            long requirement = (long)baseExperienceRequirement
                + (long)Mathf.Max(0, level - 1) * experienceGrowthPerLevel;
            return (int)Math.Min(requirement, int.MaxValue);
        }

        private void NotifyChanged()
        {
            ExperienceChanged?.Invoke(Level, CurrentExperience, ExperienceToNextLevel);
        }

        private void OnValidate()
        {
            startingLevel = Mathf.Max(1, startingLevel);
            startingExperience = Mathf.Max(0, startingExperience);
            baseExperienceRequirement = Mathf.Max(1, baseExperienceRequirement);
            experienceGrowthPerLevel = Mathf.Max(0, experienceGrowthPerLevel);
            pickupRadius = Mathf.Max(0f, pickupRadius);
        }
    }
}
