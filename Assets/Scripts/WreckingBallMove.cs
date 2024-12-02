using UnityEngine;

public class WreckingBallMove : MonoBehaviour
{
    [SerializeField] private float Speed = 1.0f;
    [SerializeField] private float Angle = 45.0f;

    private float _time;

    void Update()
    {
        _time += Time.deltaTime;
        float rotation = Mathf.Sin(_time * Speed) * Angle;
        transform.localRotation = Quaternion.Euler(0, 0, rotation);
    }
}
