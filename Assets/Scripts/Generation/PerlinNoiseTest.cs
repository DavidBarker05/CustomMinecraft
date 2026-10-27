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
    [Header("Perlin Noise Settings")]
    [SerializeField, Range(1, 8)]
    int m_Octaves = 4;
    [SerializeField, Range(1f, 8f)]
    float m_Lacunarity = 2f;
    [SerializeField, Range(0f, 1f)]
    float m_Persistence = 0.4f;
    [SerializeField, Range(-16, 0)]
    int m_TerrainLowest = -8;
    [SerializeField, Range(0, 64)]
    int m_TerrainHeighest = 64;
    [Header("World Settings")]
    [SerializeField, Range(-128, -32)]
    int m_MinHeight = -128;
    [SerializeField, Range(32, 128)]
    int m_MaxHeight = 128;
    [SerializeField, Range(32, 256)]
    int m_WorldHalfSize = 64;

    static readonly float s_MinOffset = -32768f, s_MaxOffset = 32767f;
    static readonly float s_ZoomScale = 64f; // Because use integers if don't increase zoom then perlin is to jagged

    int HeightRange => m_TerrainHeighest - m_TerrainLowest;

    float m_OffsetX, m_OffsetY;

    void Start()
    {
        InitSeed();
        m_TerrainLowest = Mathf.Max(m_MinHeight, m_TerrainLowest); // Ensure terrain lowest is never below min height
        m_TerrainHeighest = Mathf.Min(m_TerrainHeighest, m_MaxHeight); // Ensure terrain heighest is never above max height
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

    // Terrain height in range [m_TerrainLowest, m_TerrainHeighest], as int so square like minecraft
    // Also use layered perlin noise for smoother terrain
    int TerrainHeight(int x, int y)
    {
        float noise = 0f, frequency = 1f, amplitude = 1f;
        float maxPossibleHeight = 0f; // Used to normalise noise back to 0-1
        for (int i = 0; i < m_Octaves; i++)
        {
            float sampleX = x / s_ZoomScale * frequency + m_OffsetX;
            float sampleY = y / s_ZoomScale * frequency + m_OffsetY;
            float perlin = RandomPerlin(sampleX, sampleY);
            noise += perlin * amplitude;
            maxPossibleHeight += amplitude;
            amplitude *= m_Persistence;
            frequency *= m_Lacunarity;
        }
        noise /= maxPossibleHeight;
        return Mathf.RoundToInt(noise * HeightRange + m_TerrainLowest);
    }
}
