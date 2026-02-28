using UnityEngine;

public class Wallet : MonoBehaviour
{
    public static Wallet Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 🔥 giữ khi đổi scene
        }
        else
        {
            Destroy(gameObject);
        }
    }
}