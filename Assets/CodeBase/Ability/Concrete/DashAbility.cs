using UnityEngine;
using CodeBase.Player.Movement;
using CodeBase.StaticData.Ability;

namespace CodeBase.Ability.Concrete
{
    public class DashAbility : AbilityBase
    {
        private readonly PlayerMovement playerMovement;
        private readonly DashAbilityData dashData;
        
        private bool isDashing;
        private float dashTimer;
        private Vector2 dashDirection;
        private Vector2 originalVelocity;

        public DashAbility(
            Transform playerTransform,
            Rigidbody2D playerRb,
            PlayerMovement playerMovement,
            DashAbilityData staticData) 
            : base(playerTransform, playerRb, staticData)
        {
            this.playerMovement = playerMovement;
            dashData = staticData;
        }

        protected override void OnExecute()
        {
            // Зберігаємо поточну швидкість
            originalVelocity = playerRb.linearVelocity;
            
            // Визначаємо напрямок dash
            if (originalVelocity.magnitude > 0.1f)
            {
                dashDirection = originalVelocity.normalized;
            }
            else
            {
                // Якщо не рухаємося, dash у напрямку погляду
                dashDirection = new Vector2(playerTransform.localScale.x, 0f);
            }

            isDashing = true;
            dashTimer = 0f;
            
            // Вимикаємо контроль під час dash
            playerMovement.enabled = false;
        }

        protected override void OnTick(float deltaTime)
        {
            if (!isDashing) return;

            dashTimer += deltaTime;
            float progress = dashTimer / dashData.dashDuration;

            if (progress >= 1f)
            {
                EndDash();
                return;
            }

            // Плавний dash через AnimationCurve
            float curveValue = dashData.dashCurve.Evaluate(progress);
            float dashSpeed = (dashData.dashDistance / dashData.dashDuration) * curveValue;
            
            playerRb.linearVelocity = dashDirection * dashSpeed;
        }

        private void EndDash()
        {
            isDashing = false;
            playerRb.linearVelocity = originalVelocity;
            playerMovement.enabled = true;
        }
    }
}
