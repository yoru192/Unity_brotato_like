using UnityEngine;

namespace CodeBase.Infrastructure.States
{
    public class LoadoutSelectState : IState
    {
        private const string LoadoutSelect = "LoadoutSelect";
        private readonly SceneLoader _sceneLoader;

        public LoadoutSelectState(SceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }
        
        public void Enter()
        {
            Debug.Log("Entering LoadoutSelectState");
            _sceneLoader.Load(LoadoutSelect);
        }
        
        public void Exit()
        {
            
        }
    }
}