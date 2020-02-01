using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioSource _musicAudioSource;
    [SerializeField] private AudioClip _mainMusic;

    private float _soundFXVolume = 1.0f;
    private float _musicVolume = 1.0f; 

    public bool IsPaused
    {
        get
        {
            return AudioListener.pause;
        }
        set
        {
            AudioListener.pause = value;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }
    }

    public void PlaySoundFX(AudioClip audioClip, GameObject whereToPlay)
    {
        AudioSource.PlayClipAtPoint(audioClip, whereToPlay.transform.position, _soundFXVolume);
    }

    public void PlayMusic()
    {
        _musicAudioSource.clip = _mainMusic;
        _musicAudioSource.loop = true;
        _musicAudioSource.Play();
    }

}
