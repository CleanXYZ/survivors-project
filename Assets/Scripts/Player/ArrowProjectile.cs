using Survivors.Enemies;
using UnityEngine;

namespace Survivors.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class ArrowProjectile : MonoBehaviour
    {
        private Rigidbody2D body;
        private float remainingLifetime;
        private int damage;
        private bool hasHit;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        public void Initialize(Vector2 direction, float speed, float lifetime, int attackDamage)
        {
            remainingLifetime = lifetime;
            damage = attackDamage;
            body.linearVelocity = direction.normalized * speed;
        }

        private void Update()
        {
            remainingLifetime -= Time.deltaTime;

            if (remainingLifetime <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasHit)
            {
                return;
            }

            EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();

            if (enemy == null || !enemy.TryTakeDamage(damage))
            {
                return;
            }

            hasHit = true;
            body.linearVelocity = Vector2.zero;
            Destroy(gameObject);
        }
    }
}
