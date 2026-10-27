using UnityEngine;
using UnityEngine.InputSystem;

// This is just something I do that has one player script that handles switching between characters
// It also handles the input and passes that data around between characters and stuff
// It makes adding multiple characters a lot easier, just takes more time

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    [SerializeField]
    PlayerSettings m_PlayerSettings;
    [SerializeField]
    PlayerCharacter m_StartingPlayerCharacter;
    [SerializeField]
    PausePlayerCharacter m_PausePlayerCharacter;
    [SerializeField]
    PlayerCamera m_PlayerCamera;
    [SerializeField]
    Camera m_Camera;

    PlayerInput m_PlayerInput;

    PlayerCharacter m_PlayerCharacter;
    IPlayerCharacterUpdateData m_PlayerCharacterUpdateData;
    CameraUpdateData m_CameraUpdateData;
    CameraLateUpdateData m_CameraLateUpdateData;

    public PlayerCharacter CurrentPlayerCharacter => m_PlayerCharacter;

    void Awake()
    {
        m_PlayerInput = GetComponent<PlayerInput>();
        m_PlayerCamera.Init(new CameraInitData()
        {
            Camera = m_Camera,
            CameraSettings = m_PlayerSettings.CameraSettings
        });
        m_PausePlayerCharacter.Init(new PausePlayerCharacterInitData() { Player = this });
        ChangeCharacter(m_StartingPlayerCharacter);
        m_CameraUpdateData = new CameraUpdateData();
        m_CameraLateUpdateData = new CameraLateUpdateData();
    }

    void Update()
    {
        if (!m_PlayerCharacter || !m_PlayerCamera) return;
        SetCursorVisibility(m_PlayerCharacter.MouseVisible);
        m_PlayerCharacterUpdateData.DeltaTime = Time.deltaTime;
        m_CameraUpdateData.DeltaTime = Time.deltaTime;
        m_CameraUpdateData.DoCameraRotation = m_PlayerCharacter.DoCameraRotation;
        m_PlayerCamera.UpdateCamera(ref m_CameraUpdateData);
        m_PlayerCharacter.UpdateCharacter(ref m_PlayerCharacterUpdateData);
        m_CameraLateUpdateData.DeltaTime = Time.deltaTime;
        if (m_PlayerCharacterUpdateData is FirstPersonPlayerCharacterUpdateData firstPersonPlayerCharacterUpdateData)
        {
            m_CameraLateUpdateData.IsSprinting = firstPersonPlayerCharacterUpdateData.IsSprinting;
        }
        else m_PlayerCamera.ResetFOV();
    }

    void LateUpdate()
    {
        m_CameraLateUpdateData.Target = m_PlayerCharacter.CameraTarget;
        m_PlayerCamera.LateUpdateCamera(ref m_CameraLateUpdateData);
    }

    #region Change Character
    public void ChangeCharacter(PlayerCharacter playerCharacter)
    {
        if (!playerCharacter) return;
        m_PlayerCharacter = playerCharacter;
        if (!m_PlayerCharacter.HasBeenInitialized) m_PlayerCharacter.Init(PlayerCharacterInitData);
        m_PlayerCharacterUpdateData = PlayerCharacterUpdateData;
        if (!string.IsNullOrWhiteSpace(m_PlayerCharacter.ActionMap)) m_PlayerInput.SwitchCurrentActionMap(m_PlayerCharacter.ActionMap);
        m_PlayerCamera.ChangeCameraTarget(m_PlayerCharacter.CameraTarget);
        SetCursorVisibility(m_PlayerCharacter.MouseVisible);
    }

    IPlayerCharacterInitData PlayerCharacterInitData => m_PlayerCharacter switch
    {
        FirstPersonPlayerCharacter => new FirstPersonPlayerCharacterInitData()
        {
            Camera = m_Camera,
            CharacterSettings = m_PlayerSettings.CharacterSettings,
            Player = this,
            PausePlayerCharacter = m_PausePlayerCharacter
        },
        PausePlayerCharacter => new PausePlayerCharacterInitData() { Player = this },
        _ => null
    };

    IPlayerCharacterUpdateData PlayerCharacterUpdateData => m_PlayerCharacter switch
    {
        FirstPersonPlayerCharacter => new FirstPersonPlayerCharacterUpdateData(),
        PausePlayerCharacter => new PausePlayerCharacterUpdateData(),
        _ => null
    };
    #endregion Change Character

    #region Cursor Toggles
    public void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetCursorVisibility(bool bVisible)
    {
        if (bVisible) ShowCursor();
        else HideCursor();
    }
    #endregion Cursor Toggles

    #region Handle PlayerInput Events
    delegate void SetDataValueFunc<T>(T _) where T : class, IPlayerCharacterUpdateData;

    void SetDataValue<T>(SetDataValueFunc<T> dataChangeFunction) where T : class, IPlayerCharacterUpdateData
    {
        if (m_PlayerCharacterUpdateData is T t) dataChangeFunction(t);
    }

    public void HandlePauseInput(InputAction.CallbackContext _) => m_PlayerCharacter.OnPausePressed();

    public void HandleMoveInput(InputAction.CallbackContext ctx)
    {
        SetDataValue<FirstPersonPlayerCharacterUpdateData>(updateData => updateData.MovementInput = ctx.ReadValue<Vector2>());
    }

    public void HandleLookInput(InputAction.CallbackContext ctx)
    {
        if (!m_PlayerCharacter.DoCameraRotation) return;
        m_CameraUpdateData.LookInput = ctx.ReadValue<Vector2>();
    }

    public void HandleSprintInput(InputAction.CallbackContext ctx)
    {
        SetDataValue<FirstPersonPlayerCharacterUpdateData>(updateData => updateData.SprintPressedThisFrame = ctx.action.WasPressedThisFrame());
    }

    public void HandleJumpInput(InputAction.CallbackContext ctx)
    {
        SetDataValue<FirstPersonPlayerCharacterUpdateData>(updateData => updateData.JumpPressedThisFrame = ctx.action.WasPressedThisFrame());
    }

    public void HandleBreakInput(InputAction.CallbackContext ctx)
    {
        SetDataValue<FirstPersonPlayerCharacterUpdateData>(updateData => updateData.BreakPressedThisFrame = ctx.action.WasPressedThisFrame());
    }

    public void HandlePlaceInput(InputAction.CallbackContext ctx)
    {
        SetDataValue<FirstPersonPlayerCharacterUpdateData>(updateData => updateData.PlacePressedThisFrame = ctx.action.WasPressedThisFrame());
    }
    #endregion Handle PlayerInput Events
}