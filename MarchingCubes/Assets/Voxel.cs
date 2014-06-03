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
    	//target은 0으로
        generator = new MarchingCubes(0.0f);
        voxelData = new float[width, height, length];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < length; z++)
                {
                    float _x = (float)x - (float)width / 2.0f;
                    float _y = (float)y - (float)height / 2.0f;
                    float _z = (float)z - (float)length / 2.0f;

		    //구 생성 공식.  http://www.econym.demon.co.uk/isotut/simple.htm 참고
                    voxelData[x, y, z] = _x * _x + _y * _y + _z * _z - radius * radius;
                }
            }
        }

	//원점 유지를 위해 오프셋 값 넘겨줌
        Mesh cubeMesh = generator.CreateMesh(voxelData, new Vector3((float)width / 2.0f, (float)height / 2.0f, (float)length / 2.0f));
        cubeMesh.uv = new Vector2[cubeMesh.vertices.Length];
        cubeMesh.RecalculateNormals();
	mesh = cubeMesh;

	//복셀 오브젝트 생성
        GameObject child = new GameObject("0");
        child.AddComponent<MeshFilter>();
        child.AddComponent<MeshRenderer>();
        child.renderer.materials = new Material[]
	{
		material
	};
        child.GetComponent<MeshFilter>().mesh = cubeMesh;
        child.transform.parent = this.transform;

	//옥트리 생성
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
		//Ray Cast With Octree
		
		List<OcTree> nodeList = new List<OcTree>();
		List<float> dist = new List<float>();
		
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		//광선에 검출된 모든 옥트리 노드들 얻어옴.
		//추가로 카메라와 노드 사이의 거리를 가진 리스트도 얻어옴
		octree.find(ray, ref nodeList, ref dist, width, height, length, voxelData);
		
		int si = 0;
		float sd = 1000000000;
		int count = dist.Count;
		bool find = false;
		
		//가까운 거리 검색
		for (int i = 0; i < count; ++i)
		{
			if (sd > dist[i])
			{
				for (int j = 0; j < 8; ++j)
				{
					int x, y, z;
					OcTree.Calc3rdDimIdx(nodeList[i].corners[j], width, height, length, out x, out y, out z);
					
					//밀도 값(isosurface 값?)이 미리 지정한 target값 보다 작다면,
					//표면 내부 이므로 표면에 충돌 되었다고 검출
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
		
		//8인 이유는, 옥트리 코너가 8개니깐?
		for (int i = 0; i < 8; ++i)
		{
			int x, y, z;
			// 코너 인덱스 값을 x, y, z로 풀어 씀
			OcTree.Calc3rdDimIdx(nodeList[si].corners[i], width, height, length, out x, out y, out z);
			
			List<Vector3> lists = new List<Vector3>();
			
			//
			generator.Test(x, y, z, new Vector3((float)width / 2.0f, (float)height / 2.0f, (float)length / 2.0f), voxelData, lists);

			//삼각형 검출 로직. lists에는 인덱스 순서가 들어옴
			for(int h=0; h<lists.Count; h+=3)
			{
				float u, v, t;
				
				//ray와 삼각형 검출 함수. 
				if(IntersectTriangle(ray, lists[h+0], lists[h+1], lists[h+2], out u, out v, out t))
				{
					Debug.Log("Check");
					Vector3 pos = lists[h] 	+ u * (lists[h+1] - lists[h]) 
											+ v * (lists[h+2] - lists[h]); 
					
					//테스트용 파란 박스
					GameObject newObj = Instantiate(cubeObj2) as GameObject;
					newObj.transform.localPosition = pos;
					newObj.transform.parent = temp.transform;
					break;
				}
			}

			// 테스트용. 선택한 표면 주변의 버텍스들 출력
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
		// 그냥 인덱스만 쭉 돌면서 ray와 충돌된 삼각형 검출
		
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
		// 참고 : http://blog.daum.net/gamza-net/8
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
