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
                monster.Animator.Rebind();
                monster.Animator.Update(0f);
                monster.SetState(MonsterState.Idle);
            }

            if (TryGetComponent<IAstarAI>(out var ai))
            {
                ai.isStopped = false;
                ai.Teleport(transform.position);
            }

            if (TryGetComponent<EnemyAttack>(out var attack))
                attack.enabled = true;
            if (TryGetComponent<EnemyMover>(out var mover))
                mover.enabled = true;
            if (TryGetComponent<EnemyRangerAttack>(out var ranger))
                ranger.enabled = true;
        }

        public void OnDespawn() { }
    }
}