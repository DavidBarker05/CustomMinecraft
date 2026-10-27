using UnityEngine;

[CreateAssetMenu(fileName = "CameraSettings", menuName = "Player/Camera Settings")]
public class CameraSettings : ScriptableObject
{
    [field: Header("Sensitivity")]
    [field: SerializeField, Min(0f)]
    public float HorizontalSensitivity { get; private set; } = 0.2f;
    [field: SerializeField, Min(0f)]
    public float VerticalSensitivity { get; private set; } = 0.225f;

    [field: Header("Camera Angle")]
    [field: SerializeField, Range(-90f, 0f)]
    public float MinVerticalAngle { get; private set; } = -80f;
    [field: SerializeField, Range(0f, 90f)]
    public float MaxVerticalAngle { get; private set; } = 80f;

    [field: Header("Field of View")]
    [field: SerializeField, Range(0f, 180f)]
    public float VerticalFieldOfView { get; private set; } = 60f;
    [field: SerializeField, Range(0f, 180f)]
    public float SprintVerticalFieldOfView { get; private set; } = 70f;
    [field: SerializeField, Min(0f)]
    public float FieldOfViewTransitionDuration { get; private set; } = 0.15f;
}
