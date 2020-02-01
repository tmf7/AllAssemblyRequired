using UnityEngine;


public class SelfDestroy : MonoBehaviour
{
    [SerializeField] private float _delayBeforeDestroy = 3.0f;

    private void Start()
    {
        Destroy(gameObject, _delayBeforeDestroy);
    }
}
