using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class InventoryBlockDisplay : MonoBehaviour
{
    [SerializeField]
    BlockData m_Block;
    [SerializeField]
    FirstPersonPlayerCharacter m_Player;

    TMP_Text m_Text;

    void Awake() => m_Text = GetComponent<TMP_Text>();

    void OnEnable()
    {
        if (!m_Text) m_Text = GetComponent<TMP_Text>();
        UpdateText();
    }

    void Start() => m_Player.OnInventoryUpdated.AddListener(UpdateText);

    void UpdateText() => m_Text.text = $"{m_Block.DisplayName}: {m_Player.GetBlockQuantity(m_Block)}";
}
