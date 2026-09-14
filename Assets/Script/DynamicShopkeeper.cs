using System.Collections.Generic;
using Convai.Runtime.Components;
using UnityEngine;

public class DynamicShopkeeper : MonoBehaviour
{
    [Header("Convai")]
    [SerializeField] private ConvaiCharacter arthur;

    [Header("Dialogue")]
    [SerializeField] private ShopkeeperDialogueUI dialogueUI;

    private bool characterReady;
    private bool waitingForReply;

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

        // Project ini hanya memakai teks: jangan kirim input mikrofon ke Convai.
        arthur.DisableRemoteAudio();
        DisableVoiceInput();

        Debug.Log("Arthur READY - Text Dialogue Mode.");

        FlushPendingEvents();
    }

    private void OnArthurReady()
    {
        characterReady = true;
        arthur.DisableRemoteAudio();
        DisableVoiceInput();

        Debug.Log("Arthur character ready.");

        FlushPendingEvents();
    }

    private void OnMouseDown()
    {
        Interact();
    }

    public void Interact()
    {
        if (waitingForReply)
            return;

        waitingForReply = true;

        if (dialogueUI != null)
            dialogueUI.ShowDialogue("...");

        SendArthurEvent(
            "The player clicked you. Greet them warmly as Arthur the shopkeeper. " +
            "Be friendly and slightly humorous. Reply with only one short sentence."
        );
    }

    public void TestGreeting()
    {
        Interact();
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

        waitingForReply = false;

        Debug.Log(
            "Arthur: " + text
        );

        dialogueUI.ShowDialogue(text);
    }

    private void DisableVoiceInput()
    {
        var manager = ConvaiManager.ActiveManager;
        if (manager == null)
            return;

        if (manager.TryGetRoomAudioService(out var audioService))
            audioService.SetMicMuted(true);

        if (manager.TryGetRoomConnectionService(out var connectionService))
            connectionService.SetSttMuted(true);
    }
}
