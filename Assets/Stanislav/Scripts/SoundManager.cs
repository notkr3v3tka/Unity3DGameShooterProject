using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioSource audioSource;

    public AudioClip buttonClickSound;
    public AudioClip enemyHitSound;
    public AudioClip laserShootSound;
    public AudioClip playerHitSound;
    public AudioClip cardSelectSound;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    public void PlayButtonClickSound()
    {
        PlaySound(buttonClickSound);
    }

    public void PlayEnemyHitSound()
    {
        audioSource.pitch = Random.Range(0.95f, 1.05f);
        PlaySound(enemyHitSound);
    }

    public void PlayLaserShootSound()
    {
        audioSource.pitch = Random.Range(0.95f, 1.05f);
        PlaySound(laserShootSound);
    }

    public void PlayPlayerHitSound()
    {
        audioSource.pitch = Random.Range(0.95f, 1.05f);
        PlaySound(playerHitSound);
    }

    public void PlayCardSelectSound()
    {
        audioSource.pitch = Random.Range(0.95f, 1.05f); 
        PlaySound(cardSelectSound);
    }
}