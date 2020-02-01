#if UNITY_EDITOR
using UnityEngine;
using TMPro;

[ExecuteInEditMode]
public class SetBlockGroupProperties : MonoBehaviour
{
    [SerializeField] private string _masterText;
    [SerializeField] private float _masterFontSize = 12.0f;
    [SerializeField] private PhysicMaterial _masterPhysicsMaterial;
    [SerializeField] private float _masterMass = 10.0f;
    [SerializeField] private float _masterAngularDrag = 10.0f;

    private void Update()
    {
        foreach (var textField in GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            textField.text = _masterText;
            textField.fontSize = _masterFontSize;
        }

        foreach (var collider in GetComponentsInChildren<Collider>(true))
        {
            collider.material = _masterPhysicsMaterial;
        }

        foreach (var rigidbody in GetComponentsInChildren<Rigidbody>(true))
        {
            rigidbody.mass = _masterMass;
            rigidbody.angularDrag = _masterAngularDrag;
        }
    }
}
#endif