using System.Collections; // Required for Coroutines
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(LineRenderer))]
public class SimpleProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [Tooltip("The fixed power of the shot. Try 15 or 20.")]
    public float fixedLaunchForce = 15f;

    // --- THIS IS THE NEW VARIABLE ---
    [Tooltip("Time in seconds after firing before the projectile stops (set to 0 for no limit).")]
    public float projectileDuration = 5.0f;
    // ---------------------------------

    [Tooltip("The point the projectile fires from. If blank, it fires from this object's center.")]
    public Transform firePoint;

    [Header("Trajectory")]
    public int trajectoryLinePoints = 30;
    public float trajectoryTimeStep = 0.05f;

    private Rigidbody rb;
    private LineRenderer lineRenderer;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private bool isAiming = false;

    // --- NEW: To manage the timer ---
    private Coroutine stopTimerCoroutine = null;
    // --------------------------------

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        if (firePoint == null)
        {
            firePoint = transform;
        }

        grabInteractable.selectMode = UnityEngine.XR.Interaction.Toolkit.Interactables.InteractableSelectMode.Single;
        grabInteractable.throwOnDetach = false;

        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);

        lineRenderer.enabled = false;

    }

    void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    // When you grab the object
    private void OnGrab(SelectEnterEventArgs args)
    {
        // --- NEW: Stop any existing timers ---
        // This prevents the ball from stopping if you catch it mid-air
        if (stopTimerCoroutine != null)
        {
            StopCoroutine(stopTimerCoroutine);
            stopTimerCoroutine = null;
        }
        // --------------------------------------

        // Set the ball back to kinematic so it follows the hand
        rb.isKinematic = true;
        rb.useGravity = false;

        isAiming = true;
        lineRenderer.enabled = true;
    }

    // When you release the object
    private void OnRelease(SelectExitEventArgs args)
    {
        if (isAiming)
        {
            Vector3 launchVelocity = GetLaunchVelocity();

            // FIRE!
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = launchVelocity;

            // --- NEW: Start the stop timer ---
            if (projectileDuration > 0)
            {
                stopTimerCoroutine = StartCoroutine(StopAfterTime(projectileDuration));
            }
            // ---------------------------------
        }

        ResetAim();
    }

    void Update()
    {
        if (isAiming)
        {
            UpdateTrajectory();
        }

    }

    // --- THIS IS THE NEW FUNCTION ---
    private IEnumerator StopAfterTime(float duration)
    {
        // Wait for the specified number of seconds
        yield return new WaitForSeconds(duration);

        // Now, stop the projectile
   
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        stopTimerCoroutine = null;
    }
    // --------------------------------

    private Vector3 GetLaunchVelocity()
    {
        Vector3 aimDirection = firePoint.forward;
        Vector3 launchVelocity = aimDirection * fixedLaunchForce;
        return launchVelocity;
    }

    private void UpdateTrajectory()
    {
        Vector3 launchVelocity = GetLaunchVelocity();
        Vector3 startPos = firePoint.position;

        lineRenderer.positionCount = trajectoryLinePoints;
        for (int i = 0; i < trajectoryLinePoints; i++)
        {
            float t = i * trajectoryTimeStep;
            Vector3 point = startPos + (launchVelocity * t) + (0.5f * Physics.gravity * t * t);
            lineRenderer.SetPosition(i, point);
        }
    }

    private void ResetAim()
    {
        isAiming = false;
        lineRenderer.enabled = false;
        lineRenderer.positionCount = 0;
    }
}