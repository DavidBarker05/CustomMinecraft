using UnityEngine;

[CreateAssetMenu(fileName = "Block", menuName = "Scriptable Objects/Block")]
public class Block : ScriptableObject
{
    [field: SerializeField]
    public bool Breakable { get; private set; }
    [field: SerializeField]
    public Color Color { get; private set; }
    [field: SerializeField, Min(0f)]
    public float BreakTime { get; private set; }
}
