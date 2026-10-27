using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Player/Player Settings")]
public class PlayerSettings : ScriptableObject
{
    [field: SerializeField]
    public CharacterSettings CharacterSettings { get; private set; }
    [field: SerializeField]
    public CameraSettings CameraSettings { get; private set; }
}