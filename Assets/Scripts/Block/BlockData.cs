using UnityEngine;

[CreateAssetMenu(fileName = "BlockData", menuName = "Scriptable Objects/Block Data")]
public class BlockData : ScriptableObject
{
    [field: SerializeField]
    public bool Breakable { get; private set; }
    [field: SerializeField]
    public Color Color { get; private set; }
    [field: SerializeField, Min(0f)]
    public float BreakTime { get; private set; }
}
