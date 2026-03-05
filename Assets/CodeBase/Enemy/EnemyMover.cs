using Assets.FantasyMonsters.Common.Scripts;
using Pathfinding;
using UnityEngine;

namespace CodeBase.Enemy
{
    public class EnemyMover : MonoBehaviour
    {
        private Monster _monster;
        private IAstarAI _ai;
        private bool _isAttacking;
        public void SetAttacking(bool value) => _isAttacking = value;

        private void Awake()
        {
            _monster = GetComponentInChildren<Monster>();
            _ai = GetComponent<IAstarAI>();
        }

        private void Update()
        {
            if (_isAttacking) return;
            
            bool isMoving = _ai.velocity.sqrMagnitude > 0.01f;

            _monster.SetState(isMoving ? MonsterState.Walk : MonsterState.Idle);
        }
    }
}