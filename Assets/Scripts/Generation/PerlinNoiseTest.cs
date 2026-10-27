using UnityEngine;

public class PerlinNoiseTest : MonoBehaviour
{
    [SerializeField]
    GameObject m_Cube;
    [Header("Seed")]
    [SerializeField]
    bool m_UseSetSeed = false;
    [SerializeField]
    int m_Seed;
    [Header("Generation")]
    [SerializeField, Range(-128, -32)]
    int m_MinHeight = -128;
    [SerializeField, Range(32, 128)]
    int m_MaxHeight = 128;
    [SerializeField, Range(32, 256)]
    int m_WorldHalfSize = 64;
    [SerializeField, Range(-16, 0)]
    int m_TerrainLowest = -8;
    [SerializeField, Range(0, 64)]
    int m_TerrainHeighest = 64;

    static readonly float s_MinOffset = -32768f, s_MaxOffset = 32767f;

    int HeightRange => m_TerrainHeighest - m_TerrainLowest;

    float m_OffsetX, m_OffsetY;

    void Start()
    {
        InitSeed();
        for (int x = -m_WorldHalfSize; x <= m_WorldHalfSize; ++x)
        {
            for (int y = -m_WorldHalfSize; y <= m_WorldHalfSize; ++y)
            {
                int height = TerrainHeight(x, y);
                Instantiate(m_Cube, new Vector3Int(x, height, y), Quaternion.identity);
            }
        }
    }

    // Initialise the random seed as well as the offsets used in generation
    void InitSeed()
    {
        if (!m_UseSetSeed) m_Seed = Random.Range(int.MinValue, int.MaxValue);
        Random.InitState(m_Seed);
        m_OffsetX = Random.Range(s_MinOffset, s_MaxOffset);
        m_OffsetY = Random.Range(s_MinOffset, s_MaxOffset);
    }

    // Unity explicitly states PerlinNoise might return a value below 0 or above 1 so clamp it
    float Perlin01(float x, float y) => Mathf.Clamp01(Mathf.PerlinNoise(x, y));

    // PerlinNoise gives the same output for x and y coordinate every time so apply an offset to each to randomise it
    float RandomPerlin(float x, float y) => Perlin01(x + m_OffsetX, y + m_OffsetY);

    // Terrain height in range [m_MinHeight, m_MaxHeight], as int so square like minecraft
    int TerrainHeight(int x, int y) => Mathf.RoundToInt(RandomPerlin(x / 64f, y / 64f) * HeightRange + m_TerrainLowest);
}
