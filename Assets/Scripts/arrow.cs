using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Arrow : MonoBehaviour
{
    public float lifeTime = 10f;
    private Rigidbody rb;
    private bool hasHit = false;

    void Awake() { rb = GetComponent<Rigidbody>(); }
    void Start() { Destroy(gameObject, lifeTime); }

    void OnCollisionEnter(Collision collision)
    {
        // 1. You are 100% correct. This line ignores the bow.
        if (collision.gameObject.CompareTag("yeah")) return;

        // 2. This line stops it from sticking to multiple things
        if (hasHit) return;

        // 3. This is the "stick" logic
        hasHit = true;
        rb.isKinematic = true; // Stop all physics

        // 4. --- THIS WAS THE BUG ---
        rb.linearVelocity = Vector3.zero; // Changed from 'linearVelocity'
                                          // --- END BUG FIX ---

        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        transform.SetParent(collision.transform); // Stick
    }

}