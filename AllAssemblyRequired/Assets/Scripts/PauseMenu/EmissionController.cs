using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class EmissionController : MonoBehaviour
{
    [SerializeField] private AnimationCurve _emissionStrength;
    [SerializeField] private Color _emissiveColor = Color.green;

    public float Brightness
    {
        set
        {
            Renderer renderer = GetComponent<Renderer>();
            Material mat = renderer.material;

            float emissionStrength = Mathf.Clamp01(value);

            Color finalColor = _emissiveColor * Mathf.LinearToGammaSpace(emissionStrength) * _emissionStrength.Evaluate(emissionStrength);

            mat.SetColor("_EmissionColor", finalColor);
        }
    }
}
