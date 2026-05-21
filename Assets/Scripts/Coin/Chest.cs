using UnityEngine;

public class Chest : MonoBehaviour
{
    public float interactRadius = 0.5f;
    public GameObject chestUI;
    public GameObject closeChest;
    public GameObject openChest;
    public BoxCollider2D boxCollider;
    private bool isPlayerNear;
    
    void Start()
    {
       
    }

    void Update()
    {
       
        if (isPlayerNear == true)
        {
            chestUI.SetActive(true);

            if (Input.GetKeyDown(KeyCode.F))
            {
                OpenChest();
            }
        }
        else
        {
            chestUI.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
           isPlayerNear = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            isPlayerNear = false;
        }
    }


    private void OpenChest()
    {
        isPlayerNear = false;
        openChest.gameObject.SetActive(true);
        closeChest.gameObject.SetActive(false);
        chestUI.gameObject.SetActive(false);
        boxCollider.enabled = false;
    }
}
