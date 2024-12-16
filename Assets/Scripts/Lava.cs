using UnityEngine;

public class Lava : MonoBehaviour
{
    [SerializeField] private RectTransform Buttons = null;
    private bool _hasDefeated = false;
    private void OnTriggerEnter(Collider other)
    {
        if (!_hasDefeated)
        {
            if (other.CompareTag("Player") || other.CompareTag("PlayerRagdoll"))
            {
                PlayerMove playerMove = FindObjectOfType<PlayerMove>();
                playerMove.PlayerDefeated();
                Cursor.lockState = CursorLockMode.None;
                Buttons.gameObject.SetActive(true);
                _hasDefeated = true;
            }
        }   
    }
}