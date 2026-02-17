using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Ability
{
    public class AbilityController : MonoBehaviour
    {
        private readonly List<IAbility> abilities = new List<IAbility>();

        public void AddAbility(IAbility ability)
        {
            abilities.Add(ability);
        }

        public void ExecuteAbility(int index)
        {
            if (index >= 0 && index < abilities.Count)
            {
                abilities[index].Execute();
            }
        }

        public IAbility GetAbility(int index)
        {
            return index >= 0 && index < abilities.Count ? abilities[index] : null;
        }

        public int AbilityCount => abilities.Count;

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            foreach (var ability in abilities)
            {
                ability.Tick(deltaTime);
            }
        }
    }
}