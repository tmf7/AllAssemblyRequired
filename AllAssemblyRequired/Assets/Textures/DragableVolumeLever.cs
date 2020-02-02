using UnityEngine;
using UnityEngine.EventSystems;

public class DragableVolumeLever : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public enum VolumeScale : int
    {
        SOUND_FX,
        MUSIC
    }

    private bool _isHeld = false;

    [SerializeField] private Transform _topStop;
    [SerializeField] private Transform _bottomStop;
    [SerializeField] private EmissionController[] _volumeCubes;
    [SerializeField] private VolumeScale _volumeScale = VolumeScale.SOUND_FX;
    [SerializeField] private AudioClip _testSoundFX;

    [SerializeField, Range(0.0f, 1.0f)] private float TEST = 1.0f;

    private float _replayCooldown;
    private float _leverVolume = 1.0f;

    private const float REPLAY_BUFFER = 0.5f;

    private float LeverVolume
    {
        get
        {
            return _leverVolume;
        }
        set
        {
            _leverVolume = Mathf.Clamp01(value);

            transform.position = Vector3.Lerp(_bottomStop.position, _topStop.position, _leverVolume);

            foreach (var cube in _volumeCubes)
            {
                cube.Brightness = _leverVolume;
            }

            switch (_volumeScale)
            {
                case VolumeScale.SOUND_FX:
                {
                    if (_replayCooldown <= 0.0f)
                    {
                        _replayCooldown = _testSoundFX.length + REPLAY_BUFFER;
                        SoundManager.Instance.PlaySoundFX(_testSoundFX, gameObject);
                    }

                    SoundManager.Instance.SoundFXVolume = _leverVolume;
                    break;
                }
                case VolumeScale.MUSIC:
                {
                    SoundManager.Instance.SoundFXVolume = _leverVolume;
                    break;
                }
            }
        }
    }

    private void Update()
    {
        LeverVolume = TEST;

        if (_replayCooldown > 0.0f)
        {
            _replayCooldown -= Time.unscaledDeltaTime;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isHeld = true;
        Debug.Log(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isHeld)
        {
            // use that same 
            Debug.Log(eventData.position);

            //LeverVolume = 1.0f;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isHeld = false;
        Debug.Log(eventData.position);
    }
}
