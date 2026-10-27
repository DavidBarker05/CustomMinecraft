using UnityEngine;

public class PausePlayerCharacterInitData : IPlayerCharacterInitData
{
    public Player Player { get; set; }
}

public class PausePlayerCharacterUpdateData : IPlayerCharacterUpdateData
{
    public float DeltaTime { get; set; }
}

public class PausePlayerCharacter : PlayerCharacter
{
    [SerializeField]
    GameObject m_PauseScreen;

    public override bool HasBeenInitialized { get; protected set; }

    public override string ActionMap => "PausePlayer";
    public override bool MouseVisible => true;
    public override bool DoCameraRotation => false;

    public bool Paused { get; private set; } = false;

    Player m_Player;
    PlayerCharacter m_LastCharacter = null;

    public override void Init(IPlayerCharacterInitData playerCharacterInitData)
    {
        if (playerCharacterInitData is PausePlayerCharacterInitData initData)
        {
            m_Player = initData.Player;
            HasBeenInitialized = true;
        }
        else
        {
            Debug.LogError("playerCharacterInitData is not type PausePlayerCharacterInitData");
            return;
        }
    }

    public override void UpdateCharacter(ref IPlayerCharacterUpdateData playerCharacterUpdateData)
    {
        if (!HasBeenInitialized)
        {
            Debug.LogError("PausePlayerCharacter hasn't been initialized!");
            return;
        }
        if (playerCharacterUpdateData is not PausePlayerCharacterUpdateData)
        {
            Debug.LogError("playerCharacterUpdateData is not type PausePlayerCharacterUpdateData");
            return;
        }
    }

    public override void OnPausePressed() => ResumeGame();

    public void PauseGame(PlayerCharacter characterToSwitchBackTo)
    {
        if (!HasBeenInitialized)
        {
            Debug.LogError("PausePlayerCharacter hasn't been initialized!");
            return;
        }
        if (!m_PauseScreen)
        {
            Debug.LogWarning("Pause screen is missing");
            return;
        }
        if (Paused || Time.timeScale == 0f || characterToSwitchBackTo == null) return;
        Paused = true;
        m_LastCharacter = characterToSwitchBackTo;
        CameraTarget = m_LastCharacter.CameraTarget;
        m_Player.ChangeCharacter(this);
        m_PauseScreen.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    void ResumeGame()
    {
        if (!HasBeenInitialized)
        {
            Debug.LogError("PausePlayerCharacter hasn't been initialized!");
            return;
        }
        if (!m_PauseScreen)
        {
            Debug.LogWarning("Pause screen is missing");
            return;
        }
        if (!Paused || Time.timeScale == 1f || m_LastCharacter == null) return;
        Paused = false;
        m_Player.ChangeCharacter(m_LastCharacter);
        m_LastCharacter = null;
        CameraTarget = null;
        m_PauseScreen.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
}