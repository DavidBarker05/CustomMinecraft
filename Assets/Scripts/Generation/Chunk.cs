using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer),
typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    MeshFilter m_MeshFilter;
    MeshCollider m_MeshCollider;

    int m_StartX, m_StartZ;
    BlockData[,,] m_Blocks;
    int m_ChunkSize;
    int m_MinHeight;
    int m_MaxHeight;

    Mesh m_Mesh;

    void Awake()
    {
        m_MeshFilter = GetComponent<MeshFilter>();
        m_MeshCollider = GetComponent<MeshCollider>();
    }

    void OnDestroy()
    {
        // Destroy mesh otherwise memory leaks
        // Learned this in an early version of my 3D platformer
        if (m_Mesh) Destroy(m_Mesh);
    }

    public void InitChunk(int chunkStartX, int chunkStartZ, int chunkSize, int minHeight, int maxHeight)
    {
        m_StartX = chunkStartX;
        m_StartZ = chunkStartZ;
        m_Blocks = new BlockData[chunkSize, maxHeight - minHeight + 1, chunkSize];
        m_ChunkSize = chunkSize;
        m_MinHeight = minHeight;
        m_MaxHeight = maxHeight;
    }

    public void CreateMesh()
    {
        if (!m_Mesh) m_Mesh = new Mesh();
        UpdateMesh();
        m_MeshFilter.mesh = m_Mesh;
    }

    public Vector3Int ToChunkCoord(Vector3Int coord) => new Vector3Int(
        x: coord.x - m_StartX,
        y: coord.y - m_MinHeight,
        z: coord.z - m_StartZ
    );

    public bool SetBlock(Vector3Int coord, BlockData block)
    {
        Vector3Int chunkCoord = ToChunkCoord(coord);
        if (chunkCoord.x < 0 || chunkCoord.x >= m_ChunkSize) return false;
        if (chunkCoord.y < 0 || chunkCoord.y >= m_MaxHeight - m_MinHeight + 1) return false;
        if (chunkCoord.z < 0 || chunkCoord.z >= m_ChunkSize) return false;
        m_Blocks[chunkCoord.x, chunkCoord.y, chunkCoord.z] = block;
        return true;
    }

    public BlockData GetBlock(Vector3Int coord)
    {
        Vector3Int chunkCoord = ToChunkCoord(coord);
        if (chunkCoord.x < 0 || chunkCoord.x >= m_ChunkSize) return null;
        if (chunkCoord.y < 0 || chunkCoord.y >= m_MaxHeight - m_MinHeight + 1) return null;
        if (chunkCoord.z < 0 || chunkCoord.z >= m_ChunkSize) return null;
        return m_Blocks[chunkCoord.x, chunkCoord.y, chunkCoord.z];
    }

    public void UpdateMesh()
    {
        m_Mesh.Clear();
        m_MeshCollider.sharedMesh = null;
        List<Vector3> vertices = new List<Vector3>();
        List<Color> colors = new List<Color>();
        List<int> triangles = new List<int>();
        for (int x = 0; x < m_ChunkSize; ++x)
        {
            for (int y = 0; y < m_MaxHeight - m_MinHeight + 1; ++y)
            {
                for (int z = 0; z < m_ChunkSize; ++z)
                {
                    if (!m_Blocks[x, y, z]) continue; // Empty
                    BlockPosY(x, y, z, ref vertices, ref colors, ref triangles);
                    BlockPosZ(x, y, z, ref vertices, ref colors, ref triangles);
                    BlockPosX(x, y, z, ref vertices, ref colors, ref triangles);
                    BlockNegZ(x, y, z, ref vertices, ref colors, ref triangles);
                    BlockNegX(x, y, z, ref vertices, ref colors, ref triangles);
                    BlockNegY(x, y, z, ref vertices, ref colors, ref triangles);
                }
            }
        }
        m_Mesh.vertices = vertices.ToArray();
        m_Mesh.triangles = triangles.ToArray();
        m_Mesh.colors = colors.ToArray();
        m_Mesh.RecalculateNormals();
        m_Mesh.RecalculateTangents();
        m_MeshCollider.sharedMesh = m_Mesh;
    }

    void BlockPosY(int x, int y, int z, ref List<Vector3> vertices, ref List<Color> colors, ref List<int> triangles)
    {
        int worldY = y + m_MinHeight;
        if (worldY == m_MaxHeight || !m_Blocks[x, y + 1, z])
        {
            Vector3 v010 = new Vector3(x, worldY + 1, z);
            Vector3 v011 = new Vector3(x, worldY + 1, z + 1);
            Vector3 v111 = new Vector3(x + 1, worldY + 1, z + 1);
            Vector3 v110 = new Vector3(x + 1, worldY + 1, z);
            BlockData data = m_Blocks[x, y, z];
            int i010 = vertices.Count;
            vertices.Add(v010);
            colors.Add(data.Color);
            int i011 = vertices.Count;
            vertices.Add(v011);
            colors.Add(data.Color);
            int i111 = vertices.Count;
            vertices.Add(v111);
            colors.Add(data.Color);
            int i110 = vertices.Count;
            vertices.Add(v110);
            colors.Add(data.Color);
            triangles.Add(i010);
            triangles.Add(i011);
            triangles.Add(i111);
            triangles.Add(i010);
            triangles.Add(i111);
            triangles.Add(i110);
        }
    }

    void GenerateFacePosZ(int x, int y, int z, ref List<Vector3> vertices, ref List<Color> colors, ref List<int> triangles)
    {
        int worldY = y + m_MinHeight;
        Vector3 v101 = new Vector3(x + 1, worldY, z + 1);
        Vector3 v111 = new Vector3(x + 1, worldY + 1, z + 1);
        Vector3 v011 = new Vector3(x, worldY + 1, z + 1);
        Vector3 v001 = new Vector3(x, worldY, z + 1);
        BlockData data = m_Blocks[x, y, z];
        int i101 = vertices.Count;
        vertices.Add(v101);
        colors.Add(data.Color);
        int i111 = vertices.Count;
        vertices.Add(v111);
        colors.Add(data.Color);
        int i011 = vertices.Count;
        vertices.Add(v011);
        colors.Add(data.Color);
        int i001 = vertices.Count;
        vertices.Add(v001);
        colors.Add(data.Color);
        triangles.Add(i101);
        triangles.Add(i111);
        triangles.Add(i011);
        triangles.Add(i101);
        triangles.Add(i011);
        triangles.Add(i001);
    }

    void BlockPosZ(int x, int y, int z, ref List<Vector3> vertices, ref List<Color> colors, ref List<int> triangles)
    {
        int worldY = y + m_MinHeight;
        if (z < m_ChunkSize - 1)
        {
            if (m_Blocks[x, y, z + 1]) return; // Not exposed
            GenerateFacePosZ(x, y, z, ref vertices, ref colors, ref triangles);
        }
        else
        {
            Vector3Int adjacentCoord = new Vector3Int(m_StartX + x, worldY, m_StartZ + z + 1);
            Chunk adjacentChunk = World.Instance.GetChunk(adjacentCoord);
            if (!adjacentChunk) GenerateFacePosZ(x, y, z, ref vertices, ref colors, ref triangles);
            else
            {
                BlockData adjacentBlock = adjacentChunk.GetBlock(adjacentCoord);
                if (!adjacentBlock) GenerateFacePosZ(x, y, z, ref vertices, ref colors, ref triangles);
            }
        }
    }

    void GenerateFacePosX(int x, int y, int z, ref List<Vector3> vertices, ref List<Color> colors, ref List<int> triangles)
    {
        int worldY = y + m_MinHeight;
        Vector3 v100 = new Vector3(x + 1, worldY, z);
        Vector3 v110 = new Vector3(x + 1, worldY + 1, z);
        Vector3 v111 = new Vector3(x + 1, worldY + 1, z + 1);
        Vector3 v101 = new Vector3(x + 1, worldY, z + 1);
        BlockData data = m_Blocks[x, y, z];
        int i100 = vertices.Count;
        vertices.Add(v100);
        colors.Add(data.Color);
        int i110 = vertices.Count;
        vertices.Add(v110);
        colors.Add(data.Color);
        int i111 = vertices.Count;
        vertices.Add(v111);
        colors.Add(data.Color);
        int i101 = vertices.Count;
        vertices.Add(v101);
        colors.Add(data.Color);
        triangles.Add(i100);
        triangles.Add(i110);
        triangles.Add(i111);
        triangles.Add(i100);
        triangles.Add(i111);
        triangles.Add(i101);
    }

    void BlockPosX(int x, int y, int z, ref List<Vector3> vertices, ref List<Color> colors, ref List<int> triangles)
    {
        int worldY = y + m_MinHeight;
        if (x < m_ChunkSize - 1)
        {
            if (m_Blocks[x + 1, y, z]) return; // Not exposed
            GenerateFacePosX(x, y, z, ref vertices, ref colors, ref triangles);
        }
        else
        {
            Vector3Int adjacentCoord = new Vector3Int(m_StartX + x + 1, worldY, m_StartZ + z);
            Chunk adjacentChunk = World.Instance.GetChunk(adjacentCoord);
            if (!adjacentChunk) GenerateFacePosX(x, y, z, ref vertices, ref colors, ref triangles);
            else
            {
                BlockData adjacentBlock = adjacentChunk.GetBlock(adjacentCoord);
                if (!adjacentBlock) GenerateFacePosX(x, y, z, ref vertices, ref colors, ref triangles);
            }
        }
    }

    void GenerateFaceNegZ(int x, int y, int z, ref List<Vector3> vertices, ref List<Color> colors, ref List<int> triangles)
    {
        int worldY = y + m_MinHeight;
        Vector3 v000 = new Vector3(x, worldY, z);
        Vector3 v010 = new Vector3(x, worldY + 1, z);
        Vector3 v110 = new Vector3(x + 1, worldY + 1, z);
        Vector3 v100 = new Vector3(x + 1, worldY, z);
        BlockData data = m_Blocks[x, y, z];
        int i000 = vertices.Count;
        vertices.Add(v000);
        colors.Add(data.Color);
        int i010 = vertices.Count;
        vertices.Add(v010);
        colors.Add(data.Color);
        int i110 = vertices.Count;
        vertices.Add(v110);
        colors.Add(data.Color);
        int i100 = vertices.Count;
        vertices.Add(v100);
        colors.Add(data.Color);
        triangles.Add(i000);
        triangles.Add(i010);
        triangles.Add(i110);
        triangles.Add(i000);
        triangles.Add(i110);
        triangles.Add(i100);
    }

    void BlockNegZ(int x, int y, int z, ref List<Vector3> vertices, ref List<Color> colors, ref List<int> triangles)
    {
        int worldY = y + m_MinHeight;
        if (z > 0)
        {
            if (m_Blocks[x, y, z - 1]) return; // Not exposed
            GenerateFaceNegZ(x, y, z, ref vertices, ref colors, ref triangles);
        }
        else
        {
            Vector3Int adjacentCoord = new Vector3Int(m_StartX + x, worldY, m_StartZ + z - 1);
            Chunk adjacentChunk = World.Instance.GetChunk(adjacentCoord);
            if (!adjacentChunk) GenerateFaceNegZ(x, y, z, ref vertices, ref colors, ref triangles);
            else
            {
                BlockData adjacentBlock = adjacentChunk.GetBlock(adjacentCoord);
                if (!adjacentBlock) GenerateFaceNegZ(x, y, z, ref vertices, ref colors, ref triangles);
            }
        }
    }

    void GenerateFaceNegX(int x, int y, int z, ref List<Vector3> vertices, ref List<Color> colors, ref List<int> triangles)
    {
        int worldY = y + m_MinHeight;
        Vector3 v001 = new Vector3(x, worldY, z + 1);
        Vector3 v011 = new Vector3(x, worldY + 1, z + 1);
        Vector3 v010 = new Vector3(x, worldY + 1, z);
        Vector3 v000 = new Vector3(x, worldY, z);
        BlockData data = m_Blocks[x, y, z];
        int i001 = vertices.Count;
        vertices.Add(v001);
        colors.Add(data.Color);
        int i011 = vertices.Count;
        vertices.Add(v011);
        colors.Add(data.Color);
        int i010 = vertices.Count;
        vertices.Add(v010);
        colors.Add(data.Color);
        int i000 = vertices.Count;
        vertices.Add(v000);
        colors.Add(data.Color);
        triangles.Add(i001);
        triangles.Add(i011);
        triangles.Add(i010);
        triangles.Add(i001);
        triangles.Add(i010);
        triangles.Add(i000);
    }

    void BlockNegX(int x, int y, int z, ref List<Vector3> vertices, ref List<Color> colors, ref List<int> triangles)
    {
        int worldY = y + m_MinHeight;
        if (x > 0)
        {
            if (m_Blocks[x - 1, y, z]) return; // Not exposed
            GenerateFaceNegX(x, y, z, ref vertices, ref colors, ref triangles);
        }
        else
        {
            Vector3Int adjacentCoord = new Vector3Int(m_StartX + x - 1, worldY, m_StartZ + z);
            Chunk adjacentChunk = World.Instance.GetChunk(adjacentCoord);
            if (!adjacentChunk) GenerateFaceNegX(x, y, z, ref vertices, ref colors, ref triangles);
            else
            {
                BlockData adjacentBlock = adjacentChunk.GetBlock(adjacentCoord);
                if (!adjacentBlock) GenerateFaceNegX(x, y, z, ref vertices, ref colors, ref triangles);
            }
        }
    }

    void BlockNegY(int x, int y, int z, ref List<Vector3> vertices, ref List<Color> colors, ref List<int> triangles)
    {
        int worldY = y + m_MinHeight;
        if (worldY == m_MinHeight || !m_Blocks[x, y - 1, z])
        {
            Vector3 v001 = new Vector3(x, worldY, z + 1);
            Vector3 v000 = new Vector3(x, worldY, z);
            Vector3 v100 = new Vector3(x + 1, worldY, z);
            Vector3 v101 = new Vector3(x + 1, worldY, z + 1);
            BlockData data = m_Blocks[x, y, z];
            int i001 = vertices.Count;
            vertices.Add(v001);
            colors.Add(data.Color);
            int i000 = vertices.Count;
            vertices.Add(v000);
            colors.Add(data.Color);
            int i100 = vertices.Count;
            vertices.Add(v100);
            colors.Add(data.Color);
            int i101 = vertices.Count;
            vertices.Add(v101);
            colors.Add(data.Color);
            triangles.Add(i001);
            triangles.Add(i000);
            triangles.Add(i100);
            triangles.Add(i001);
            triangles.Add(i100);
            triangles.Add(i101);
        }
    }
}
