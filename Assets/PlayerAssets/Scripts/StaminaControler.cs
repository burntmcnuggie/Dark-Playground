using UnityEngine;
using UnityEngine.UI;

public class StaminaController : MonoBehaviour
{
    ThirdPersonController ThirdPersonControllerReference;
    public float maxStamina = 1;
    public float currentStamina = 1f;
    public float recoverySpeed = 0.05f;
    public float recoverySpeedBoost = 0.05f;
    public float depletionSpeed = 0.05f;
    bool a = true;

    [SerializeField] Slider staminaBar;

    public bool canSprint = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (ThirdPersonControllerReference == null) {
            ThirdPersonControllerReference = GetComponent<ThirdPersonController>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        print("currentStamina = " + currentStamina);
        if (!a && ThirdPersonControllerReference.Grounded) {
            a = true;
        }

        // deplete stamina
        if (currentStamina > 0) {
            if (ThirdPersonControllerReference.isSprinting) {
                currentStamina -= depletionSpeed * Time.deltaTime;
            }
            if (ThirdPersonControllerReference.isJumping) {
                if (a) {
                    currentStamina -= depletionSpeed * 3;
                    a = false;
                }
            }
        }

        // recover stamina
        if (currentStamina < maxStamina && !ThirdPersonControllerReference.isJumping && !ThirdPersonControllerReference.isSprinting) {
            if (ThirdPersonControllerReference.isCrouching) {
                currentStamina += recoverySpeed * 4 * Time.deltaTime;
            }
            else {
                currentStamina += recoverySpeed * Time.deltaTime;
            }
        }

        // update stamina bar
        staminaBar.value = currentStamina;
        canSprint = currentStamina > 0;
    }
}