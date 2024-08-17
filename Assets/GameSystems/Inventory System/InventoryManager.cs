using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [SerializeField] private List<ItemSO> hotbarItems = new List<ItemSO>(5);
    List<string> keysHeld=  new List<string>();
    private void Awake()
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

    public void AddItem(ItemSO newItem)
    {
        if (hotbarItems.Count < 5)
        {
            hotbarItems.Add(newItem);
            newItem.OnPickup();
        }
        else
        {
            Debug.Log("Hotbar full. Swap or drop an item.");
        }
    }

    public void UseItem(int slot)
    {
        if (slot >= 0 && slot < hotbarItems.Count)
        {
            hotbarItems[slot].Use();
        }
    }

    public void SwapItems(int slot1, int slot2)
    {
        ItemSO temp = hotbarItems[slot1];
        hotbarItems[slot1] = hotbarItems[slot2];
        hotbarItems[slot2] = temp;
        Debug.Log("Items swapped.");
    }

    public void DropItem(int slot)
    {
        if (slot < hotbarItems.Count)
        {
            hotbarItems[slot].OnDrop();
            hotbarItems.RemoveAt(slot);
        }
    }

    public void AddKey(string keyAdded)
    {
        keysHeld.Add(keyAdded);
    }

    public bool CheckHasKey(string keyChecked)
    {
        return keysHeld.Contains(keyChecked);
    }


}
