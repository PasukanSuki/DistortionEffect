using UnityEngine;

public class DialogueBillboard : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            return;
        }

        transform.rotation = Quaternion.LookRotation(
            transform.position - mainCamera.transform.position
        );
    }
}