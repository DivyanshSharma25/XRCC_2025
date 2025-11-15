using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Chest : MonoBehaviour
{
    [Header("Chest Models")]
    [SerializeField]
    [Tooltip("The closed chest model")]
    public GameObject closedChestModel;
    public bool jumpscare = false;
    [SerializeField]
    [Tooltip("The opened chest model")]
    public GameObject openedChestModel;
    public AudioSource jump_scare_audio;
    [SerializeField]
    [Tooltip("Tag or name to identify the key object")]
    private string keyTag = "Key";

    private bool isOpen = false;

    string curent_key;

    private void Start()
    {
        // Ensure initial state
        if (closedChestModel != null)
            closedChestModel.SetActive(true);

        if (openedChestModel != null)
            openedChestModel.SetActive(false);

        isOpen = false;
    }

    // private void CheckSocketForKey()
    // {
    //     if (socketInteractor == null)
    //         return;

    //     // Try to get hasSelection property using reflection
    //     var hasSelectionProperty = socketInteractor.GetType().GetProperty("hasSelection");

    //     bool hasKeyAttached = false;
    //     if (hasSelectionProperty != null)
    //     {
    //         hasKeyAttached = (bool)hasSelectionProperty.GetValue(socketInteractor);
    //     }

    //     if (hasKeyAttached && !isOpen)
    //     {
    //         // A key has been attached - open the chest
    //         OpenChest();
    //     }
    //     else if (!hasKeyAttached && isOpen)
    //     {
    //         // Key was removed - close the chest
    //         CloseChest();
    //     }
    // }

    private void OpenChest()
    {
        isOpen = true;

        if (closedChestModel != null)
            closedChestModel.SetActive(false);

        if (openedChestModel != null)
            openedChestModel.SetActive(true);

        Debug.Log("Chest opened!");
    }

    private void CloseChest()
    {
        isOpen = false;

        if (closedChestModel != null)
            closedChestModel.SetActive(true);

        if (openedChestModel != null)
            openedChestModel.SetActive(false);

        Debug.Log("Chest closed!");
    }

    /// <summary>
    /// Public method to manually open/close the chest (optional)
    /// </summary>
    public void ToggleChest()
    {
        if (curent_key == keyTag)
        {
            {
                if (isOpen)
                    CloseChest();
                else
                    OpenChest();
                if (jumpscare)
                {
                    jump_scare_audio.Play();
                }
            }
        }

    }

    public bool IsOpen => isOpen;

    private void OnTriggerEnter(Collider other)
    {
        curent_key = other.gameObject.name;
    }
    private void OnTriggerExit(Collider other)
    {
        curent_key = "";
    }
}
