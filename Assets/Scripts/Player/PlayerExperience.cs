using System;
using UnityEngine;

namespace Survivors.Player
{
    public sealed class PlayerExperience : MonoBehaviour
    {
        [Header("Starting State")]
        [SerializeField, Min(1)] private int startingLevel = 1;
        [SerializeField, Min(0)] private int startingExperience;

        [Header("Level Requirement")]
        [SerializeField, Min(1)] private int experienceToNextLevel = 10;

        public event Action<int, int, int> ExperienceChanged;
        public event Action<int> LeveledUp;

        public int Level { get; private set; }
        public int CurrentExperience { get; private set; }
        public int ExperienceToNextLevel => experienceToNextLevel;

        private void Awake()
        {
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
            if (amount <= 0)
            {
                return;
            }

            ApplyExperience(amount);
            NotifyChanged();
        }

        private void ApplyExperience(int amount)
        {
            CurrentExperience += Mathf.Max(0, amount);

            while (CurrentExperience >= experienceToNextLevel)
            {
                CurrentExperience -= experienceToNextLevel;
                Level++;
                LeveledUp?.Invoke(Level);
            }
        }

        private void NotifyChanged()
        {
            ExperienceChanged?.Invoke(Level, CurrentExperience, experienceToNextLevel);
        }

        private void OnValidate()
        {
            startingLevel = Mathf.Max(1, startingLevel);
            startingExperience = Mathf.Max(0, startingExperience);
            experienceToNextLevel = Mathf.Max(1, experienceToNextLevel);
        }
    }
}
