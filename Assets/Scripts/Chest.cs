using UnityEngine;

public class Chest : MonoBehaviour
{
    [Header("Chest Models")]
    [SerializeField]
    [Tooltip("The closed chest model")]
    public GameObject closedChestModel;

    [SerializeField]
    [Tooltip("The opened chest model")]
    public GameObject openedChestModel;

    [SerializeField]
    [Tooltip("Tag or name to identify the key object")]
    private string keyTag = "Key";

    private bool isOpen = false;

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
        if (isOpen)
            CloseChest();
        else
            OpenChest();
    }

    public bool IsOpen => isOpen;
}
