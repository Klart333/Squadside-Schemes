using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuDedicatedServer : MonoBehaviour
{
    private void Start()
    {
#if DEDICATED_SERVER
        Debug.Log("Dedicated Server");

        SceneManager.LoadScene(1);

#endif
    }
}
