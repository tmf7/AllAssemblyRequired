using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSelectBlock : ClickableBlock
{
    [SerializeField] private Transform _menuSpawnPoint;
    [SerializeField] private GameObject _menuPrefabToSpawn;
    [SerializeField] private ParticleSystem _destroyParticles;

    public override void OnClick()
    {
        DestroyOldMenu();
        Instantiate(_menuPrefabToSpawn, _menuSpawnPoint.position, _menuSpawnPoint.rotation);
    }

    private void DestroyOldMenu()
    {
        var allBoxes = FindObjectsOfType<ClickableBlock>();

        foreach (var box in allBoxes)
        {
            var newExplosion = Instantiate(_destroyParticles, box.transform.position, box.transform.rotation);
            newExplosion.transform.localScale = box.transform.lossyScale;
            Destroy(box.gameObject);
        }
    }

}
