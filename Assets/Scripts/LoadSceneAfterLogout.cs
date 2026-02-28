using UnityEngine;

public class LoadSceneAfterLogout : MonoBehaviour
{
    public static LoadSceneAfterLogout Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ đối tượng này tồn tại vĩnh viễn
        }
        else
        {
            Destroy(gameObject); // Xóa bản sao thừa nếu quay lại cảnh cũ
        }
    }

    public void LoadNextScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Login");
    }
}
