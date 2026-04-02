using UnityEngine;

public class LoadSceneAfterLogout : MonoBehaviour
{
    public static LoadSceneAfterLogout Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadNextScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Login");
    }
}
