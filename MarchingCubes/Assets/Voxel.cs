using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Voxel : MonoBehaviour
{
    public Material material;

    public int width = 32;
    public int height = 32;
    public int length = 32;

    public float radius = 16.0f;

    public Vector3 scale = new Vector3(1, 1, 1);

    OcTree octree;
    MarchingCubes generator;
    float[, ,] voxelData;

	Mesh mesh;

    void Start()
    {
        generator = new MarchingCubes(0.0f);
        voxelData = new float[width, height, length];
		CustomPerlinNoise noise = new CustomPerlinNoise (0xee, 16, 0.5f, 4.0f, new Vector3 (width, height, length));

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < length; z++)
                {
                    float _x = (float)x - (float)width / 2.0f;
                    float _y = (float)y - (float)height / 2.0f;
                    float _z = (float)z - (float)length / 2.0f;

                    //voxelData[x, y, z] = _x * _x + _y * _y + _z * _z - radius * radius;
					voxelData[x, y, z] = noise.Get(x, y, z);
                }
            }
        }

        Mesh cubeMesh = generator.CreateMesh(voxelData, new Vector3((float)width / 2.0f, (float)height / 2.0f, (float)length / 2.0f));
        cubeMesh.uv = new Vector2[cubeMesh.vertices.Length];
        cubeMesh.RecalculateNormals();
		mesh = cubeMesh;

        GameObject child = new GameObject("0");
        child.AddComponent<MeshFilter>();
        child.AddComponent<MeshRenderer>();
        child.GetComponent<Renderer>().materials = new Material[]
		{
			material
		};
        child.GetComponent<MeshFilter>().mesh = cubeMesh;
        child.transform.parent = this.transform;

        octree = new OcTree(width, height, length,
                             new Bounds(Vector3.zero,
                                         new Vector3((float)(width) * scale.x,
                                                      (float)(height) * scale.y,
                                                      (float)(length) * scale.z)
                    ));

        octree.Build();

    }

    public Object cubeObj;
	public Object cubeObj2;

	public GameObject temp;

	bool PickingTestOne()	
	{
		List<OcTree> nodeList = new List<OcTree>();
		List<float> dist = new List<float>();
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		octree.find(ray, ref nodeList, ref dist, width, height, length, voxelData);
		
		int si = 0;
		float sd = 1000000000;
		int count = dist.Count;
		bool find = false;
		for (int i = 0; i < count; ++i)
		{
			if (sd > dist[i])
			{
				for (int j = 0; j < 8; ++j)
				{
					int x, y, z;
					OcTree.Calc3rdDimIdx(nodeList[i].corners[j], width, height, length, out x, out y, out z);
					if (voxelData[x, y, z] < generator.densityTarget)
					{
						sd = dist[i];
						si = i;
						find = true;
						break;
					}
				}
			}
		}
		
		if (find == false)
			return false;
		
		for (int i = 0; i < 8; ++i)
		{
			int x, y, z;
			OcTree.Calc3rdDimIdx(nodeList[si].corners[i], width, height, length, out x, out y, out z);
			
			List<Vector3> lists = new List<Vector3>();
			generator.Test(x, y, z, new Vector3((float)width / 2.0f, (float)height / 2.0f, (float)length / 2.0f), voxelData, lists);

			//Bugㅇㅣㅆㅇ...
			for(int h=0; h<lists.Count; h+=3)
			{
				float u, v, t;
				if(IntersectTriangle(ray, lists[h+0], lists[h+1], lists[h+2], out u, out v, out t))
				{
					Debug.Log("Check");
					Vector3 pos = lists[h] 	+ u * (lists[h+1] - lists[h]) 
											+ v * (lists[h+2] - lists[h]); 
					
					GameObject newObj = Instantiate(cubeObj2) as GameObject;
					newObj.transform.localPosition = pos;
					newObj.transform.parent = temp.transform;
					break;
				}
			}

			//Bug..
			for (int h = 0; h < lists.Count; ++h)
			{
				GameObject newObj = Instantiate(cubeObj) as GameObject;
				newObj.transform.localPosition = lists[h];
				newObj.transform.parent = temp.transform;
			}
		}

		return false;
	}

	bool PickingTestTwo()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		int[] indices = mesh.GetIndices (0);
		Vector3[] vertices = mesh.vertices;

		int size = indices.Length;	
		for (int i=0; i<size; i+=3)
		{
			Vector3 p0 = vertices[ indices[i] ];
			Vector3 p1 = vertices[ indices[i+1] ];
			Vector3 p2 = vertices[ indices[i+2] ];

			float u,v,t;
			if( IntersectTriangle(ray, p0, p1, p2, out u, out v, out t) )
			{
				return true;
			}
		}

		return false;
	}
	
	void Update()
	{
		if (Input.GetMouseButtonDown(1))
		{

		}
		if (Input.GetMouseButtonDown(0))
		{
			PickingTestOne();
		}
	}
	
	bool IntersectTriangle(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2, out float u, out float v, out float t)
	{
		u = v = t = -1;
		
		Vector3 edge1 = v1 - v0;
		Vector3 edge2 = v2 - v0;
		
		Vector3 pvec = Vector3.Cross (ray.direction, edge2);
		
		float determinant = Vector3.Dot (edge1, edge2);
		
		Vector3 tvec;
		if( determinant > 0.0f )
		{
			tvec = ray.origin - v0;
		}
		else
		{
			tvec = v0 - ray.origin;
			determinant = -determinant;
		}

		if (determinant < float.Epsilon)
			return false;

		u = Vector3.Dot (tvec, pvec);
		if (u < 0.0f || u > determinant)
			return false;

		Vector3 qvec = Vector3.Cross (tvec, edge1);

		v = Vector3.Dot (ray.direction, qvec);
		if (v < 0.0f || (u + v) > determinant)
			return false;

		t = Vector3.Dot (edge2, qvec);
		float invDet = 1.0f / determinant;
		t *= invDet;
		u *= invDet;
		v *= invDet;

		return true;
	}
}
