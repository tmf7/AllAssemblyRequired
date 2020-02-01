using UnityEngine;
using UnityEngine.SceneManagement;

public class StartBlock : ClickableBlock
{
    [SerializeField] private SceneIndex _sceneToLoad;

    public override void OnClick()
    {
        SceneManager.LoadScene((int)_sceneToLoad, LoadSceneMode.Single);
    }
}
