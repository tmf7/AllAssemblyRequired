using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody))]
public class ClickableBlock : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private float _pushForce = 100.0f;
    [SerializeField] private float _waitDurtion = 1.0f;

    private static float CLEAR_MENU_FORCE = 10000.0f;
    private static float CLEAR_MENU_RADIUS = 10000.0f;

    public virtual void OnClick() { }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector3 pushDirection = (transform.position - Camera.main.transform.position).normalized;
        GetComponent<Rigidbody>().AddForce(_pushForce * pushDirection, ForceMode.Impulse);
        StartCoroutine(WaitForExplosion());
    }

    private IEnumerator WaitForExplosion()
    {
        yield return new WaitForSecondsRealtime(_waitDurtion);

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

        OnClick();
    }
}
