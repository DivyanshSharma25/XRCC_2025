using UnityEngine;

public class DiscoBallController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("How fast the ball spins around its Y-axis")]
    public float spinSpeed = 50.0f;
    [Tooltip("How fast the ball moves up and down")]
    public float bobSpeed = 1.0f;
    [Tooltip("How high and low the ball goes from its starting point")]
    public float bobHeight = 0.5f;

    [Header("Child Lights")]
    [Tooltip("A list of colors for the lights to cycle through")]
    public Color[] lightColors;
    [Tooltip("How fast the lights fade from one color to the next")]
    public float colorTransitionSpeed = 0.5f;

    // Private variables
    private Light[] childLights;
    private Vector3 startPosition;
    private int currentColorIndex = 0;
    private float colorTimer = 0.0f;

    void Start()
    {
        // Find all Light components in all children
        childLights = GetComponentsInChildren<Light>();

        // Store the starting position for the bobbing calculation
        startPosition = transform.position;
    }

    void Update()
    {
        // 1. Spinning
        // Rotate the disco ball around its own "up" axis
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);

        // 2. Up and Down Movement
        // Use a Sine wave for a smooth bobbing motion
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // 3. Child Light Color Transition
        HandleLightColorTransition();
    }

    private void HandleLightColorTransition()
    {
        // Don't run if there are no lights or not enough colors to transition
        if (childLights == null || childLights.Length == 0 || lightColors == null || lightColors.Length < 2)
        {
            return;
        }

        // Increment the timer
        colorTimer += Time.deltaTime * colorTransitionSpeed;

        // Determine the color we are starting from and the one we are going to
        int nextColorIndex = (currentColorIndex + 1) % lightColors.Length;

        Color startColor = lightColors[currentColorIndex];
        Color endColor = lightColors[nextColorIndex];

        // Lerp (linearly interpolate) between the two colors
        Color newLightColor = Color.Lerp(startColor, endColor, colorTimer);

        // Apply the new color to all child lights
        foreach (Light light in childLights)
        {
            light.color = newLightColor;
        }

        // If the timer has reached 1, we have arrived at the target color
        if (colorTimer >= 1.0f)
        {
            colorTimer = 0.0f; // Reset timer
            currentColorIndex = nextColorIndex; // Move to the next color
        }
    }
}