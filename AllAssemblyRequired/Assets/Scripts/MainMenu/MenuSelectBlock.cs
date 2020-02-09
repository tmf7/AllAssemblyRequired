using UnityEngine;

public class MenuSelectBlock : ClickableBlock
{
    [SerializeField] private Transform _menuSpawnPoint;
    [SerializeField] private GameObject _menuPrefabToSpawn;
    [SerializeField] private ParticleSystem _destroyParticles;
    [SerializeField] private AudioClip[] _explosionSounds;

    public override void OnClick()
    {
        DestroyOldMenu();
        Instantiate(_menuPrefabToSpawn, _menuSpawnPoint.position, _menuSpawnPoint.rotation);
    }

    private void DestroyOldMenu()
    {
        var allBoxes = FindObjectsOfType<ClickableBlock>();
        var oldMenuParent = allBoxes[0].transform.root;

        foreach (var box in allBoxes)
        {
            var newExplosion = Instantiate(_destroyParticles, box.transform.position, box.transform.rotation);
            newExplosion.transform.localScale = box.transform.lossyScale;
            SoundManager.Instance.Play2DSoundFX(_explosionSounds[Random.Range(0, _explosionSounds.Length)]);
            Destroy(box.gameObject);
        }

        Destroy(oldMenuParent.gameObject);
    }

}
