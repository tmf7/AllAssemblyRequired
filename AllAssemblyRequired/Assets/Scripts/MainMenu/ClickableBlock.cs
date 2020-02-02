using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
public class ClickableBlock : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private float _pushForce = 100.0f;
    [SerializeField] private float _waitDurtion = 1.0f;
    [SerializeField] private AudioClip[] _clickSounds;
    [SerializeField] private AudioClip[] _tumbleSounds;
    [SerializeField] private bool _applyExplosiveForce = true;


    private static float CLEAR_MENU_FORCE = 10000.0f;
    private static float CLEAR_MENU_RADIUS = 10000.0f;

    public virtual void OnClick() { }

    private Transform GetClosestCamera()
    {

        return null;
       // return FindObjectsOfType<Camera>()
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector3 pushDirection = (transform.position - Camera.main.transform.position).normalized;
        GetComponent<Rigidbody>().AddForce(_pushForce * pushDirection, ForceMode.Impulse);
        SoundManager.Instance.PlaySoundFX(_clickSounds[Random.Range(0, _clickSounds.Length)], Camera.main.gameObject);
        StartCoroutine(WaitForExplosion());
    }

    private IEnumerator WaitForExplosion()
    {
        yield return new WaitForSecondsRealtime(_waitDurtion);

        if (_applyExplosiveForce)
        {
            Vector3 explosionCenter = Vector3.zero;

            var allBoxes = FindObjectsOfType<ClickableBlock>().Select(go => go.GetComponent<Rigidbody>());

            foreach (var rigidbody in allBoxes)
            {
                explosionCenter += rigidbody.position;
            }

            explosionCenter /= allBoxes.Count();

            foreach (var rigidbody in allBoxes)
            {
                rigidbody.AddExplosionForce(CLEAR_MENU_FORCE, explosionCenter, CLEAR_MENU_RADIUS);
            }

            yield return new WaitForSecondsRealtime(_waitDurtion);
        }

        OnClick();
    }

    private void OnCollisionEnter(Collision collision)
    {
        SoundManager.Instance.PlaySoundFX(_tumbleSounds[Random.Range(0, _tumbleSounds.Length)], gameObject);
    }
}
