using UnityEngine;

public class ShopkeeperTrigger : MonoBehaviour
{
    [SerializeField] private ShopkeeperDialogueUI dialogueUI;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            dialogueUI.ShowDialogue(
                "Welcome, traveler! Take a look around."
            );
        }
    }
}