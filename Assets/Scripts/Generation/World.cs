using UnityEngine;

public class World : MonoBehaviour
{
    // Singleton because only ever one world and makes placing and removing blocks easier
    public static World Instance { get; private set; }

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
    int m_TerrainHighest = 64;
    [Header("World Settings")]
    [field: SerializeField, Range(-128, -32)]
    public int MinHeight { get; private set; } = -128;
    [field: SerializeField, Range(32, 127)]
    public int MaxHeight { get; private set; } = 127;
    [SerializeField, Range(32, 256)]
    int m_WorldHalfSize = 64;
    [Header("Chunks")]
    [SerializeField]
    Chunk m_Chunk;
    [SerializeField, Range(1, 256)]
    int m_ChunkSize = 16;
    [Header("Blocks")]
    [SerializeField]
    BlockData m_Bedrock;
    [SerializeField]
    BlockData m_Stone;
    [SerializeField]
    BlockData m_Grass;
    [SerializeField]
    BlockData m_Snow;
    [SerializeField, Min(1), Tooltip("How many grass blocks from surface before stone starts spawning")]
    int m_GrassDepth = 3;
    [SerializeField, Min(1), Tooltip("How many snow blocks from surface before stone starts spawning")]
    int m_SnowDepth = 1;
    [SerializeField, Min(0)]
    int m_SnowHeight = 48;
    [Header("Player")]
    [SerializeField]
    FirstPersonPlayerCharacter m_Player;

    static readonly float s_MinOffset = -32768f, s_MaxOffset = 32767f;
    static readonly float s_ZoomScale = 64f; // Because use integers if don't increase zoom then perlin is to jagged

    int m_NumChunksOneAxis;
    Chunk[,] m_Chunks;

    public int WorldSize => m_WorldHalfSize * 2;
    int StartingTerrainHeightRange => m_TerrainHighest - m_TerrainLowest;

    float m_OffsetX, m_OffsetY;

    Vector3Int m_SpawnPoint;

