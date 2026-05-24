using System;
using UnityEngine;

namespace CodeBase.Logic
{
    public class PauseInputHandler : MonoBehaviour
    {
        public event Action OnPausePressed;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                OnPausePressed?.Invoke();
        }
    }
}
