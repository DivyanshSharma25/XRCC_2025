using UnityEngine;

public class hoop : MonoBehaviour
{
    int score=0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("basketball"))
        {
            Debug.Log("Score!");
            score++;
            Debug.Log(score);
            other.gameObject.transform.position = new Vector3(4.47f, 2f, -8.47f);
            other.gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        }
    }
}
