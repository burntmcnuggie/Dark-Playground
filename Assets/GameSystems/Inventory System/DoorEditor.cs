using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(Door))]
public class DoorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Door doorScript = (Door)target;

        if (GUILayout.Button("Generate Key"))
        {
            GenerateKeyForDoor(doorScript);
        }
    }

    private void GenerateKeyForDoor(Door door)
    {
        // Define the offset in local coordinates (e.g., right in front of the door)
        Vector3 offset = new Vector3(0f, 0f, 1f); // Change this as needed

        string[] keyPrefabs = Directory.GetFiles(Application.dataPath + "/Resources/Keys", "*.prefab");
        if (keyPrefabs.Length == 0)
        {
            Debug.LogError("No key prefabs found in Resources/Keys folder!");
            return;
        }

        // Pick a random key prefab
        string randomKeyPrefabPath = keyPrefabs[Random.Range(0, keyPrefabs.Length)];
        GameObject keyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(randomKeyPrefabPath);

        // Generate a unique key ID
        string uniqueKeyID = System.Guid.NewGuid().ToString();

        // Instantiate and configure the key prefab
        GameObject keyInstance = PrefabUtility.InstantiatePrefab(keyPrefab) as GameObject;
        Key keyScript = keyInstance.GetComponent<Key>();
        if (keyScript == null)
        {
            keyScript = keyInstance.AddComponent<Key>();
        }
        keyScript.keyID = uniqueKeyID;

        // Position the key relative to the door using the local space offset
        keyInstance.transform.position = door.transform.position + door.transform.TransformDirection(offset);
        keyInstance.transform.rotation = door.transform.rotation;

        // Assign the unique key ID to the door
        door.requiredKey = uniqueKeyID;

        Debug.Log("Key generated with ID: " + uniqueKeyID + " at position: " + keyInstance.transform.position);
    }
}