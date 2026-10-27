using UnityEngine;

public interface IPlayerCharacterInitData { }

public interface IPlayerCharacterUpdateData
{
    public float DeltaTime { get; set; }
}

public abstract class PlayerCharacter : MonoBehaviour
{
    [field: SerializeField]
    public Transform CameraTarget { get; set; }

    public abstract bool HasBeenInitialized { get; protected set; }
    public abstract string ActionMap { get; }
    public abstract bool MouseVisible { get; }
    public abstract bool DoCameraRotation { get; }

    public abstract void Init(IPlayerCharacterInitData playerCharacterInitData);
    public abstract void UpdateCharacter(ref IPlayerCharacterUpdateData playerCharacterUpdateData);

    public abstract void OnPausePressed();
}