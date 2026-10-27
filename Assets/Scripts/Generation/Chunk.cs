using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    MeshFilter m_MeshFilter;

    int m_StartX, m_StartZ;
    BlockData[,,] m_Blocks;
    int m_ChunkSize;
    int m_MinHeight;
    int m_MaxHeight;

    Mesh m_Mesh;

    void Awake() => m_MeshFilter = GetComponent<MeshFilter>();

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

    public void SetBlock(Vector3Int coord, BlockData block)
    {
        Vector3Int chunkCoord = ToChunkCoord(coord);
        if (chunkCoord.x < 0 || chunkCoord.x >= m_ChunkSize) return;
        if (chunkCoord.y < 0 || chunkCoord.y >= m_MaxHeight - m_MinHeight + 1) return;
        if (chunkCoord.z < 0 || chunkCoord.z >= m_ChunkSize) return;
        m_Blocks[chunkCoord.x, chunkCoord.y, chunkCoord.z] = block;
    }

    void UpdateMesh()
    {
        m_Mesh.Clear();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        for (int x = 0; x < m_ChunkSize; ++x)
        {
            for (int y = 0; y < m_MaxHeight - m_MinHeight + 1; ++y)
            {
                for (int z = 0; z < m_ChunkSize; ++z)
                {
                    if (!m_Blocks[x, y, z]) continue; // Empty
                    BlockPosY(x, y, z, ref vertices, ref triangles);
                    BlockPosZ(x, y, z, ref vertices, ref triangles);
                    BlockPosX(x, y, z, ref vertices, ref triangles);
                    BlockNegZ(x, y, z, ref vertices, ref triangles);
                    BlockNegX(x, y, z, ref vertices, ref triangles);
                    BlockNegY(x, y, z, ref vertices, ref triangles);
                }
            }
        }
        m_Mesh.vertices = vertices.ToArray();
        m_Mesh.triangles = triangles.ToArray();
        m_Mesh.RecalculateNormals();
        m_Mesh.RecalculateTangents();
    }

    void BlockPosY(int x, int y, int z, ref List<Vector3> vertices, ref List<int> triangles)
    {
        int worldY = y + m_MinHeight;
        if (worldY == m_MaxHeight || !m_Blocks[x, y + 1, z])
        {
            Vector3 v010 = new Vector3(x, worldY + 1, z);
            Vector3 v011 = new Vector3(x, worldY + 1, z + 1);
            Vector3 v111 = new Vector3(x + 1, worldY + 1, z + 1);
            Vector3 v110 = new Vector3(x + 1, worldY + 1, z);
            int i010;
            if (vertices.Contains(v010)) i010 = vertices.IndexOf(v010);
            else
            {
                i010 = vertices.Count;
                vertices.Add(v010);
            }
            int i011;
            if (vertices.Contains(v011)) i011 = vertices.IndexOf(v011);
            else
            {
                i011 = vertices.Count;
                vertices.Add(v011);
            }
            int i111;
            if (vertices.Contains(v111)) i111 = vertices.IndexOf(v111);
            else
            {
                i111 = vertices.Count;
                vertices.Add(v111);
            }
            int i110;
            if (vertices.Contains(v110)) i110 = vertices.IndexOf(v110);
            else
            {
                i110 = vertices.Count;
                vertices.Add(v110);
            }
            triangles.Add(i010);
            triangles.Add(i011);
            triangles.Add(i111);
            triangles.Add(i010);
            triangles.Add(i111);
            triangles.Add(i110);
        }
    }

    void BlockPosZ(int x, int y, int z, ref List<Vector3> vertices, ref List<int> triangles)
    {
        int worldY = y + m_MinHeight;
        if (z < m_ChunkSize - 1)
        {
            if (m_Blocks[x, y, z + 1]) return; // Not exposed
            Vector3 v101 = new Vector3(x + 1, worldY, z + 1);
            Vector3 v111 = new Vector3(x + 1, worldY + 1, z + 1);
            Vector3 v011 = new Vector3(x, worldY + 1, z + 1);
            Vector3 v001 = new Vector3(x, worldY, z + 1);
            int i101;
            if (vertices.Contains(v101)) i101 = vertices.IndexOf(v101);
            else
            {
                i101 = vertices.Count;
                vertices.Add(v101);
            }
            int i111;
            if (vertices.Contains(v111)) i111 = vertices.IndexOf(v111);
            else
            {
                i111 = vertices.Count;
                vertices.Add(v111);
            }
            int i011;
            if (vertices.Contains(v011)) i011 = vertices.IndexOf(v011);
            else
            {
                i011 = vertices.Count;
                vertices.Add(v011);
            }
            int i001;
            if (vertices.Contains(v001)) i001 = vertices.IndexOf(v001);
            else
            {
                i001 = vertices.Count;
                vertices.Add(v001);
            }
            triangles.Add(i101);
            triangles.Add(i111);
            triangles.Add(i011);
            triangles.Add(i101);
            triangles.Add(i011);
            triangles.Add(i001);
        }
    }

    void BlockPosX(int x, int y, int z, ref List<Vector3> vertices, ref List<int> triangles)
    {
        int worldY = y + m_MinHeight;
        if (x < m_ChunkSize - 1)
        {
            if (m_Blocks[x + 1, y, z]) return; // Not exposed
            Vector3 v100 = new Vector3(x + 1, worldY, z);
            Vector3 v110 = new Vector3(x + 1, worldY + 1, z);
            Vector3 v111 = new Vector3(x + 1, worldY + 1, z + 1);
            Vector3 v101 = new Vector3(x + 1, worldY, z + 1);
            int i100;
            if (vertices.Contains(v100)) i100 = vertices.IndexOf(v100);
            else
            {
                i100 = vertices.Count;
                vertices.Add(v100);
            }
            int i110;
            if (vertices.Contains(v110)) i110 = vertices.IndexOf(v110);
            else
            {
                i110 = vertices.Count;
                vertices.Add(v110);
            }
            int i111;
            if (vertices.Contains(v111)) i111 = vertices.IndexOf(v111);
            else
            {
                i111 = vertices.Count;
                vertices.Add(v111);
            }
            int i101;
            if (vertices.Contains(v101)) i101 = vertices.IndexOf(v101);
            else
            {
                i101 = vertices.Count;
                vertices.Add(v101);
            }
            triangles.Add(i100);
            triangles.Add(i110);
            triangles.Add(i111);
            triangles.Add(i100);
            triangles.Add(i111);
            triangles.Add(i101);
        }
    }

    void BlockNegZ(int x, int y, int z, ref List<Vector3> vertices, ref List<int> triangles)
    {
        int worldY = y + m_MinHeight;
        if (z > 0)
        {
            if (m_Blocks[x, y, z - 1]) return; // Not exposed
            Vector3 v000 = new Vector3(x, worldY, z);
            Vector3 v010 = new Vector3(x, worldY + 1, z);
            Vector3 v110 = new Vector3(x + 1, worldY + 1, z);
            Vector3 v100 = new Vector3(x + 1, worldY, z);
            int i000;
            if (vertices.Contains(v000)) i000 = vertices.IndexOf(v000);
            else
            {
                i000 = vertices.Count;
                vertices.Add(v000);
            }
            int i010;
            if (vertices.Contains(v010)) i010 = vertices.IndexOf(v010);
            else
            {
                i010 = vertices.Count;
                vertices.Add(v010);
            }
            int i110;
            if (vertices.Contains(v110)) i110 = vertices.IndexOf(v110);
            else
            {
                i110 = vertices.Count;
                vertices.Add(v110);
            }
            int i100;
            if (vertices.Contains(v100)) i100 = vertices.IndexOf(v100);
            else
            {
                i100 = vertices.Count;
                vertices.Add(v100);
            }
            triangles.Add(i000);
            triangles.Add(i010);
            triangles.Add(i110);
            triangles.Add(i000);
            triangles.Add(i110);
            triangles.Add(i100);
        }
    }

    void BlockNegX(int x, int y, int z, ref List<Vector3> vertices, ref List<int> triangles)
    {
        int worldY = y + m_MinHeight;
        if (x > 0)
        {
            if (m_Blocks[x - 1, y, z]) return; // Not exposed
            Vector3 v001 = new Vector3(x, worldY, z + 1);
            Vector3 v011 = new Vector3(x, worldY + 1, z + 1);
            Vector3 v010 = new Vector3(x, worldY + 1, z);
            Vector3 v000 = new Vector3(x, worldY, z);
            int i001;
            if (vertices.Contains(v001)) i001 = vertices.IndexOf(v001);
            else
            {
                i001 = vertices.Count;
                vertices.Add(v001);
            }
            int i011;
            if (vertices.Contains(v011)) i011 = vertices.IndexOf(v011);
            else
            {
                i011 = vertices.Count;
                vertices.Add(v011);
            }
            int i010;
            if (vertices.Contains(v010)) i010 = vertices.IndexOf(v010);
            else
            {
                i010 = vertices.Count;
                vertices.Add(v010);
            }
            int i000;
            if (vertices.Contains(v000)) i000 = vertices.IndexOf(v000);
            else
            {
                i000 = vertices.Count;
                vertices.Add(v000);
            }
            triangles.Add(i001);
            triangles.Add(i011);
            triangles.Add(i010);
            triangles.Add(i001);
            triangles.Add(i010);
            triangles.Add(i000);
        }
    }

    void BlockNegY(int x, int y, int z, ref List<Vector3> vertices, ref List<int> triangles)
    {
        int worldY = y + m_MinHeight;
        if (worldY == m_MinHeight || !m_Blocks[x, y - 1, z])
        {
            Vector3 v001 = new Vector3(x, worldY, z + 1);
            Vector3 v000 = new Vector3(x, worldY, z);
            Vector3 v100 = new Vector3(x + 1, worldY, z);
            Vector3 v101 = new Vector3(x + 1, worldY, z + 1);
            int i001;
            if (vertices.Contains(v001)) i001 = vertices.IndexOf(v001);
            else
            {
                i001 = vertices.Count;
                vertices.Add(v001);
            }
            int i000;
            if (vertices.Contains(v000)) i000 = vertices.IndexOf(v000);
            else
            {
                i000 = vertices.Count;
                vertices.Add(v000);
            }
            int i100;
            if (vertices.Contains(v100)) i100 = vertices.IndexOf(v100);
            else
            {
                i100 = vertices.Count;
                vertices.Add(v100);
            }
            int i101;
            if (vertices.Contains(v101)) i101 = vertices.IndexOf(v101);
            else
            {
                i101 = vertices.Count;
                vertices.Add(v101);
            }
            triangles.Add(i001);
            triangles.Add(i000);
            triangles.Add(i100);
            triangles.Add(i001);
            triangles.Add(i100);
            triangles.Add(i101);
        }
    }
}
