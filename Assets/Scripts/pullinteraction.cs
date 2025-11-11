using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(LineRenderer))]
public class String_Simple : MonoBehaviour
{
    public event Action Grabbed;
    public event Action Released;

    [Header("Main Bow (MUST ASSIGN)")]
    public XRGrabInteractable bowInteractable;

    [Header("String Visuals (MUST ASSIGN)")]
    public Transform stringStart;
    public Transform stringEnd;
    public Transform nockPoint;

    [Header("Held Positions (MUST ASSIGN)")]
    public Transform heldStart;
    public Transform heldEnd;
    public Transform heldNockPoint;

    private XRGrabInteractable grabInteractable;
    private LineRenderer lineRenderer;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 3;

        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);

        SetStringVisuals(false);
    }

    void Update()
    {
        // Continuously update string visuals to follow the bow
        SetStringVisuals(grabInteractable.isSelected);
    }

    void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (bowInteractable.isSelected)
        {
            Grabbed?.Invoke();
        }
        else
        {
            grabInteractable.interactionManager.SelectExit(args.interactorObject, grabInteractable);
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        Released?.Invoke();
    }

    private void SetStringVisuals(bool isHeld)
    {
        Transform start = isHeld ? heldStart : stringStart;
        Transform nock = isHeld ? heldNockPoint : nockPoint;
        Transform end = isHeld ? heldEnd : stringEnd;

        Transform lineTransform = transform;

        // Convert world points to this object's local space
        lineRenderer.SetPosition(0, lineTransform.InverseTransformPoint(start.position));
        lineRenderer.SetPosition(1, lineTransform.InverseTransformPoint(nock.position));
        lineRenderer.SetPosition(2, lineTransform.InverseTransformPoint(end.position));
    }
}