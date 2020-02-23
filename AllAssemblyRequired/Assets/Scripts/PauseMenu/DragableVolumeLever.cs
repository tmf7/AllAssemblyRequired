using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class DragableVolumeLever : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public enum VolumeScale : int
    {
        SOUND_FX,
        MUSIC
    }

    [SerializeField] private Camera _eventCamera;
    [SerializeField] private Transform _topStop;
    [SerializeField] private Transform _bottomStop;
    [SerializeField] private EmissionController[] _volumeCubes;
    [SerializeField] private VolumeScale _volumeScale = VolumeScale.SOUND_FX;
    [SerializeField] private AudioClip _testSoundFX;
    [SerializeField] private TextMeshProUGUI _volumeText;
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private GameObject _selectionHighlight;

    private RectTransform _rectTransform;
    private float _maxTranslationValue;
    private float _replayCooldown;
    private float _leverVolume = 1.0f;
    private bool _clicked;
    private bool _hovered;

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
                    if (_clicked && _replayCooldown <= 0.0f)
                    {
                        _replayCooldown = _testSoundFX.length + REPLAY_BUFFER;
                        SoundManager.Instance.Play2DSoundFX(_testSoundFX);
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

    private void Start()
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

        _rectTransform = GetComponentInChildren<RectTransform>();
        _maxTranslationValue = Vector3.Distance(_bottomStop.position, _topStop.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 dragPosition = GetRectTransformWorldPoint(eventData.position);
        CalculateTranslation(dragPosition);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _clicked = true;
        OnDrag(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {

        _hovered = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _clicked = false;

    }

    private void Update()
    {
        if (_clicked && _replayCooldown > 0.0f)
        {
            _replayCooldown -= Time.unscaledDeltaTime;
        }

        _selectionHighlight.SetActive(_clicked || _hovered);
    }

    private Vector3 GetRectTransformWorldPoint(Vector2 screenPoint)
    {
        Vector3 worldPointInRectTransform;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(_rectTransform, screenPoint, _eventCamera, out worldPointInRectTransform);

        return worldPointInRectTransform;
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
