using Survivors.Combat;
using UnityEngine;

namespace Survivors.Enemies
{
    [RequireComponent(typeof(MeleeEnemy), typeof(SpriteRenderer))]
    public sealed class MeleeEnemyCooldownVisual : AttackCooldownRadialVisual
    {
        protected override MonoBehaviour GetCooldownSourceBehaviour()
        {
            return GetComponent<MeleeEnemy>();
        }
    }
}
