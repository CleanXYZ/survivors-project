using Survivors.Combat;
using UnityEngine;

namespace Survivors.Player
{
    [RequireComponent(typeof(PlayerBowAttack), typeof(SpriteRenderer))]
    public sealed class PlayerBowCooldownVisual : AttackCooldownRadialVisual
    {
        protected override MonoBehaviour GetCooldownSourceBehaviour()
        {
            return GetComponent<PlayerBowAttack>();
        }
    }
}
