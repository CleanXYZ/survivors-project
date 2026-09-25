using System;
using UnityEngine;

namespace Survivors.Player
{
    public enum PlayerExperienceState
    {
        Normal,
        LevelUpPending
    }

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
        private float timeScaleBeforeLevelUp = 1f;
        private bool pausedForLevelUp;

        public event Action<int, int, int> ExperienceChanged;
        public event Action<int> LeveledUp;
        public event Action<PlayerExperienceState> StateChanged;

        public int Level { get; private set; }
        public int CurrentExperience { get; private set; }
        public int ExperienceToNextLevel => CalculateExperienceRequirement(Level);
        public float PickupRadius => pickupRadius;
        public PlayerExperienceState State { get; private set; }
        public bool IsLevelUpPending => State == PlayerExperienceState.LevelUpPending;
        public bool CanGainExperience => !health.IsDead && State == PlayerExperienceState.Normal;

        private void Awake()
        {
            health = GetComponent<PlayerHealth>();
            Level = startingLevel;
            CurrentExperience = startingExperience;
            State = PlayerExperienceState.Normal;
        }

        private void OnEnable()
        {
            health.Died += HandlePlayerDeath;
        }

        private void Start()
        {
            TryBeginLevelUp();
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

            CurrentExperience = (int)Math.Min(
                (long)CurrentExperience + amount,
                int.MaxValue);
            TryBeginLevelUp();
            NotifyChanged();
            return true;
        }

        public bool CompleteLevelUp()
        {
            if (!IsLevelUpPending)
            {
                return false;
            }

            if (health.IsDead)
            {
                HandlePlayerDeath();
                return false;
            }

            if (TryBeginLevelUp())
            {
                NotifyChanged();
                return true;
            }

            SetState(PlayerExperienceState.Normal);
            ResumeAfterLevelUp();
            return true;
        }

        private bool TryBeginLevelUp()
        {
            int requiredExperience = ExperienceToNextLevel;

            if (CurrentExperience < requiredExperience || health.IsDead)
            {
                return false;
            }

            CurrentExperience -= requiredExperience;
            Level++;
            PauseForLevelUp();
            SetState(PlayerExperienceState.LevelUpPending);
            LeveledUp?.Invoke(Level);
            return true;
        }

        private void PauseForLevelUp()
        {
            if (pausedForLevelUp)
            {
                return;
            }

            timeScaleBeforeLevelUp = Time.timeScale;
            pausedForLevelUp = true;
            Time.timeScale = 0f;
        }

        private void ResumeAfterLevelUp()
        {
            if (!pausedForLevelUp)
            {
                return;
            }

            Time.timeScale = timeScaleBeforeLevelUp;
            pausedForLevelUp = false;
        }

        private void HandlePlayerDeath()
        {
            if (IsLevelUpPending)
            {
                SetState(PlayerExperienceState.Normal);
            }

            ResumeAfterLevelUp();
        }

        private void SetState(PlayerExperienceState newState)
        {
            if (State == newState)
            {
                return;
            }

            State = newState;
            StateChanged?.Invoke(State);
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

        private void OnDisable()
        {
            if (health != null)
            {
                health.Died -= HandlePlayerDeath;
            }

            if (IsLevelUpPending)
            {
                SetState(PlayerExperienceState.Normal);
            }

            ResumeAfterLevelUp();
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
