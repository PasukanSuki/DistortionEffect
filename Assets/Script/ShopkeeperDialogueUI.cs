using System.Collections;
using TMPro;
using UnityEngine;

public class ShopkeeperDialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBubble;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private float displayDuration = 5f;

    private Coroutine hideCoroutine;

    private void Start()
    {
        dialogueBubble.SetActive(false);
    }

    public void ShowDialogue(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        dialogueText.text = message;

        dialogueBubble.SetActive(true);

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        hideCoroutine = StartCoroutine(HideDialogue());
    }

    private IEnumerator HideDialogue()
    {
        yield return new WaitForSeconds(displayDuration);

        dialogueBubble.SetActive(false);
        hideCoroutine = null;
    }
}