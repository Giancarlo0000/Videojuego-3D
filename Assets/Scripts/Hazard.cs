using UnityEngine;

public class Hazard : MonoBehaviour
{
    [SerializeField] private float ParalysisTimePerHit = 1f;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerMove>().KnockedOut(ParalysisTimePerHit);
        }
    }
}
