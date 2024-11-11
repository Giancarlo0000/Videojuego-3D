using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    [SerializeField] private Color GizmoColor = Color.red;
    private EnemyAI _enemyAi;

    private void Start()
    {
        _enemyAi = GetComponentInParent<EnemyAI>();
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _enemyAi.HandleHit(other);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = GizmoColor;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
