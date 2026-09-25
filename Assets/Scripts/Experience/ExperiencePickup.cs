using Survivors.Player;
using UnityEngine;

namespace Survivors.Experience
{
    [DisallowMultipleComponent]
    public sealed class ExperiencePickup : MonoBehaviour
    {
        [Header("Tracking")]
        [SerializeField, Min(0f)] private float trackingSpeed = 8f;
        [SerializeField, Min(0.01f)] private float collectionDistance = 0.2f;

        private PlayerExperience target;
        private int experienceValue;
        private bool isTracking;
        private bool isCollected;

        public bool IsTracking => isTracking;
        public int ExperienceValue => experienceValue;

        public static ExperiencePickup Spawn(
            ExperiencePickup prefab,
            Vector3 position,
            PlayerExperience target,
            int experienceValue)
        {
            if (prefab == null || target == null || experienceValue <= 0)
            {
                return null;
            }

            ExperiencePickup pickup = Instantiate(prefab, position, Quaternion.identity);
            pickup.Initialize(target, experienceValue);
            return pickup;
        }

        private void Initialize(PlayerExperience playerExperience, int value)
        {
            target = playerExperience;
            experienceValue = value;
        }

        private void Update()
        {
            if (isCollected || target == null || Time.timeScale <= 0f)
            {
                return;
            }

            if (!target.CanGainExperience)
            {
                return;
            }

            Vector3 targetPosition = target.transform.position;

            if (!isTracking)
            {
                float pickupRadius = target.PickupRadius;
                if ((targetPosition - transform.position).sqrMagnitude > pickupRadius * pickupRadius)
                {
                    return;
                }

                isTracking = true;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                trackingSpeed * Time.deltaTime);

            if ((targetPosition - transform.position).sqrMagnitude <= collectionDistance * collectionDistance)
            {
                TryCollect();
            }
        }

        private void TryCollect()
        {
            if (isCollected)
            {
                return;
            }

            isCollected = true;

            if (target.TryAddExperience(experienceValue))
            {
                Destroy(gameObject);
                return;
            }

            isCollected = false;
        }

        private void OnValidate()
        {
            trackingSpeed = Mathf.Max(0f, trackingSpeed);
            collectionDistance = Mathf.Max(0.01f, collectionDistance);
        }
    }
}
