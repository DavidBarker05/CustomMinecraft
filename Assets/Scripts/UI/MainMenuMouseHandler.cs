using UnityEngine;

public class MainMenuMouseHandler : MonoBehaviour
{
    public static MainMenuMouseHandler Instance { get; private set; }

    void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        else
        {
            Instance = this;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }
}
