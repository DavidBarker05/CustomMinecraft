using System.Collections.Generic;
using UnityEngine;

public class MenuHandler : MonoBehaviour
{
    [SerializeField]
    GameObject m_MainScreen;

    readonly HashSet<GameObject> m_AdditionalScreens = new HashSet<GameObject>();
    GameObject m_CurrentScreen;

    void OnEnable() => m_CurrentScreen = m_MainScreen;

    public void OpenScreen(GameObject screen)
    {
        if (!screen || m_CurrentScreen != m_MainScreen) return;
        m_AdditionalScreens.Add(screen);
        m_CurrentScreen = screen;
        m_MainScreen.SetActive(false);
        screen.SetActive(true);
    }

    public void CloseScreen(GameObject screen)
    {
        if (!screen || m_CurrentScreen != screen) return;
        m_CurrentScreen = m_MainScreen;
        screen.SetActive(false);
        m_MainScreen.SetActive(true);
    }

    public void Close()
    {
        if (m_CurrentScreen != m_MainScreen) CloseScreen(m_CurrentScreen);
    }
}
