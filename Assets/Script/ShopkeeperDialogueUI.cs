using System.Collections;
using TMPro;
using UnityEngine;

public class ShopkeeperDialogueUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialogueBubble;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 6f;

    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (dialogueBubble != null)
        {
            dialogueBubble.SetActive(false);
        }
    }

    public void ShowDialogue(string message)
    {
        if (dialogueBubble == null || dialogueText == null)
        {
            Debug.LogWarning("Dialogue UI belum dihubungkan.");
            return;
        }

        if (string.IsNullOrWhiteSpace(message))
            return;

        dialogueText.text = message.Trim();
        dialogueBubble.SetActive(true);

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    public void HideDialogue()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (dialogueBubble != null)
        {
            dialogueBubble.SetActive(false);
        }
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        if (dialogueBubble != null)
        {
            dialogueBubble.SetActive(false);
        }

        hideCoroutine = null;
    }
}