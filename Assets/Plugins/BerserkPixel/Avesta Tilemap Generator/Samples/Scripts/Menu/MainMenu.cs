using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour 
{

    public void Click_GoToMenu() 
    {
        SceneManager.LoadScene((int)SceneIndexes.Menu);
    }

    public void Click_GoToDemo() 
    {
        SceneManager.LoadScene((int)SceneIndexes.Demo);
    }

    public void Click_GoToOcclussion() 
    {
        SceneManager.LoadScene((int)SceneIndexes.Occlussion);
    }
}

public enum SceneIndexes {
    Menu = 0,
    Demo = 1,
    Occlussion = 2,
    Destructible = 3,
    Bomberman = 4
}