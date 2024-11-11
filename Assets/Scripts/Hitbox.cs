using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [SerializeField] private Color GizmoColor = Color.yellow;
    private PlayerMove _player = null;

    private void Start()
    {
        _player = GetComponentInParent<PlayerMove>();
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            _player.HandleHit(other);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = GizmoColor;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
