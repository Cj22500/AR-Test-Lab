using UnityEngine;

public class BGMManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip musicClip;
    public bool playOnStart = true;
    public bool loop = true;

    void Start()
    {
        // If you didn't drag an AudioSource in, we'll try to find one on this object
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (musicClip != null)
        {
            PlayMusic(musicClip);
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.playOnAwake = false;
        audioSource.Play();
    }
}
