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
					//Get the values in the 8 neighbours which make up a cube
					FillCube(x, y, z, voxels, ref cube);

					//Perform algorithm
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
		
		//Make a local copy of the cube's corner positions
		for(i = 0; i < 8; i++) cubePosition[i] = new Vector3( pos.x + vertexOffset[i,0], pos.y + vertexOffset[i,1], pos.z + vertexOffset[i,2]);
		
		for(i = 0; i < 6; i++)
		{
	        for(j = 0; j < 4; j++)
	        {
                vertexInACube = tetrahedronsInACube[i,j];
                tetrahedronPosition[j] = cubePosition[vertexInACube];
                tetrahedronValue[j] = cube[vertexInACube];
	        }
			
	        MarchTetrahedron(tetrahedronPosition, tetrahedronValue, vertList, indexList);
		}
	}

	void MarchTetrahedron(Vector3[] tetrahedronPosition, float[] tetrahedronValue, List<Vector3> vertList, List<int> indexList)
	{
	
	    //Find which vertices are inside of the surface and which are outside
		int flagIndex = 0;	
		for (int i = 0; i < 4; i++)
		{
			if (tetrahedronValue [i] > _target)
				flagIndex |= 1 << i;
		}

		//Find which edges are intersected by the surface
	    int edgeFlags = tetrahedronEdgeFlags[flagIndex];
	
	    //If the tetrahedron is entirely inside or outside of the surface, then there will be no intersections
	    if(edgeFlags == 0) return;

	    //Find the point of intersection of the surface with each edge
		Vector3[] edgeVertex = new Vector3[6];
		for(int i = 0; i < 6; i++)
	    {
            //if there is an intersection on this edge
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
		
	    //Save the triangles that were found. There can be up to 2 per tetrahedron
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

	// tetrahedronEdgeConnection lists the index of the endpoint vertices for each of the 6 edges of the tetrahedron
	// tetrahedronEdgeConnection[6][2]
	static int[,] tetrahedronEdgeConnection = new int[,]
	{
	    {0,1},  {1,2},  {2,0},  {0,3},  {1,3},  {2,3}
	};

	// tetrahedronEdgeConnection lists the index of verticies from a cube 
	// that made up each of the six tetrahedrons within the cube
	// tetrahedronsInACube[6][4]
	
	static int[,] tetrahedronsInACube = new int[,]
	{
	    {0,5,1,6},
	    {0,1,2,6},
	    {0,2,3,6},
	    {0,3,7,6},
	    {0,7,4,6},
	    {0,4,5,6}
	};
	
	// For any edge, if one vertex is inside of the surface and the other is outside of the surface
	//  then the edge intersects the surface
	// For each of the 4 vertices of the tetrahedron can be two possible states : either inside or outside of the surface
	// For any tetrahedron the are 2^4=16 possible sets of vertex states
	// This table lists the edges intersected by the surface for all 16 possible vertex states
	// There are 6 edges.  For each entry in the table, if edge #n is intersected, then bit #n is set to 1
	// tetrahedronEdgeFlags[16]

	static int[] tetrahedronEdgeFlags = new int[]
	{
		0x00, 0x0d, 0x13, 0x1e, 0x26, 0x2b, 0x35, 0x38, 0x38, 0x35, 0x2b, 0x26, 0x1e, 0x13, 0x0d, 0x00
	};


	// For each of the possible vertex states listed in tetrahedronEdgeFlags there is a specific triangulation
	// of the edge intersection points.  tetrahedronTriangles lists all of them in the form of
	// 0-2 edge triples with the list terminated by the invalid value -1.
	// tetrahedronTriangles[16][7]

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
