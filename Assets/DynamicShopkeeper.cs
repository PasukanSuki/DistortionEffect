using System.Collections.Generic;
using Convai.Runtime.Components;
using UnityEngine;

public class DynamicShopkeeper : MonoBehaviour
{
    [Header("Convai")]
    [SerializeField] private ConvaiCharacter arthur;

    [Header("Dialogue")]
    [SerializeField] private ShopkeeperDialogueUI dialogueUI;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Settings")]
    [SerializeField] private bool greetPlayerAutomatically = true;

    private bool characterReady;
    private bool playerInside;
    private bool greetedThisVisit;
    private bool purchasedThisVisit;

    private readonly Queue<string> pendingEvents = new Queue<string>();

    private void OnEnable()
    {
        if (arthur != null)
        {
            arthur.OnTranscriptReceived += OnArthurTranscriptReceived;
            arthur.OnCharacterReady += OnArthurReady;
        }
    }

    private void OnDisable()
    {
        if (arthur != null)
        {
            arthur.OnTranscriptReceived -= OnArthurTranscriptReceived;
            arthur.OnCharacterReady -= OnArthurReady;
        }
    }

    private async void Start()
    {
        if (arthur == null)
        {
            Debug.LogError(
                "DynamicShopkeeper: ConvaiCharacter Arthur belum di-assign."
            );

            return;
        }

        if (dialogueUI == null)
        {
            Debug.LogError(
                "DynamicShopkeeper: Dialogue UI belum di-assign."
            );

            return;
        }

        Debug.Log("Menghubungkan Arthur ke Convai...");

        var startOperation =
            arthur.StartConversationAsync(destroyCancellationToken);

        await startOperation;

        if (!startOperation.IsSuccessful)
        {
            Debug.LogError(
                "Arthur gagal terhubung ke Convai: " +
                startOperation.Error.Message
            );

            return;
        }

        var readyOperation =
            arthur.WaitForCharacterReadyAsync(
                timeoutSeconds: 15f,
                destroyCancellationToken
            );

        await readyOperation;

        if (!readyOperation.IsSuccessful)
        {
            Debug.LogError(
                "Arthur tidak ready. Periksa API Key, Character ID, dan internet."
            );

            return;
        }

        characterReady = true;

        // Project ini TEXT ONLY.
        // Tidak membutuhkan AudioSource atau ConvaiAudioOutput.
        arthur.DisableRemoteAudio();

        Debug.Log("Arthur READY - Text Dialogue Mode.");

        FlushPendingEvents();
    }

    private void OnArthurReady()
    {
        characterReady = true;
        arthur.DisableRemoteAudio();

        Debug.Log("Arthur character ready.");

        FlushPendingEvents();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (playerInside)
            return;

        playerInside = true;
        purchasedThisVisit = false;

        if (greetPlayerAutomatically && !greetedThisVisit)
        {
            greetedThisVisit = true;

            SendArthurEvent(
                "A customer has just approached your shop. " +
                "Greet the customer warmly as Arthur the shopkeeper. " +
                "Be friendly and slightly humorous. " +
                "Reply with only one short sentence."
            );
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInside = false;
        greetedThisVisit = false;
        purchasedThisVisit = false;

        dialogueUI.HideDialogue();
    }

    public void NotifyPurchase(string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            itemName = "an item";
        }

        purchasedThisVisit = true;

        SendArthurEvent(
            $"The customer has successfully purchased {itemName}. " +
            $"React happily as Arthur the shopkeeper and briefly comment " +
            $"on the customer's choice. Reply with only one short sentence."
        );
    }

    public void OnShopExitPressed()
    {
        if (!playerInside)
            return;

        if (purchasedThisVisit)
        {
            SendArthurEvent(
                "The customer is leaving after making a purchase. " +
                "Give them a short friendly farewell as Arthur. " +
                "Reply with only one short sentence."
            );
        }
        else
        {
            SendArthurEvent(
                "The customer is leaving the shop without buying anything. " +
                "React with playful but friendly disappointment as Arthur. " +
                "Do not insult the customer. " +
                "Reply with only one short sentence."
            );
        }
    }

    public void TestGreeting()
    {
        SendArthurEvent(
            "Greet the customer as Arthur the shopkeeper. " +
            "Reply with only one short sentence."
        );
    }

    private void SendArthurEvent(string eventMessage)
    {
        if (arthur == null)
            return;

        if (!characterReady || !arthur.IsCharacterReady)
        {
            Debug.Log(
                "Arthur belum ready. Event dimasukkan ke queue."
            );

            pendingEvents.Enqueue(eventMessage);
            return;
        }

        Debug.Log(
            "Event dikirim ke Arthur: " + eventMessage
        );

        arthur.SendNarrativeEvent(eventMessage);
    }

    private void FlushPendingEvents()
    {
        if (!characterReady || arthur == null)
            return;

        while (pendingEvents.Count > 0)
        {
            string eventMessage = pendingEvents.Dequeue();

            arthur.SendNarrativeEvent(eventMessage);
        }
    }

    private void OnArthurTranscriptReceived(
        string text,
        bool isFinal
    )
    {
        if (!isFinal)
            return;

        if (string.IsNullOrWhiteSpace(text))
            return;

        Debug.Log(
            "Arthur: " + text
        );

        dialogueUI.ShowDialogue(text);
    }
}