using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SceneLoadButton : MonoBehaviour
{
    [SerializeField]
    int m_SceneIndex = 0;
    [SerializeField]
    LoadingScreen m_LoadingScreen;

    void Awake() => GetComponent<Button>().onClick.AddListener(() =>
    {
        Time.timeScale = 1f;
        m_LoadingScreen.SceneIndexToLoad = m_SceneIndex;
        m_LoadingScreen.gameObject.SetActive(true);
    });
}
