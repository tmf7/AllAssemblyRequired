
using UnityEngine;

public class MenuSelectListener : MonoBehaviour
{
    [SerializeField] private GameObject _pauseMenuArea;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
        {
            _pauseMenuArea.SetActive(!_pauseMenuArea.activeSelf);
        }
    }
}
