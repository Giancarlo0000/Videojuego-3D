using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonChangeScene : MonoBehaviour
{
    private Button _button = null;
    [SerializeField] private string SceneName = string.Empty;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(ChangeScene);
    }
    private void ChangeScene() => SceneManager.LoadScene(SceneName);
}