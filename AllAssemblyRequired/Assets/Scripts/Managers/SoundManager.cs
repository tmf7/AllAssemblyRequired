using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioSource _musicAudioSource;
    [SerializeField] private AudioClip _mainMusic;
    [SerializeField] private AudioClip _menuMusic;

    private float _soundFXVolume = 1.0f;
    private float _musicVolume = 1.0f; 

    public float SoundFXVolume
    {
        get
        {
            return _soundFXVolume;
        }
        set
        {
            _soundFXVolume = Mathf.Clamp01(value);
        }
    }

    public float MusicVolume
    {
        get
        {
            return _musicAudioSource.volume;
        }
        set
        {
            _musicAudioSource.volume = Mathf.Clamp01(value);
        }
    }

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
        if (audioClip != null && whereToPlay != null)
        {
            AudioSource.PlayClipAtPoint(audioClip, whereToPlay.transform.position, _soundFXVolume);
        }
    }

    public void PlayMainMusic()
    {
        _musicAudioSource.clip = _mainMusic;
        _musicAudioSource.loop = true;
        _musicAudioSource.Play();
    }

    public void PlayMenuMusic()
    {
        _musicAudioSource.clip = _menuMusic;
        _musicAudioSource.loop = true;
        _musicAudioSource.Play();
    }

}
