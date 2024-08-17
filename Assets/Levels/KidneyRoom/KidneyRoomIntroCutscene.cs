using UnityEngine;
using System.Collections;

public class KidneyRoomIntroCutscene : MonoBehaviour
{

    public bool testing;
    Animator playerAnimator;

    ThirdPersonController playerController;

    [SerializeField] AudioClip introNoise;

    [SerializeField] Quest kidneyQuest;


    void Start()
    {
        playerAnimator = GameManager.Instance.playerAnimator;
        playerController = GameManager.Instance.playerController;

        if(!testing)
            StartCoroutine(IntroCutscene());
    }
    public void StartIntroCutscene()
    {
        StartCoroutine(IntroCutscene());
    }

    IEnumerator IntroCutscene()
    {
        playerController.DeactivateControls();
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("Having a bad time", true);
        }
        else
        {
            Debug.Log("no animator ");
        }
        GameManager.Instance.PlaySound(introNoise);
        yield return new WaitForSeconds(introNoise.length);

        playerAnimator.SetBool("Having a bad time", false);

        yield return new WaitForSeconds(13f * 0.5f);
        
        VoiceLineController playerVoiceLineController = GameManager.Instance.playerVoiceLineController;

        playerVoiceLineController.PlayVoiceLine("where am I");
        yield return new WaitForSeconds(playerVoiceLineController.GetVoiceLineLength("where am I"));

        playerAnimator.SetTrigger("sadge");
        yield return new WaitForSeconds(1);
        playerVoiceLineController.PlayVoiceLine("where did my kidney goo");
        yield return new WaitForSeconds(playerVoiceLineController.GetVoiceLineLength("where did my kidney goo"));

        playerAnimator.SetTrigger("resetAnim");
        playerController.ActivateControls();
        QuestManager.Instance.AddQuest(kidneyQuest);
    }
}
