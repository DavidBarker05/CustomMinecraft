using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    MeshFilter m_MeshFilter;

    int m_StartX, m_StartZ;
    Block[,,] m_Blocks;
    int m_ChunkSize;
    int m_MinHeight;
    int m_MaxHeight;

    Mesh m_Mesh;

    void Awake()
    {
        m_MeshFilter = GetComponent<MeshFilter>();
    }

    void OnDestroy()
    {
        if (m_Mesh) Destroy(m_Mesh);
    }

    public void InitChunk(int chunkStartX, int chunkStartZ, int chunkSize, int minHeight, int maxHeight)
    {
        m_StartX = chunkStartX;
        m_StartZ = chunkStartZ;
        m_Blocks = new Block[chunkSize, maxHeight - minHeight + 1, chunkSize];
        m_ChunkSize = chunkSize;
        m_MinHeight = minHeight;
        m_MaxHeight = maxHeight;
    }

    public void CreateMesh()
    {

    }

    public Vector3Int ToChunkCoord(Vector3Int coord) => new Vector3Int(
        x: coord.x - m_StartX,
        y: coord.y - m_MinHeight,
        z: coord.z - m_StartZ
    );

    public void SetBlock(Vector3Int chunkCoord, Block block)
    {
        if (chunkCoord.x < 0 || chunkCoord.x >= m_ChunkSize) return;
        if (chunkCoord.y < 0 || chunkCoord.y >= m_MaxHeight - m_MinHeight + 1) return;
        if (chunkCoord.z < 0 || chunkCoord.z >= m_ChunkSize) return;
        m_Blocks[chunkCoord.x, chunkCoord.y, chunkCoord.z] = block;
    }
}
