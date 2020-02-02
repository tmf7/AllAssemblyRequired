using UnityEngine;
using TMPro;

public class DragableVolumeLever : MonoBehaviour
{
    public enum VolumeScale : int
    {
        SOUND_FX,
        MUSIC
    }

    private bool _leverHit = false;

    [SerializeField] private Camera _eventCamera;
    [SerializeField] private Transform _topStop;
    [SerializeField] private Transform _bottomStop;
    [SerializeField] private EmissionController[] _volumeCubes;
    [SerializeField] private VolumeScale _volumeScale = VolumeScale.SOUND_FX;
    [SerializeField] private AudioClip _testSoundFX;
    [SerializeField] private TextMeshProUGUI _volumeText;
    [SerializeField] private LayerMask _targetLayer;

    [SerializeField, Range(0.0f, 1.0f)] private float TEST = 1.0f;

    private float _maxTranslationValue;
    private float _replayCooldown;
    private float _leverVolume = 1.0f;
    private bool _clicked;

    private const float REPLAY_BUFFER = 0.5f;

    private Vector3 InterpolationVector => (_topStop.position - _bottomStop.position).normalized;

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
                        SoundManager.Instance.PlaySoundFX(_testSoundFX, Camera.main.gameObject);
                    }

                    SoundManager.Instance.SoundFXVolume = _leverVolume;
                    break;
                }
                case VolumeScale.MUSIC:
                {
                    SoundManager.Instance.MusicVolume = _leverVolume;
                    break;
                }
            }
        }
    }

    private void Awake()
    {
        switch (_volumeScale)
        {
            case VolumeScale.SOUND_FX:
                {
                    _volumeText.text = "FX";
                    LeverVolume = SoundManager.Instance.SoundFXVolume;
                    break;
                }
            case VolumeScale.MUSIC:
                {
                    _volumeText.text = "Music";
                    LeverVolume = SoundManager.Instance.MusicVolume;
                    break;
                }
        }

        _maxTranslationValue = Vector3.Distance(_bottomStop.position, _topStop.position);
    }

    private void Update()
    {
        if (_replayCooldown > 0.0f)
        {
            _replayCooldown -= Time.unscaledDeltaTime;
        }

        _clicked = Input.GetMouseButton(0);

        if (Input.GetMouseButtonUp(0))
        {
            _leverHit = false;
        }

        if (_clicked)
        {
            var hits = Physics.RaycastAll(_eventCamera.ScreenPointToRay(Input.mousePosition), _targetLayer.value);

            foreach (var hit in hits)
            {
                var lever = hit.collider.GetComponent<DragableVolumeLever>();
                if (lever != null && lever._volumeScale == _volumeScale)
                {
                    Debug.Log(lever.name);
                    _leverHit = true;
                    break;
                }
            }

            if (_leverHit)
            {
                Vector3 mousePosition = Input.mousePosition + _eventCamera.transform.forward * ((transform.position - _eventCamera.transform.position).magnitude);
                CalculateTranslation(_eventCamera.ScreenToWorldPoint(mousePosition));
            }
        }
    }

    private void CalculateTranslation(Vector3 queryPoint)
    {
        Vector3 closestPoint = GetClosestPointOnLineSegment(queryPoint);
        float firstPositionTranslationDelta = Vector3.Distance(_bottomStop.position, closestPoint);

        LeverVolume = Mathf.Clamp01(firstPositionTranslationDelta / _maxTranslationValue);
    }

    private Vector3 GetClosestPointOnLineSegment(Vector3 queryPoint)
    {
        Vector3 firstPositionToQueryPoint = queryPoint - _bottomStop.position;
        float closestPointDistance = Vector3.Dot(firstPositionToQueryPoint, InterpolationVector);
        closestPointDistance = Mathf.Clamp(closestPointDistance, 0.0f, _maxTranslationValue);

        return _bottomStop.position + InterpolationVector * closestPointDistance;
    }
}
