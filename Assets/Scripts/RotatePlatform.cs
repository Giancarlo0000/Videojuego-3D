using UnityEngine;

public class RotatePlatform : MonoBehaviour
{
    [SerializeField] private float Speed = 100f;

    private void Update()
    {
        transform.Rotate(Vector3.up * Speed * Time.deltaTime);
    }
}
