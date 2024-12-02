using System.Collections;
using UnityEngine;

public class WreckingBall : MonoBehaviour
{
    [SerializeField] private float PushForce = 10.0f;
    [SerializeField] private float PushDuration = 2f;
    [SerializeField] private float ParalysisTimePerHit = 3f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            PlayerMove playerMove = collision.gameObject.GetComponent<PlayerMove>();

            if (playerRb != null)
            {
                Vector3 pushDirection = collision.transform.position - transform.position;
                pushDirection.y = 0;

                StartCoroutine(PushPlayer(playerRb, pushDirection, playerMove));
            }
        }
    }

    private IEnumerator PushPlayer(Rigidbody playerRb, Vector3 pushDirection, PlayerMove playerMove)
    {
        playerMove.KnockedOut(ParalysisTimePerHit);
        for (float t = 0; t < PushDuration; t += Time.deltaTime)
        {
            playerRb.AddForce(pushDirection * (PushForce * Time.deltaTime / PushDuration), ForceMode.Impulse);
            yield return null;
        }
    }
}
