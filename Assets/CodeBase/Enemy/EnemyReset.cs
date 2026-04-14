using Assets.FantasyMonsters.Common.Scripts;
using CodeBase.Logic;
using Pathfinding;
using UnityEngine;

namespace CodeBase.Enemy
{
    public class EnemyReset : MonoBehaviour, IPoolable
    {
        public void OnSpawn()
        {
            Monster monster = GetComponentInChildren<Monster>();
            if (monster != null)
            {
                monster.SetState(MonsterState.Idle);
            }

            if (TryGetComponent<IAstarAI>(out var ai))
            {
                ai.isStopped = false;
                ai.Teleport(transform.position);
            }

            if (TryGetComponent<EnemyAttack>(out var attack))
                attack.enabled = true;
                attack.ResetState();
            if (TryGetComponent<EnemyMover>(out var mover))
                mover.enabled = true;
            if (TryGetComponent<EnemyRangerAttack>(out var ranger))
                ranger.enabled = true;
            if (TryGetComponent<AttackIndicator>(out var indicator))
                indicator.Hide();
        }

        public void OnDespawn() { }
    }
}