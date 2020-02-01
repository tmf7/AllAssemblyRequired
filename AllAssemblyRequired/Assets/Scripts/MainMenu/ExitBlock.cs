using UnityEngine;

public class ExitBlock : ClickableBlock
{
    public override void OnClick()
    {
        Debug.Log("Quitting");
        Application.Quit();
    }
}
