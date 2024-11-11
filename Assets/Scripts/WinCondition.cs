using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinCondition : MonoBehaviour
{
    [SerializeField] private ParticleSystem ParticleSystem;

    private AudioSource _audioSource;
    private AudioManager _audioManager;
    private bool _hasWin = false;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioManager = FindObjectOfType<AudioManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !_hasWin)
        {
            ParticleSystem.Play();
            _hasWin = true;
            _audioManager.StopMusic();
            _audioSource.Play();
            DestroyEnemies();
            StartCoroutine(SfxResetSound());
        }
    }

    private void DestroyEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }
    }

    private IEnumerator SfxResetSound()
    {
        yield return new WaitForSeconds(_audioSource.clip.length);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
