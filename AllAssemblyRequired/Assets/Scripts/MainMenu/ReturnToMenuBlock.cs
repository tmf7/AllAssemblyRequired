using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMenuBlock : ClickableBlock
{
    [SerializeField] private ParticleSystem _destroyParticles;
    [SerializeField] private AudioClip[] _explosionSounds;
    [SerializeField] private SceneIndex _sceneToLoad = SceneIndex.MAIN_MENU;

    public override void OnClick()
    {
        DestroyOldMenu();
        SoundManager.Instance.PlayMenuMusic();
        SceneManager.LoadScene((int)_sceneToLoad, LoadSceneMode.Single);
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
