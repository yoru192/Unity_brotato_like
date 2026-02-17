namespace CodeBase.Ability
{
    public interface IAbility
    {
        string AbilityName { get; }
        float Cooldown { get; }
        float RemainingCooldown { get; }
        bool IsReady { get; }
        
        void Execute();
        void Tick(float deltaTime);
    }
}