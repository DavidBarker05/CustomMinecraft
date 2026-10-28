using TMPro;
using UnityEngine;

public class InventoryBlockDisplay : MonoBehaviour
{
    [SerializeField]
    GameObject m_SelectedHighlight;
    [SerializeField]
    TMP_Text m_Text;
    [SerializeField, Range(0, 9)]
    int m_InventoryKey;
    [SerializeField]
    BlockData m_Block;
    [SerializeField]
    FirstPersonPlayerCharacter m_Player;

    void OnEnable() => UpdateDisplay();

    void Start()
    {
        m_Player.OnInventoryUpdated.AddListener(UpdateDisplay);
        UpdateDisplay(); // In case OnEnable happens before player inventory updates or anything
    }

    void UpdateDisplay()
    {
        m_SelectedHighlight.SetActive(m_Player.CurrentPlaceBlock == m_Block);
        m_Text.text = $"{m_InventoryKey}. {m_Block.DisplayName}: {m_Player.GetBlockQuantity(m_Block)}";
    }
}
