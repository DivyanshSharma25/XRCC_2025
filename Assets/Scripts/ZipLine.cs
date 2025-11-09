using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

[RequireComponent(typeof(BoxCollider))]
public class ZipLine : MonoBehaviour
{
    [Header("Input")]
    [SerializeField]
    private InputActionReference interactAction;
    [Header("Zipline Settings")]
    [Tooltip("The other end point of the zipline")]
    public Transform otherEnd;

    [Tooltip("Speed of zipline movement")]
    public float ziplineSpeed = 5f;

    [Tooltip("Whether this is the start or end point")]
    public bool isStartPoint = true;

    [Tooltip("Offset from the zipline path (e.g., how far below the line the player hangs)")]
    public Vector3 playerOffset = new Vector3(0, -1f, 0);

    private Vector3 startPoint;
    private Vector3 endPoint;

    [Header("Interaction Settings")]
    [Tooltip("Size of the trigger area")]
    public Vector3 triggerSize = new Vector3(1f, 1f, 1f);

    [Tooltip("Which layers can trigger the zipline")]
    public LayerMask playerLayer;

    private bool playerInTrigger = false;
    private GameObject currentPlayer;
    private bool isZiplining = false;
    private Vector3 targetPosition;
    private PhotonView playerPhotonView;

    private void OnEnable()
    {
        if (interactAction != null && interactAction.action != null)
        {
            interactAction.action.performed += OnInteractPerformed;
        }
    }

    private void OnDisable()
    {
        if (interactAction != null && interactAction.action != null)
        {
            interactAction.action.performed -= OnInteractPerformed;
        }
    }

    private void Start()
    {
        // Configure the trigger collider
        var triggerArea = GetComponent<BoxCollider>();
        triggerArea.isTrigger = true;
        triggerArea.size = triggerSize;
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (playerInTrigger && !isZiplining)
        {
            StartZiplining();
        }
    }

    private void Update()
    {

        // Handle zipline movement
        if (isZiplining && currentPlayer != null)
        {
            // Calculate progress along the zipline
            Vector3 ziplineDirection = (endPoint - startPoint).normalized;
            float step = ziplineSpeed * Time.deltaTime;

            // Current position on the line before offset
            Vector3 currentPos = currentPlayer.transform.position - playerOffset;

            // Calculate next position along the line
            Vector3 nextPos = Vector3.MoveTowards(currentPos, endPoint, step);

            // Apply position with offset
            currentPlayer.transform.position = nextPos + playerOffset;

            // Check if we've reached the target
            if (Vector3.Distance(nextPos, endPoint) < 0.01f)
            {
                StopZiplining();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if it's a player
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            playerInTrigger = true;
            currentPlayer = other.gameObject;
            playerPhotonView = currentPlayer.GetComponent<PhotonView>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == currentPlayer)
        {
            playerInTrigger = false;
            currentPlayer = null;
            playerPhotonView = null;
        }
    }

    public void StartZiplining()
    {
        if (!playerInTrigger || currentPlayer == null || isZiplining) return;

        // Only allow if we're the owner of the player
        if (playerPhotonView != null && !playerPhotonView.IsMine) return;

        isZiplining = true;

        // Define the raw endpoints of the cable (without offset)
        Vector3 A = transform.position;
        Vector3 B = otherEnd != null ? otherEnd.position : transform.position;

        // Use the player's current position (compensated by offset) to find the nearest point on the line
        Vector3 playerPosOnLine = ClosestPointOnSegment(A, B, currentPlayer.transform.position - playerOffset);

        // Decide direction based on which endpoint is closer to the player projection
        float distToA = Vector3.Distance(playerPosOnLine, A);
        float distToB = Vector3.Distance(playerPosOnLine, B);

        if (distToA <= distToB)
        {
            startPoint = playerPosOnLine;
            endPoint = B;
        }
        else
        {
            startPoint = playerPosOnLine;
            endPoint = A;
        }

        // Set initial target (end point without offset, offset applied during movement)
        targetPosition = endPoint + playerOffset;

        // Optionally disable player controls while ziplining
        DisablePlayerControls();
    }

    public void StopZiplining()
    {
        if (!isZiplining) return;

        isZiplining = false;
        // Snap the player to the exact end point + offset to avoid tiny position errors
        if (currentPlayer != null)
        {
            currentPlayer.transform.position = endPoint + playerOffset;
        }

        EnablePlayerControls();
    }

    /// <summary>
    /// Returns the closest point on the segment AB to point P.
    /// </summary>
    private Vector3 ClosestPointOnSegment(Vector3 A, Vector3 B, Vector3 P)
    {
        Vector3 AB = B - A;
        float ab2 = Vector3.Dot(AB, AB);
        if (ab2 == 0f) return A; // A and B are the same point

        float t = Vector3.Dot(P - A, AB) / ab2;
        t = Mathf.Clamp01(t);
        return A + AB * t;
    }

    private void DisablePlayerControls()
    {
        // Get the character controller or movement component and disable it
        var movement = currentPlayer.GetComponent<CharacterController>();
        if (movement != null)
            movement.enabled = false;
    }

    private void EnablePlayerControls()
    {
        if (currentPlayer == null) return;

        // Re-enable character controller or movement
        var movement = currentPlayer.GetComponent<CharacterController>();
        if (movement != null)
            movement.enabled = true;
    }

    private void OnDrawGizmos()
    {
        // Draw the zipline path
        if (otherEnd != null)
        {
            // Draw main zipline path
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, otherEnd.position);

            // Draw offset path to show where player will actually move
            Gizmos.color = Color.cyan;
            Vector3 startWithOffset = transform.position + playerOffset;
            Vector3 endWithOffset = otherEnd.position + playerOffset;
            Gizmos.DrawLine(startWithOffset, endWithOffset);

            // Draw vertical offset lines
            Gizmos.color = Color.grey;
            Gizmos.DrawLine(transform.position, startWithOffset);
            Gizmos.DrawLine(otherEnd.position, endWithOffset);

            // Draw trigger areas
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawCube(transform.position, triggerSize);
        }
    }
}
