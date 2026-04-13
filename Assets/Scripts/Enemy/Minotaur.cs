using Nethereum.Merkle;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Minotaur : MonoBehaviour
{
    public float currentHp;
    public float maxHp;
    public ItemDatabase data;
    public GameObject armorPrefab;
    void Start()
    {
        StartCoroutine(WaitForPlayerStats());
    }

    IEnumerator WaitForPlayerStats()
    {
        while (PlayerStats.Instance == null || PlayerStats.Instance.maxHp <= 0)
        {
            yield return null;
        }
        maxHp = PlayerStats.Instance.maxHp;
        currentHp = maxHp;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q)) 
        {
            currentHp -= 100;
        }
        Dead();
    }
    public void Dead()
    {
        if(currentHp <= 0)
        {
            Destroy(gameObject);
            DropItem();
        }
    }
    public void DropItem()
    {
        int drop = Random.Range(1,2);
        if(drop == 1)
        {
            ItemData randomData = data.GetRandomArmorData();
            GameObject newItem = Instantiate(armorPrefab, transform.position, Quaternion.identity);
            Collectible collectibleScript = newItem.GetComponent<Collectible>();
            if (collectibleScript != null)
            {
                collectibleScript.Setup(randomData);
            }
        }
    }
}
