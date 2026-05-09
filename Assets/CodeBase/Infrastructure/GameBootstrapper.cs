using CodeBase.Infrastructure.States;
using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Infrastructure
{
    public class GameBootstrapper : MonoBehaviour, ICoroutineRunner
    {
        public LoadingCurtain curtainPrefab;
        
        private Game _game;
        
        private void Awake()
        {
            var curtain = Instantiate(curtainPrefab);
            curtain.gameObject.SetActive(false);
            _game = new Game(this, curtain);
            _game.stateMachine.Enter<BootstrapState>();
            DontDestroyOnLoad(this);
        }
        
    }
}