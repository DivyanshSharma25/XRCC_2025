using UnityEngine;
using TMPro; // Required for TextMeshPro

[RequireComponent(typeof(Collider))]
public class ScoreManager : MonoBehaviour
{
    [Header("Scoring")]
    [Tooltip("The UI Text element to display the score")]
    public TextMeshProUGUI scoreText;

    [Tooltip("The tag assigned to your basketball prefab")]
    public string ballTag = "Basketball";

    private int score = 0;

    void Start()
    {
        // Make sure the collider is a trigger
        GetComponent<Collider>().isTrigger = true;

        // Initialize the score text
        UpdateScoreText();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object is the ball
        if (!other.CompareTag(ballTag))
        {
            return;
        }

        // Try to get the ball's Rigidbody
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null)
        {
            return;
        }

        // --- THIS IS THE UPDATED CONDITION ---
        // 1. Only check if the Y-velocity is negative (coming down)
        if (rb.linearVelocity.y < 0)
        {
            // All checks passed. Score!
            RegisterScore();
        }
        // --- END UPDATE ---
    }

    private void RegisterScore()
    {
        // Update the score
        score++;
        UpdateScoreText();

        // (Optional: Add a sound effect here)
        // audioSource.PlayOneShot(swooshSound);
    }

    private void UpdateScoreText()
    {
        
            Debug.Log("score: " + score);
        
    }
}