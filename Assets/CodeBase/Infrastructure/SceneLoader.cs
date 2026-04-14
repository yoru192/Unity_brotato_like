using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeBase.Infrastructure
{
    public class SceneLoader
    {
        private readonly ICoroutineRunner _coroutineRunner;

        public SceneLoader(ICoroutineRunner coroutineRunner) =>
            _coroutineRunner = coroutineRunner;

        public void Load(string sceneName, Action onLoaded = null, Action<float> onProgress = null) =>
            _coroutineRunner.StartCoroutine(LoadScene(sceneName, onLoaded, onProgress));

        private IEnumerator LoadScene(string sceneName, Action onLoaded = null, Action<float> onProgress = null)
        {
            if (SceneManager.GetActiveScene().name == sceneName)
            {
                onLoaded?.Invoke();
                yield break;
            }

            AsyncOperation waitNextScene = SceneManager.LoadSceneAsync(sceneName);
            waitNextScene.allowSceneActivation = false;
            
            while (waitNextScene.progress < 0.9f)
            {
                float p = waitNextScene.progress / 0.9f;
                onProgress?.Invoke(p);
                Debug.Log($"[SceneLoader] progress: {p:F2}, onProgress is null: {onProgress == null}");
                yield return null;
            }

            onProgress?.Invoke(1f);
            waitNextScene.allowSceneActivation = true;

            while (!waitNextScene.isDone)
                yield return null;

            onLoaded?.Invoke();
        }
    }
}