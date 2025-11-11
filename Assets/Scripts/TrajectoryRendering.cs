using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryRenderer : MonoBehaviour
{
    public int linePoints = 30;
    public float timeStep = 0.05f;

    private LineRenderer _lineRenderer;

    void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 0; // Start with no line
    }

    public void Show()
    {
        _lineRenderer.enabled = true;
    }

    public void Hide()
    {
        _lineRenderer.enabled = false;
        _lineRenderer.positionCount = 0; // Clear the line
    }

    // Bow.cs will call this to draw the arc
    public void Draw(Vector3 startPos, Vector3 velocity)
    {
        if (!_lineRenderer.enabled) return;

        _lineRenderer.positionCount = linePoints;

        for (int i = 0; i < linePoints; i++)
        {
            float t = i * timeStep;
            // Physics formula: s = ut + 0.5at^2
            Vector3 point = startPos + (velocity * t) + (0.5f * Physics.gravity * t * t);
            _lineRenderer.SetPosition(i, point);
        }
    }
}