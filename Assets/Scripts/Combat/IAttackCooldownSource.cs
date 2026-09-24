namespace Survivors.Combat
{
    public interface IAttackCooldownSource
    {
        bool IsAttackOnCooldown { get; }
        float AttackCooldownProgress { get; }
    }
}
