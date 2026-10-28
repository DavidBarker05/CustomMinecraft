using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSettings", menuName = "Player/Character Settings")]
public class CharacterSettings : ScriptableObject
{
    [field: Header("Movement")]
    [field: SerializeField]
    public bool CanSprintInAir { get; private set; } = true;
    [field: SerializeField, Min(1f)]
    public float MovementSpeed { get; private set; } = 4.5f;
    [field: SerializeField, Min(1f)]
    public float SprintSpeed { get; private set; } = 10f;
    [field: SerializeField, Min(0f)]
    public float JumpHeight { get; private set; } = 1f;
    [field: SerializeField, Min(0f)]
    public float TimeToReachJumpHeight { get; private set; } = 0.25f;
    [field: SerializeField, Range(0f, 1f)]
    public float JumpBuffer { get; private set; } = 0.125f;
    [field: SerializeField, Range(0f, 1f)]
    public float CoyoteTime { get; private set; } = 0.1f;

    [field: Header("Mining and Placing")]
    [field: SerializeField]
    public LayerMask WorldLayer { get; private set; }
    [field: SerializeField, Min(0.001f)]
    public float MiningDistance { get; private set; } = 7f;
    [field: SerializeField, Min(1f)]
    public float PlacingDistance { get; private set; } = 5f;
    [field: SerializeField, Min(0.001f)]
    public float TimeBetweenPlacements { get; private set; } = 0.25f;

    public float Gravity { get; private set; }
    public float PosGravity { get; private set; }
    public float InitialJumpVelocity { get; private set; }

    void OnValidate() => CalculateValues();

    void OnEnable() => CalculateValues();

    // Calculate the gravity that should be applied to the player based on how high we want the player to jump
    // and how long we want the player to reach that jump height
    void CalculateValues()
    {
        Gravity = (-2f * JumpHeight) / (TimeToReachJumpHeight * TimeToReachJumpHeight);
        PosGravity = Mathf.Abs(Gravity);
        InitialJumpVelocity = PosGravity * TimeToReachJumpHeight;
    }
}