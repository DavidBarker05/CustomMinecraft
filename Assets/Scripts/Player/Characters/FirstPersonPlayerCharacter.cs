using UnityEngine;

public class FirstPersonPlayerCharacterInitData : IPlayerCharacterInitData
{
    public CharacterSettings CharacterSettings { get; set; }
    public Player Player { get; set; }
    public PausePlayerCharacter PausePlayerCharacter { get; set; }

    public Camera Camera { get; set; }
}

public class FirstPersonPlayerCharacterUpdateData : IPlayerCharacterUpdateData
{
    #region Input
    public float DeltaTime { get; set; }

    public Vector2 MovementInput { get; set; }
    public bool JumpPressedThisFrame { get; set; }
    public bool SprintPressedThisFrame { get; set; }

    public bool BreakPressedThisFrame { get; set; }
    public bool PlacePressedThisFrame { get; set; }
    #endregion Input

    #region Output
    public bool IsSprinting { get; set; }
    #endregion
}

[
    RequireComponent(
        typeof(CharacterController)
    ),
]
public class FirstPersonPlayerCharacter : PlayerCharacter
{
    static readonly float s_Epsilon = 0.05f;
    static readonly float s_SqrEpsilon = s_Epsilon * s_Epsilon;

    [SerializeField]
    BlockData m_BlockToPlace;

    Camera m_Camera;
    CharacterSettings m_CharacterSettings;
    CharacterController m_CC;

    bool m_bIsGrounded;
    bool m_bBumpedHead;

    Vector3 m_Velocity;
    bool m_bIsJumping;
    bool m_bIsFalling;

    float m_CurrentJumpBufferTimer;
    float m_CurrentCoyoteTimer;

    bool m_bIsSprinting;
    float m_MovementSpeed;

    public override bool HasBeenInitialized { get; protected set; }

    public override string ActionMap => "Player";
    public override bool MouseVisible => false;
    public override bool DoCameraRotation => true;

    bool m_bCurrentBreakBlockCoordSet = false;
    Vector3Int m_CurrentBreakBlockCoord;
    BlockData m_CurrentBreakBlockData;
    float m_CurrentBreakTime = 0f;

    BlockData m_CurrentPlaceBlock;
    float m_CurrentPlaceTime;

    PausePlayerCharacter m_PausePlayerCharacter;

    public override void Init(IPlayerCharacterInitData playerCharacterInitData)
    {
        if (playerCharacterInitData is FirstPersonPlayerCharacterInitData initData)
        {
            m_CC = GetComponent<CharacterController>();
            m_CharacterSettings = initData.CharacterSettings;
            m_PausePlayerCharacter = initData.PausePlayerCharacter;
            m_Camera = initData.Camera;
            m_CurrentPlaceTime = m_CharacterSettings.TimeBetweenPlacements;
            m_CurrentPlaceBlock = m_BlockToPlace;
            HasBeenInitialized = true;
        }
        else
        {
            Debug.LogError("playerCharacterInitData is not type FirstPersonPlayerCharacter");
            return;
        }
    }

    public override void UpdateCharacter(ref IPlayerCharacterUpdateData playerCharacterUpdateData)
    {
        if (!HasBeenInitialized)
        {
            Debug.LogError("FirstPersonPlayerCharacter hasn't been initialized!");
            return;
        }
        if (playerCharacterUpdateData is FirstPersonPlayerCharacterUpdateData updateData)
        {
            HandleMovement(ref updateData);
            if (m_bCurrentBreakBlockCoordSet && m_CurrentBreakBlockData)
            {
                if (m_CurrentBreakBlockData.Breakable)
                {
                    m_CurrentBreakTime += updateData.DeltaTime;
                    if (m_CurrentBreakTime >= m_CurrentBreakBlockData.BreakTime)
                    {
                        World.Instance.BreakBlock(m_CurrentBreakBlockCoord);
                        m_bCurrentBreakBlockCoordSet = false;
                        m_CurrentBreakTime = 0f;
                    }
                }
            }
            if (m_CurrentPlaceTime < m_CharacterSettings.TimeBetweenPlacements) m_CurrentPlaceTime += updateData.DeltaTime;
            if (updateData.BreakPressedThisFrame) HandleMining();
            else if (m_bCurrentBreakBlockCoordSet)
            {
                m_bCurrentBreakBlockCoordSet = false;
                m_CurrentBreakTime = 0f;
            }
            if (updateData.PlacePressedThisFrame) HandlePlacing();
        }
        else
        {
            Debug.LogError("playerCharacterUpdateData is not type PausePlayerCharacterUpdateData");
            return;
        }
    }

