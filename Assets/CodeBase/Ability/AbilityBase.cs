using UnityEngine;
using CodeBase.StaticData;

namespace CodeBase.Ability
{
    public abstract class AbilityBase : IAbility
    {
        public string AbilityName { get; protected set; }
        public float Cooldown { get; protected set; }
        public float RemainingCooldown { get; protected set; }
        public bool IsReady => RemainingCooldown <= 0f;

        protected readonly Transform playerTransform;
        protected readonly Rigidbody2D playerRb;
        
        protected AbilityBase(
            Transform playerTransform,
            Rigidbody2D playerRb,
            AbilityStaticData staticData)
        {
            this.playerTransform = playerTransform;
            this.playerRb = playerRb;
            AbilityName = staticData.menuName;
            Cooldown = staticData.abilityCooldown;
            RemainingCooldown = 0f;
        }

        public void Execute()
        {
            if (!IsReady) return;
            
            OnExecute();
            RemainingCooldown = Cooldown;
        }

        public void Tick(float deltaTime)
        {
            if (RemainingCooldown > 0f)
                RemainingCooldown -= deltaTime;
                
            OnTick(deltaTime);
        }

        protected abstract void OnExecute();
        protected virtual void OnTick(float deltaTime) { }
    }
}