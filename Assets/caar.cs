using System.Collections; // Required
using Unity.Mathematics; // Required
using UnityEngine;
using UnityEngine.Splines; // Required
using UnityEngine.XR.Interaction.Toolkit; // Required

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
[RequireComponent(typeof(Rigidbody))] // <-- Now required
public class SplineCar : MonoBehaviour
{
    [Header("Path (MUST ASSIGN)")]
    [Tooltip("The SplineContainer object that defines the path.")]
    public SplineContainer path;

    [Header("References (MUST ASSIGN)")]
    [Tooltip("The main 'XR Origin' or 'XR Rig' object for the player.")]
    public GameObject playerRig;
    [Tooltip("An empty object inside the car where the player should sit.")]
    public Transform playerSeat;

    [Header("Movement Settings")]
    [Tooltip("How fast the car moves along the path (in meters per second).")]
    public float speed = 10.0f;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;
    private bool isAtStart = true;
    private bool isMoving = false;

    // --- THIS IS THE FIX ---
    private Rigidbody rb; // Add a reference for the Rigidbody
    private CharacterController playerCharacterController;
    private ContinuousMoveProviderBase moveProvider;
    private UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider teleportProvider;
    // --- END FIX ---

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        rb = GetComponent<Rigidbody>(); // <-- Get the Rigidbody

        if (playerRig != null)
        {
            playerCharacterController = playerRig.GetComponent<CharacterController>();
            moveProvider = playerRig.GetComponent<ContinuousMoveProviderBase>();
            teleportProvider = playerRig.GetComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider>();
        }
    }

    void Start()
    {
        interactable.selectEntered.AddListener(ToggleRide);

        if (path != null)
        {
            path.Spline.Evaluate(0.0f, out float3 localPos, out float3 localForward, out float3 localUp);
            transform.position = path.transform.TransformPoint(localPos);
            transform.rotation = Quaternion.LookRotation(path.transform.TransformDirection(localForward), path.transform.TransformDirection(localUp));

            // --- THIS IS THE FIX ---
            // Disable physics when parked at the start
            if (rb != null)
                rb.isKinematic = true;
            // --- END FIX ---
        }
    }

    void OnDestroy()
    {
        interactable.selectEntered.RemoveListener(ToggleRide);
    }

    private void ToggleRide(SelectEnterEventArgs args)
    {
        if (isMoving) return;

        // --- THIS IS THE FIX ---
        // Enable physics for the ride
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = false; // Make sure it doesn't fall off the spline
        }
        // --- END FIX ---

        if (playerCharacterController != null)
            playerCharacterController.enabled = false;
        if (moveProvider != null)
            moveProvider.enabled = false;
        if (teleportProvider != null)
            teleportProvider.enabled = false;

        playerRig.transform.SetParent(playerSeat);
        playerRig.transform.localPosition = Vector3.zero;
        playerRig.transform.localRotation = Quaternion.identity;

        float targetT = isAtStart ? 1.0f : 0.0f;
        StartCoroutine(MoveAlongPath(targetT));
    }

    private IEnumerator MoveAlongPath(float targetT)
    {
        isMoving = true;
        float currentT = isAtStart ? 0.0f : 1.0f;

        while (Mathf.Abs(targetT - currentT) > 0.001f)
        {
            float pathLength = path.Spline.GetLength();
            float step = (speed * Time.deltaTime) / pathLength;
            currentT = Mathf.MoveTowards(currentT, targetT, step);

            path.Spline.Evaluate(currentT, out float3 localPos, out float3 localForward, out float3 localUp);

            // --- THIS IS THE FIX ---
            // Move the car using the Rigidbody for smooth, physics-safe movement
            Vector3 newWorldPos = path.transform.TransformPoint(localPos);
            Quaternion newWorldRot = Quaternion.LookRotation(path.transform.TransformDirection(localForward), path.transform.TransformDirection(localUp));

            rb.MovePosition(newWorldPos);
            rb.MoveRotation(newWorldRot);
            // --- END FIX ---

            yield return null;
        }

        // --- Ride is Over ---

        // Snap to final position
        path.Spline.Evaluate(targetT, out float3 endLocalPos, out float3 endLocalFwd, out float3 endLocalUp);
        rb.MovePosition(path.transform.TransformPoint(endLocalPos));
        rb.MoveRotation(Quaternion.LookRotation(path.transform.TransformDirection(endLocalFwd), path.transform.TransformDirection(endLocalUp)));

        // --- THIS IS THE FIX ---
        // Disable physics now that we are parked at the end
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = true; // Reset gravity to its default
        }
        // --- END FIX ---

        playerRig.transform.SetParent(null);

        if (playerCharacterController != null)
            playerCharacterController.enabled = true;
        if (moveProvider != null)
            moveProvider.enabled = true;
        if (teleportProvider != null)
            teleportProvider.enabled = true;

        isAtStart = !isAtStart;
        isMoving = false;
    }
}