    public override void OnPausePressed()
    {
        if (!HasBeenInitialized)
        {
            Debug.LogError("FirstPersonPlayerCharacter hasn't been initialized!");
            return;
        }
        m_PausePlayerCharacter.PauseGame(this);
    }

    #region Movement
    void HandleMovement(ref FirstPersonPlayerCharacterUpdateData updateData)
    {
        UpdateRotation(m_Camera.transform.rotation);
        CollisionChecks();
        UpdateTimers(updateData.DeltaTime);
        UpdateMovementSpeed(updateData.SprintPressedThisFrame);
        UpdateHorizontalVelocity(updateData.MovementInput);
        JumpChecks(updateData.JumpPressedThisFrame);
        UpdateVerticalVelocity(updateData.DeltaTime);
        updateData.IsSprinting =
            m_bIsSprinting // Pressing sprint
            && (m_bIsGrounded || m_CharacterSettings.CanSprintInAir) // On ground or allowed to sprint in air
            && (updateData.MovementInput.x > s_Epsilon || updateData.MovementInput.y > s_Epsilon); // Pressing movement keys
        m_CC.Move(m_Velocity * updateData.DeltaTime);
    }

    void UpdateRotation(Quaternion rotation)
    {
        Vector3 forward = Vector3.ProjectOnPlane(rotation * Vector3.forward, Vector3.up).normalized;
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

    #region Collision Checks
    void GroundCheck() => m_bIsGrounded = m_CC.isGrounded;

    void BumpedHeadCheck() => m_bBumpedHead = m_CC.collisionFlags.HasFlag(CollisionFlags.Above);

    void CollisionChecks()
    {
        GroundCheck();
        BumpedHeadCheck();
    }
    #endregion

    void UpdateTimers(float deltaTime)
    {
        m_CurrentJumpBufferTimer -= deltaTime;
        if (!m_bIsGrounded) m_CurrentCoyoteTimer -= deltaTime;
        else m_CurrentCoyoteTimer = 0f;
    }

    void UpdateMovementSpeed(bool bSprintPressedThisFrame)
    {
        m_bIsSprinting = ((m_bIsGrounded && !m_bIsFalling) || m_CharacterSettings.CanSprintInAir) && bSprintPressedThisFrame;
        m_MovementSpeed = m_bIsSprinting ? m_CharacterSettings.SprintSpeed : m_CharacterSettings.MovementSpeed;
    }

    void UpdateHorizontalVelocity(Vector2 movementInput)
    {
        float xIn = movementInput.x;
        float zIn = movementInput.y;
        Vector3 hIn = Vector3.ClampMagnitude(xIn * transform.right + zIn * transform.forward, 1f);
        hIn = hIn.sqrMagnitude > s_SqrEpsilon ? hIn : Vector3.zero;
        m_Velocity.x = hIn.x * m_MovementSpeed;
        m_Velocity.z = hIn.z * m_MovementSpeed;
    }

    #region Jumping
    void JumpChecks(bool bJumpPressedThisFrame)
    {
        if (bJumpPressedThisFrame) m_CurrentJumpBufferTimer = m_CharacterSettings.JumpBuffer;
        if (m_CurrentJumpBufferTimer > 0f && !m_bIsJumping && (m_bIsGrounded || m_CurrentCoyoteTimer > 0f)) InitiateJump();
        if ((m_bIsJumping || m_bIsFalling) && m_bIsGrounded && m_Velocity.y <= 0f)
        {
            m_bIsJumping = false;
            m_bIsFalling = false;
            m_Velocity.y = 0f;
        }
    }

    void InitiateJump()
    {
        m_bIsJumping = true;
        m_CurrentJumpBufferTimer = 0f;
        m_Velocity.y = m_CharacterSettings.InitialJumpVelocity;
    }
    #endregion

    #region Update Vertical Velocity
    void UpdateVerticalVelocityWhileJumping(float deltaTime)
    {
        if (m_bBumpedHead && m_Velocity.y > 0) m_Velocity.y = 0f;
        m_Velocity.y -= m_CharacterSettings.PosGravity * deltaTime;
        m_bIsFalling = m_Velocity.y < 0f;
    }

    void UpdateVerticalVelocityWhileFalling(float deltaTime)
    {
        m_bIsFalling = true;
        m_Velocity.y -= m_CharacterSettings.PosGravity * deltaTime;
    }

    void UpdateVerticalVelocity(float deltaTime)
    {
        if (m_bIsJumping) UpdateVerticalVelocityWhileJumping(deltaTime);
        else if (!m_bIsGrounded) UpdateVerticalVelocityWhileFalling(deltaTime);
        else m_Velocity.y = -1f;
    }
    #endregion Update Vertical Velocity
    #endregion Movement

    #region Mining and Placing
    #region Mining
    void HandleMining()
    {
        if (Physics.Raycast(
            origin: m_Camera.transform.position,
            direction: m_Camera.transform.forward,
            hitInfo: out RaycastHit hitInfo,
            maxDistance: m_CharacterSettings.MiningDistance,
            layerMask: m_CharacterSettings.WorldLayer
        ))
        {
            // Go a little bit into the hit point to ensure the coord is for the block
            Vector3Int blockCoord = World.IntCoord(hitInfo.point + m_Camera.transform.forward * 0.01f);
            if (!m_bCurrentBreakBlockCoordSet || m_CurrentBreakBlockCoord != blockCoord)
            {
                m_bCurrentBreakBlockCoordSet = true;
                m_CurrentBreakBlockCoord = blockCoord;
                m_CurrentBreakTime = 0f;
                Chunk chunk = World.Instance.GetChunk(m_CurrentBreakBlockCoord);
                if (chunk) m_CurrentBreakBlockData = chunk.GetBlock(m_CurrentBreakBlockCoord);
            }
        }
        else
        {
            m_bCurrentBreakBlockCoordSet = false;
            m_CurrentBreakTime = 0f;
        }
    }
    #endregion Mining

    #region Placing
    void HandlePlacing()
    {
        if (m_CurrentPlaceTime < m_CharacterSettings.TimeBetweenPlacements) return;
        if (Physics.Raycast(
            origin: m_Camera.transform.position,
            direction: m_Camera.transform.forward,
            hitInfo: out RaycastHit hitInfo,
            maxDistance: m_CharacterSettings.MiningDistance,
            layerMask: m_CharacterSettings.WorldLayer
        ))
        {
            Vector3Int blockCoord = World.IntCoord(hitInfo.point - m_Camera.transform.forward * 0.01f);
            if (Physics.CheckBox(
                center: blockCoord,
                halfExtents: Vector3.one / 2f, // Half 1x1x1 cube
                orientation: Quaternion.identity,
                layerMask: 1 << gameObject.layer // gameObject.layer returns the layer not a LayerMask which is 1 << layer
            )) return; // If would place cube inside self don't
            World.Instance.PlaceBlock(blockCoord, m_CurrentPlaceBlock);
            m_CurrentPlaceTime = 0f;
        }
    }
    #endregion Placing
    #endregion Mining and Placing
}
