using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DamagePopupPool : MonoBehaviour
{
    public static DamagePopupPool Instance;

    [Header("Prefab Damage Popup")]
    public GameObject damagePopupPrefab;

    private Queue<GameObject> poolQueue = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Tạo sẵn một ít để tránh lag lúc đầu
        for (int i = 0; i < 8; i++)
        {
            CreateNewPopup();
        }
    }

    // Tạo mới một DamagePopup khi pool hết
    private GameObject CreateNewPopup()
    {
        GameObject obj = Instantiate(damagePopupPrefab, transform);
        obj.SetActive(false);
        poolQueue.Enqueue(obj);
        return obj;
    }

    public void ShowDamage(int damageAmount, Vector3 position)
    {
        // Nếu pool hết thì tự động tạo thêm
        if (poolQueue.Count == 0)
        {
            CreateNewPopup();
        }

        GameObject popup = poolQueue.Dequeue();
        popup.SetActive(true);
        popup.transform.position = position;

        DamagePopup damageScript = popup.GetComponent<DamagePopup>();
        if (damageScript != null)
        {
            damageScript.Setup(damageAmount, position);
        }

        // Trả lại pool sau khi animation xong
        StartCoroutine(ReturnToPool(popup));
    }

    private IEnumerator ReturnToPool(GameObject obj)
    {
        yield return new WaitForSeconds(1.6f);

        if (obj != null)
        {
            obj.SetActive(false);
            poolQueue.Enqueue(obj);
        }
    }
}