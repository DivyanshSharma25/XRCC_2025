using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class Bow_Simple : MonoBehaviour
{
    [Header("Assets")]
    public GameObject arrowPrefab;
    public Transform nockPoint;

    [Header("Held Position (MUST ASSIGN)")]
    public Transform heldNockPoint;

    [Header("Scripts (MUST ASSIGN)")]
    public String_Simple stringInteractable;
    public TrajectoryRenderer trajectoryRenderer;

    [Header("Settings")]
    public float fixedLaunchForce = 3000f; // This is the default

    private XRGrabInteractable bowInteractable;
    private GameObject currentArrow = null;
    private bool isLoaded = false;
    private float currentArrowMass = 1.0f;

    void Awake()
    {
        bowInteractable = GetComponent<XRGrabInteractable>();

        if (stringInteractable == null)
            stringInteractable = GetComponentInChildren<String_Simple>();
        if (trajectoryRenderer == null)
            trajectoryRenderer = GetComponentInChildren<TrajectoryRenderer>();

        stringInteractable.Grabbed += OnStringGrabbed;
        stringInteractable.Released += OnStringReleased;
        bowInteractable.selectExited.AddListener(OnBowReleased);
    }

    void OnDestroy()
    {
        if (stringInteractable != null)
        {
            stringInteractable.Grabbed -= OnStringGrabbed;
            stringInteractable.Released -= OnStringReleased;
        }
        bowInteractable.selectExited.RemoveListener(OnBowReleased);
    }

    private void OnBowReleased(SelectExitEventArgs args)
    {
        if (isLoaded)
        {
            XRGrabInteractable stringGrabber = stringInteractable.GetComponent<XRGrabInteractable>();
            if (stringGrabber.firstInteractorSelecting != null)
            {
                stringGrabber.interactionManager.SelectExit(stringGrabber.firstInteractorSelecting, stringGrabber);
            }

            isLoaded = false;
            if (trajectoryRenderer != null)
                trajectoryRenderer.Hide();
            
        }
    }

    private void OnStringGrabbed()
    {
        if (bowInteractable.isSelected)
        {
            LoadArrow();
            isLoaded = true;
            if (trajectoryRenderer != null)
                trajectoryRenderer.Show();
        }
    }

    private void OnStringReleased()
    {
        if (isLoaded)
        {
            FireArrow();
        }
        isLoaded = false;
        if (trajectoryRenderer != null)
            trajectoryRenderer.Hide();
    }

    private void LoadArrow()
    {
        Vector3 spawnPosition = nockPoint.position-(nockPoint.forward*0.1f);
        Quaternion arrowRotation = nockPoint.rotation * Quaternion.Euler(0, 0, 0);
        currentArrow = Instantiate(arrowPrefab, spawnPosition, arrowRotation);

        currentArrow.transform.SetParent(nockPoint);

        Rigidbody rb = currentArrow.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        currentArrowMass = rb.mass;
    }

    private void FireArrow()
    {
        if (currentArrow != null)
        {
            currentArrow.transform.SetParent(null);
            Rigidbody rb = currentArrow.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;

            Vector3 launchVelocity = nockPoint.forward * fixedLaunchForce;
            rb.AddForce(launchVelocity);

            currentArrow = null;
        }
    }

    void Update()
    {
        if (isLoaded && currentArrow != null)
        {
            Vector3 aimDirection = nockPoint.forward;
            float launchSpeed = fixedLaunchForce;

            Vector3 velocity = aimDirection * (launchSpeed / currentArrowMass);

            if (trajectoryRenderer != null)
                trajectoryRenderer.Draw(heldNockPoint.position, velocity);
        }
    }
}