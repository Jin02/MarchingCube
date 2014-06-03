using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MarchingTetrahedra
{
	public float    densityTarget
	{
		set
		{
			_target = value;
		}
	}
	private float   _target;
	private Hashtable _hash = new Hashtable();
	private int index = 0;
	
	public MarchingTetrahedra()
	{
		_target = 0.0f;
	}
	
	public Mesh CreateMesh(float[, ,] voxels)
	{
		List<Vector3> verts = new List<Vector3>();
		List<int> index = new List<int>();
		
		float[] cube = new float[8];

		for(int x = 0; x < voxels.GetLength(0)-1; x++)
		{
			for(int y = 0; y < voxels.GetLength(1)-1; y++)
			{
				for(int z = 0; z < voxels.GetLength(2)-1; z++)
				{
					//큐브 값 채움
					FillCube(x, y, z, voxels, ref cube);

					//Tetrahedra 구성
					MarchCubeTetrahedron(new Vector3(x,y,z), cube, verts, index);
				}
			}
		}
		
		
		Mesh mesh = new Mesh();
		mesh.vertices = verts.ToArray();
		Debug.Log("Marching Tetrahedra Geometry Info : " + " VertexNum: " + verts.Count + " indexNum:" + index.Count);
		mesh.triangles = index.ToArray();
		
		return mesh;
	}
	
	void MarchCubeTetrahedron(Vector3 pos, float[] cube, List<Vector3> vertList, List<int> indexList)
	{
		int i, j, vertexInACube;
		Vector3[] cubePosition = new Vector3[8];
		Vector3[] tetrahedronPosition = new Vector3[4];
		float[] tetrahedronValue = new float[4];
		
		//코너 위치 세팅. 
		for(i = 0; i < 8; i++)
			cubePosition[i] =
					new Vector3( pos.x + vertexOffset[i,0],
					 pos.y + vertexOffset[i,1],
					  pos.z + vertexOffset[i,2]);
		
		// 큐브 내에서 6등분 함
		for(i = 0; i < 6; i++)
		{
			//4면체가 필요한 버텍스는 4개
	        for(j = 0; j < 4; j++)
	        {
                vertexInACube = tetrahedronsInACube[i,j];
                tetrahedronPosition[j] = cubePosition[vertexInACube];
                tetrahedronValue[j] = cube[vertexInACube];
	        }
			
			// Marching Tetrahedra 구성
	        MarchTetrahedron(tetrahedronPosition, tetrahedronValue, vertList, indexList);
		}
	}

	void MarchTetrahedron(Vector3[] tetrahedronPosition, float[] tetrahedronValue, List<Vector3> vertList, List<int> indexList)
	{	
		// Marching Cube처럼 flagIndex 구성
		int flagIndex = 0;	
		for (int i = 0; i < 4; i++)
		{
			if (tetrahedronValue [i] > _target)
				flagIndex |= 1 << i;
		}

		// MC와 동일
	    int edgeFlags = tetrahedronEdgeFlags[flagIndex];	
	    if(edgeFlags == 0) return;

		Vector3[] edgeVertex = new Vector3[6];
		for(int i = 0; i < 6; i++)
	    {
            if((edgeFlags & (1<<i)) != 0)
            {
                int vert0 = tetrahedronEdgeConnection[i,0];
                int vert1 = tetrahedronEdgeConnection[i,1];
                float offset = GetOffset(tetrahedronValue[vert0], tetrahedronValue[vert1]);
                float invOffset = 1.0f - offset;

                edgeVertex[i].x = invOffset*tetrahedronPosition[vert0].x + offset*tetrahedronPosition[vert1].x;
                edgeVertex[i].y = invOffset*tetrahedronPosition[vert0].y + offset*tetrahedronPosition[vert1].y;
                edgeVertex[i].z = invOffset*tetrahedronPosition[vert0].z + offset*tetrahedronPosition[vert1].z;     
            }
	    }
		
	    for(int i = 0; i < 2; i++)
	    {
            if(tetrahedronTriangles[flagIndex,3*i] < 0) break;
		
            for(int j = 0; j < 3; j++)
            {
				int vertexIndex = tetrahedronTriangles[flagIndex, 3 * i + j];
				
				Vector3 p = edgeVertex[vertexIndex];
				System.Int64 hashCode = CalcHash(p);
				
				if (_hash.ContainsKey(hashCode) == false)
				{
					_hash.Add(hashCode, index);
					indexList.Add(index++);
					vertList.Add(p);
				}
				else
				{
					int beforeIdx = (int)_hash[hashCode];
					indexList.Add(beforeIdx);
				}
			}

	    }
	}
	
	System.Int64 CalcHash(Vector3 v)
	{
		unchecked
		{
			System.Int64 result = v.x.GetHashCode();
			result = (result * 397) ^ v.y.GetHashCode();
			result = (result * 397) ^ v.z.GetHashCode();
			return result;
		}
	}
	
	void FillCube(int x, int y, int z, float[, ,] voxels, ref float[] cube)
	{
		for (int i = 0; i < 8; i++)
			cube[i] = voxels[x + vertexOffset[i, 0], y + vertexOffset[i, 1], z + vertexOffset[i, 2]];
	}
	
	float GetOffset(float v1, float v2)
	{
		float delta = v2 - v1;
		return (delta == 0.0f) ? 0.5f : (_target - v1) / delta;
	}
		
	static int[,] vertexOffset = new int[,]
	{
		{0, 0, 0},{1, 0, 0},{1, 1, 0},{0, 1, 0},
		{0, 0, 1},{1, 0, 1},{1, 1, 1},{0, 1, 1}
	};

	static int[,] tetrahedronEdgeConnection = new int[,]
	{
	    {0,1},  {1,2},  {2,0},  {0,3},  {1,3},  {2,3}
	};

	static int[,] tetrahedronsInACube = new int[,]
	{
	    {0,5,1,6},
	    {0,1,2,6},
	    {0,2,3,6},
	    {0,3,7,6},
	    {0,7,4,6},
	    {0,4,5,6}
	};
	
	static int[] tetrahedronEdgeFlags = new int[]
	{
		0x00, 0x0d, 0x13, 0x1e, 0x26, 0x2b, 0x35, 0x38, 0x38, 0x35, 0x2b, 0x26, 0x1e, 0x13, 0x0d, 0x00
	};

	static int[,] tetrahedronTriangles = new int[,]
	{
        {-1, -1, -1, -1, -1, -1, -1},
        { 0,  3,  2, -1, -1, -1, -1},
        { 0,  1,  4, -1, -1, -1, -1},
        { 1,  4,  2,  2,  4,  3, -1},

        { 1,  2,  5, -1, -1, -1, -1},
        { 0,  3,  5,  0,  5,  1, -1},
        { 0,  2,  5,  0,  5,  4, -1},
        { 5,  4,  3, -1, -1, -1, -1},

        { 3,  4,  5, -1, -1, -1, -1},
        { 4,  5,  0,  5,  2,  0, -1},
        { 1,  5,  0,  5,  3,  0, -1},
        { 5,  2,  1, -1, -1, -1, -1},

        { 3,  4,  2,  2,  4,  1, -1},
        { 4,  1,  0, -1, -1, -1, -1},
        { 2,  3,  0, -1, -1, -1, -1},
        {-1, -1, -1, -1, -1, -1, -1}
	};
}
