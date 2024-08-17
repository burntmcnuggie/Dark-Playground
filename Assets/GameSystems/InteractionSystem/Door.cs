using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [SerializeField] private AudioClip[] doorSounds;
    [SerializeField] private AudioClip lockedSound; // Sound to play when the door is locked

    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float swingDuration = 1f;
    [SerializeField] private bool openDirectionPositive = true;
    [SerializeField] public string requiredKey = "MasterKey"; // The specific string value for the key to unlock the door

    public bool isOpen = false;
    public bool isLocked = true; // Initial locked status of the door
    private bool isSwinging = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine swingCoroutine;
    [SerializeField] private BoxCollider boxCollider; // Reference to the BoxCollider

    private void Start()
    {
        closedRotation = transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(0, 0, openDirectionPositive ? openAngle : -openAngle);
        boxCollider = GetComponent<BoxCollider>(); // Get the BoxCollider component

        if (isOpen)
        {
            swingCoroutine = StartCoroutine(SwingDoor(openRotation));
        }
    }

    public void OnInteract(PlayerInteraction interactor)
    {
        if (isSwinging)
        {
            StopCoroutine(swingCoroutine);
        }

        if (isLocked)
        {
            // Check if the player has the correct key
            if (InventoryManager.Instance.CheckHasKey(requiredKey))
            {
                isLocked = false; // Unlock the door
                print("Door unlocked");
            }
            else
            {
                // Play locked door sound if locked
                if (lockedSound != null)
                {
                    AudioSource.PlayClipAtPoint(lockedSound, transform.position);
                }
                return; // Exit the method to prevent door from toggling open state
            }
        }

        isOpen = !isOpen;
        swingCoroutine = StartCoroutine(SwingDoor(isOpen ? openRotation : closedRotation));

        if (doorSounds.Length > 0)
        {
            AudioClip doorSound = doorSounds[Random.Range(0, doorSounds.Length)];
            AudioSource.PlayClipAtPoint(doorSound, transform.position);
        }
    }

    private IEnumerator SwingDoor(Quaternion targetRotation)
    {
        isSwinging = true;
        float elapsedTime = 0f;
        Quaternion startRotation = transform.rotation;

        while (elapsedTime < swingDuration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / swingDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
        isSwinging = false;
    }

    private bool PlayerHasKey(string key)
    {
        // Check if the player has the required key. Implement your own logic here
        return true; // This should interface with your game's inventory system
    }

    // private void OnDrawGizmos()
    // {
    //     if (!boxCollider) return; // Early exit if boxCollider is not set

    //     // Calculate the center of the door based on the BoxCollider's center and transform
    //     Vector3 centerPoint = transform.position + transform.rotation * Vector3.Scale(boxCollider.center, transform.localScale);

    //     // Adjust the size of the gizmo based on the BoxCollider's size and transform
    //     Vector3 gizmoSize = Vector3.Scale(boxCollider.size, transform.localScale);

    //     // Draw the door in its current state (open or closed)
    //     Gizmos.color = isOpen ? Color.green : Color.red; 
    //     Gizmos.matrix = Matrix4x4.TRS(centerPoint, transform.rotation, gizmoSize);
    //     Gizmos.DrawCube(Vector3.zero, new Vector3(1, 2, 0.1f));

    //     // If not in play mode, draw the door's open position for visualization
    //     if (!Application.isPlaying)
    //     {
    //         Gizmos.color = Color.yellow;
    //         Quaternion openStateRotation = closedRotation * Quaternion.Euler(0, openDirectionPositive ? openAngle : -openAngle, 0);
    //         Gizmos.matrix = Matrix4x4.TRS(centerPoint, openStateRotation, gizmoSize);
    //         Gizmos.DrawCube(Vector3.zero, new Vector3(1, 2, 0.1f));
    //     }
    // }

}
