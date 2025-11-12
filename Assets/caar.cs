using System.Collections; // Required for Coroutines
using Unity.Mathematics; // Required for float3, etc.
using UnityEngine;
using UnityEngine.Splines; // Required for using splines
using UnityEngine.XR.Interaction.Toolkit; // Required for VR

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class CarPathFollower : MonoBehaviour
{
    [Header("Path to Follow")]
    [Tooltip("The SplineContainer object that defines the path for the car.")]
    public SplineContainer path;

    [Header("References (MUST ASSIGN)")]
    [Tooltip("The main 'XR Origin' or 'XR Rig' object that represents the player")]
    public GameObject playerRig;

    [Header("Movement Settings")]
    [Tooltip("How fast the car moves along the path (in meters per second).")]
    public float speed = 5.0f;

    // --- THIS IS THE FIX ---
    // Changed 't' from 'double' to 'float' to match the Spline.Evaluate method
    private float t = 0.0f;
    // -----------------------

    private bool isMoving = false;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable carInteractable;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor playerHand = null;

    void Awake()
    {
        // Get the grabbable component on this car
        carInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        // Subscribe to the grab event
        carInteractable.selectEntered.AddListener(StartRide);
    }

    void OnDestroy()
    {
        carInteractable.selectEntered.RemoveListener(StartRide);
    }

    /// <summary>
    /// Called when the player grabs the car.
    /// </summary>
    private void StartRide(SelectEnterEventArgs args)
    {
        // Don't start if we're already moving
        if (isMoving) return;

        // Store the player's hand
        playerHand = args.interactorObject;

        // Start the movement
        StartCoroutine(MoveAlongPath());
    }

    /// <summary>
    /// Moves both the car and the player along the spline path.
    /// </summary>
    private IEnumerator MoveAlongPath()
    {
        isMoving = true;
        t = 0.0f; // Start from the beginning

        // Loop until we reach the end of the path (t = 1.0)
        while (t < 1.0f) // <-- Also change here
        {
            // 1. Get the total length of the spline path
            float pathLength = path.Spline.GetLength();

            // 2. Calculate how far to move this frame
            float distanceThisFrame = speed * Time.deltaTime;

            // 3. Update our progress 't' (a value from 0 to 1)
            t += (distanceThisFrame / pathLength);

            // 4. Get the new position and rotation from the spline
            //    This function now correctly receives a 'float'
            path.Spline.Evaluate(t, out float3 pos, out float3 forward, out float3 up);

            // 5. Apply the position and rotation to the CAR
            transform.position = pos;
            transform.rotation = Quaternion.LookRotation(forward, up);

            // 6. Apply the position and rotation to the PLAYER RIG
            playerRig.transform.position = pos;
            playerRig.transform.rotation = Quaternion.LookRotation(forward, up);

            yield return null; // Wait for the next frame
        }

        // --- Ride is Over ---

        // 7. Snap to the final position
        path.Spline.Evaluate(1.0f, out float3 endPos, out float3 endForward, out float3 endUp); // <-- Also change here
        transform.position = endPos;
        transform.rotation = Quaternion.LookRotation(endForward, endUp);
        playerRig.transform.position = endPos;
        playerRig.transform.rotation = Quaternion.LookRotation(endForward, endUp);

        // 8. Force the player's hand to let go of the car
        if (playerHand != null)
        {
            carInteractable.interactionManager.SelectExit(playerHand, carInteractable);
        }

        isMoving = false;
        playerHand = null;
    }
}