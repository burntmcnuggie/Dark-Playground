using UnityEngine;

public class Key : MonoBehaviour
{
   public string keyID;
   void OnTriggerEnter(Collider other)
   {
    if(other.gameObject.CompareTag("Player"))
    {
        InventoryManager.Instance.AddKey(keyID);
    }
   }
}
