using CodeBase.Ability;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeBase.Player
{
    public class PlayerAbilityInput : MonoBehaviour
    {
        private AbilityController abilityController;
        private PlayerControls controls;
        
        private InputAction ability1Action;
        private InputAction ability2Action;

        public void Construct(AbilityController abilityController)
        {
            this.abilityController = abilityController;
        }

        private void Awake()
        {
            controls = new PlayerControls();
            
            ability1Action = controls.Player.Ability1;
            ability2Action = controls.Player.Ability2;
        }

        private void OnEnable()
        {
            controls.Enable();
            ability1Action.performed += OnAbility1;
            ability2Action.performed += OnAbility2;
        }

        private void OnDisable()
        {
            ability1Action.performed -= OnAbility1;
            ability2Action.performed -= OnAbility2;
            controls.Disable();
        }

        private void OnAbility1(InputAction.CallbackContext context)
        {
            abilityController?.ExecuteAbility(0);
        }

        private void OnAbility2(InputAction.CallbackContext context)
        {
            abilityController?.ExecuteAbility(1);
        }
    }
}