    void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        if (Instance != this) return;
        InitSeed();
        m_NumChunksOneAxis = WorldSize / m_ChunkSize;
        InitChunks();
        m_TerrainLowest = Mathf.Max(MinHeight, m_TerrainLowest); // Ensure terrain lowest is never below min height
        m_TerrainHighest = Mathf.Min(m_TerrainHighest, MaxHeight); // Ensure terrain heighest is never above max height
        m_ChunkSize = Mathf.Min(m_ChunkSize, WorldSize); // Ensure chunk is never larger than world
        GenerateBlocks();
        GenerateMeshes();
        GenerateBoundries();
        RespawnPlayer();
    }

    public static Vector3Int IntCoord(Vector3 coord) => Vector3Int.FloorToInt(coord);

    public Chunk GetChunk(Vector3Int intCoord)
    {
        int x = (intCoord.x + m_WorldHalfSize) / m_ChunkSize;
        int z = (intCoord.z + m_WorldHalfSize) / m_ChunkSize;
        if (x < 0 || z < 0 || x >= m_NumChunksOneAxis || z >= m_NumChunksOneAxis) return null;
        return m_Chunks[x, z];
    }

    public Chunk GetChunk(Vector3 coord)
    {
        Vector3Int intCoord = IntCoord(coord);
        return GetChunk(intCoord);
    }

    public void RespawnPlayer()
    {
        if (!m_Player) return;
        CharacterController cc = m_Player.GetComponent<CharacterController>();
        cc.enabled = false;
        m_Player.transform.position = m_SpawnPoint;
        cc.enabled = true;
        cc.Move(Vector3.down);
    }

    public void BreakBlock(Vector3 coord)
    {
        Vector3Int intCoord = IntCoord(coord);
        Chunk chunk = GetChunk(intCoord);
        if (!chunk) return;
        chunk.SetBlock(intCoord, null);
        chunk.UpdateMesh();
        // vvv Adjacent Chunks vvv
        Vector3Int intCoordPosZ = intCoord + Vector3Int.forward;
        Chunk chunkPosZ = GetChunk(intCoordPosZ);
        if (chunkPosZ && chunkPosZ != chunk) // if chunkPosZ == chunk then no adjacent chunk in posZ so no need to update
            chunkPosZ.UpdateMesh();
        Vector3Int intCoordPosX = intCoord + Vector3Int.right;
        Chunk chunkPosX = GetChunk(intCoordPosX);
        if (chunkPosX && chunkPosX != chunk) // if chunkPosX == chunk then no adjacent chunk in posX so no need to update
            chunkPosX.UpdateMesh();
        Vector3Int intCoordNegZ = intCoord + Vector3Int.back;
        Chunk chunkNegZ = GetChunk(intCoordNegZ);
        if (chunkNegZ && chunkNegZ != chunk) // if chunkNegZ == chunk then no adjacent chunk in negZ so no need to update
            chunkNegZ.UpdateMesh();
        Vector3Int intCoordNegX = intCoord + Vector3Int.left;
        Chunk chunkNegX = GetChunk(intCoordNegX);
        if (chunkNegX && chunkNegX != chunk) // if chunkNegX == chunk then no adjacent chunk in negX so no need to update
            chunkNegX.UpdateMesh();
        // ^^^ Adjacent Chunks ^^^
    }

    public bool PlaceBlock(Vector3 coord, BlockData block)
    {
        Vector3Int intCoord = IntCoord(coord);
        Chunk chunk = GetChunk(intCoord);
        if (!chunk) return false;
        if (!chunk.SetBlock(intCoord, block)) return false;
        chunk.UpdateMesh();
        // vvv Adjacent Chunks vvv
        Vector3Int intCoordPosZ = intCoord + Vector3Int.forward;
        Chunk chunkPosZ = GetChunk(intCoordPosZ);
        if (chunkPosZ && chunkPosZ != chunk) // if chunkPosZ == chunk then no adjacent chunk in posZ so no need to update
            chunkPosZ.UpdateMesh();
        Vector3Int intCoordPosX = intCoord + Vector3Int.right;
        Chunk chunkPosX = GetChunk(intCoordPosX);
        if (chunkPosX && chunkPosX != chunk) // if chunkPosX == chunk then no adjacent chunk in posX so no need to update
            chunkPosX.UpdateMesh();
        Vector3Int intCoordNegZ = intCoord + Vector3Int.back;
        Chunk chunkNegZ = GetChunk(intCoordNegZ);
        if (chunkNegZ && chunkNegZ != chunk) // if chunkNegZ == chunk then no adjacent chunk in negZ so no need to update
            chunkNegZ.UpdateMesh();
        Vector3Int intCoordNegX = intCoord + Vector3Int.left;
        Chunk chunkNegX = GetChunk(intCoordNegX);
        if (chunkNegX && chunkNegX != chunk) // if chunkNegX == chunk then no adjacent chunk in negX so no need to update
            chunkNegX.UpdateMesh();
        // ^^^ Adjacent Chunks ^^^
        return true;
    }

    // Initialise the random seed as well as the offsets used in generation
    void InitSeed()
    {
        if (!m_UseSetSeed) m_Seed = Random.Range(int.MinValue, int.MaxValue);
        Random.InitState(m_Seed);
        m_OffsetX = Random.Range(s_MinOffset, s_MaxOffset);
        m_OffsetY = Random.Range(s_MinOffset, s_MaxOffset);
    }

    void InitChunks()
    {
        m_Chunks = new Chunk[m_NumChunksOneAxis, m_NumChunksOneAxis];
        for (int i = 0; i < m_NumChunksOneAxis; ++i)
        {
            for (int j = 0; j < m_NumChunksOneAxis; ++j)
            {
                m_Chunks[i, j] = Instantiate(m_Chunk, transform);
                Vector3Int chunkPos = new Vector3Int(-m_WorldHalfSize + i * m_ChunkSize, 0, -m_WorldHalfSize + j * m_ChunkSize);
                m_Chunks[i, j].transform.position = chunkPos;
                m_Chunks[i, j].InitChunk(chunkPos.x, chunkPos.z, m_ChunkSize, MinHeight, MaxHeight);
            }
        }
    }

    // Unity explicitly states PerlinNoise might return a value below 0 or above 1 so clamp it
    float Perlin01(float x, float y) => Mathf.Clamp01(Mathf.PerlinNoise(x, y));

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
            float perlin = Perlin01(sampleX, sampleY);
            noise += perlin * amplitude;
            maxPossibleHeight += amplitude;
            amplitude *= m_Persistence;
            frequency *= m_Lacunarity;
        }
        noise /= maxPossibleHeight;
        return Mathf.RoundToInt(noise * StartingTerrainHeightRange + m_TerrainLowest);
    }

    void GenerateBlocks()
    {
        for (int x = -m_WorldHalfSize; x < m_WorldHalfSize; ++x)
        {
            for (int z = -m_WorldHalfSize; z < m_WorldHalfSize; ++z)
            {
                int chunkX = (x + m_WorldHalfSize) / m_ChunkSize;
                int chunkZ = (z + m_WorldHalfSize) / m_ChunkSize;
                int height = TerrainHeight(x, z);
                if (x == 0 && z == 0) m_SpawnPoint = new Vector3Int(x, height + 1, z);
                Chunk chunk = m_Chunks[chunkX, chunkZ];
                for (int y = MinHeight; y <= height; ++y)
                {
                    Vector3Int coord = new Vector3Int(x, y, z);
                    BlockData block;
                    if (y == MinHeight) block = m_Bedrock; // Bottom of world
                    else if (height < m_SnowHeight) // Height is below where snow starts generating
                    {
                        if (y <= height - m_GrassDepth) block = m_Stone;
                        else block = m_Grass;
                    }
                    else
                    {
                        if (y <= height - m_SnowDepth) block = m_Stone;
                        else block = m_Snow;
                    }
                    chunk.SetBlock(coord, block);
                }
            }
        }
    }

    void GenerateMeshes()
    {
        for (int i = 0; i < m_NumChunksOneAxis; ++i)
        {
            for (int j = 0; j < m_NumChunksOneAxis; ++j)
            {
                m_Chunks[i, j].CreateMesh();
            }
        }
    }

    // Boundries to the world so the player won't walk off
    void GenerateBoundries()
    {
        int midHeight = (MaxHeight + MinHeight + 1) / 2;

        GameObject posZ = new GameObject("PosZBoundry");
        posZ.transform.parent = transform;
        BoxCollider posZBoundry = posZ.AddComponent<BoxCollider>();
        posZBoundry.center = new Vector3(0f, midHeight, m_WorldHalfSize + 0.5f);
        posZBoundry.size = new Vector3(WorldSize + 2f, MaxHeight - MinHeight + 51f, 1f);

        GameObject posX = new GameObject("PosXBoundry");
        posX.transform.parent = transform;
        BoxCollider posXBoundry = posX.AddComponent<BoxCollider>();
        posXBoundry.center = new Vector3(m_WorldHalfSize + 0.5f, midHeight, 0f);
        posXBoundry.size = new Vector3(1f, MaxHeight - MinHeight + 51, WorldSize + 2f);

        GameObject negZ = new GameObject("NegZBoundry");
        negZ.transform.parent = transform;
        BoxCollider negZBoundry = negZ.AddComponent<BoxCollider>();
        negZBoundry.center = new Vector3(0f, midHeight, -m_WorldHalfSize - 0.5f);
        negZBoundry.size = new Vector3(WorldSize + 2f, MaxHeight - MinHeight + 51f, 1f);

        GameObject negX = new GameObject("NegXBoundry");
        negX.transform.parent = transform;
        BoxCollider negXBoundry = negX.AddComponent<BoxCollider>();
        negXBoundry.center = new Vector3(-m_WorldHalfSize - 0.5f, midHeight, 0f);
        negXBoundry.size = new Vector3(1f, MaxHeight - MinHeight + 51, WorldSize + 2f);
    }
}
