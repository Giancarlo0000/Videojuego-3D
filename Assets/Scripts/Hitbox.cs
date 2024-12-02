using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [SerializeField] private Color GizmoColor = Color.yellow;
    private MonoBehaviour _owner;

    private void Start()
    {
        _owner = GetComponentInParent<PlayerMove>() as MonoBehaviour
                ?? GetComponentInParent<EnemyAI>() as MonoBehaviour;

        if (_owner == null)
        {
            Debug.LogError("No se encontró un componente compatible");
            enabled = false;
        }

        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_owner == null) return;

        if (_owner is PlayerMove player && other.CompareTag("Enemy"))
        {
            player.HandleHit(other);
        }
        else if (_owner is EnemyAI enemy && other.CompareTag("Player"))
        {
            enemy.HandleHit(other);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = GizmoColor;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}