using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource MusicSource = null;
    [SerializeField] private AudioClip[] Songs = null;
    private int _songIndex = -1;

    private void Start()
    {
        PlayRandomSong();
    }

    private void PlayRandomSong()
    {
        int randomIndex;

        do
        {
            randomIndex = Random.Range(0, Songs.Length);
        } while (randomIndex == _songIndex);

        _songIndex = randomIndex;
        MusicSource.clip = Songs[randomIndex];
        MusicSource.Play();

        Invoke("PlayNextSong", MusicSource.clip.length);
    }

    private void PlayNextSong()
    {
        PlayRandomSong();
    }

    public void StopMusic()
    {
        if (MusicSource.isPlaying)
        {
            MusicSource.Pause();
        }
    }
}
