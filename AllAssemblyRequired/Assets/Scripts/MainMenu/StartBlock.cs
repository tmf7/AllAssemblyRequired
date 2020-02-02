using UnityEngine;
using UnityEngine.SceneManagement;

public class StartBlock : ClickableBlock
{
    [SerializeField] private SceneIndex _sceneToLoad;

    private void Start()
    {
        SoundManager.Instance.PlayMenuMusic();
    }

    public override void OnClick()
    {
        SoundManager.Instance.PlayMainMusic();
        SceneManager.LoadScene((int)_sceneToLoad, LoadSceneMode.Single);
    }
}
