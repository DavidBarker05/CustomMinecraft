using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSettings", menuName = "Player/Character Settings")]
public class CharacterSettings : ScriptableObject
{
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

    public float Gravity { get; private set; }
    public float PosGravity { get; private set; }
    public float InitialJumpVelocity { get; private set; }

    void OnValidate() => CalculateValues();

    void OnEnable() => CalculateValues();

    void CalculateValues()
    {
        Gravity = (-2f * JumpHeight) / (TimeToReachJumpHeight * TimeToReachJumpHeight);
        PosGravity = Mathf.Abs(Gravity);
        InitialJumpVelocity = PosGravity * TimeToReachJumpHeight;
    }
}