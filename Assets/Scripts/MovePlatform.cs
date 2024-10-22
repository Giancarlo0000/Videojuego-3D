using UnityEngine;

public class MovePlatform : MonoBehaviour
{
    [SerializeField] private float speed = 2.0f;
    [SerializeField] private bool moveHorizontally = true;
    [SerializeField] private float distance = 5f;

    private Vector3 startPoint;
    private Vector3 endPoint;
    private float time;

    void Start()
    {
        CalculatePoints();
    }

    void Update()
    {
        time += Time.deltaTime * speed;
        float movement = Mathf.PingPong(time, 1);

        transform.position = Vector3.Lerp(startPoint, endPoint, movement);
    }

    void OnDrawGizmos()
    {
        Vector3 startPointGizmo = transform.position;
        Vector3 endPointGizmo = moveHorizontally ? new Vector3(startPointGizmo.x + distance, startPointGizmo.y, startPointGizmo.z) : new Vector3(startPointGizmo.x, startPointGizmo.y + distance, startPointGizmo.z);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(startPointGizmo, endPointGizmo);

        Gizmos.DrawCube(startPointGizmo, new Vector3(0.2f, 0.2f, 0.2f));
        Gizmos.DrawCube(endPointGizmo, new Vector3(0.2f, 0.2f, 0.2f));
    }

    void CalculatePoints()
    {
        startPoint = transform.position;
        endPoint = moveHorizontally ? new Vector3(startPoint.x + distance, startPoint.y, startPoint.z) : new Vector3(startPoint.x, startPoint.y + distance, startPoint.z);
    }
}
