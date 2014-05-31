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

    int _w, _h, _l;

    void Start()
    {
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

                    voxelData[x, y, z] = _x * _x + _y * _y + _z * _z - radius * radius;
                    float _maxXY = Mathf.Max(_x * _x - 1, _y * _y - 1);
                    float _maxXYZ = Mathf.Max(_maxXY, _z * _z - 1);
                    //voxelData[x, y, z] = _maxXYZ;
                }
            }
        }

        Mesh cubeMesh = generator.CreateMesh(voxelData, new Vector3((float)width / 2.0f, (float)height / 2.0f, (float)length / 2.0f));
        cubeMesh.uv = new Vector2[cubeMesh.vertices.Length];
        cubeMesh.RecalculateNormals();

        GameObject child = new GameObject("0");
        child.AddComponent<MeshFilter>();
        child.AddComponent<MeshRenderer>();
        child.renderer.materials = new Material[]
		{
			material
		};
        child.GetComponent<MeshFilter>().mesh = cubeMesh;
        child.transform.parent = this.transform;

        _w = width;
        _h = height;
        _l = length;
        octree = new OcTree(_w, _h, _l,
                             new Bounds(Vector3.zero,
                                         new Vector3((float)(_w) * scale.x,
                                                      (float)(_h) * scale.y,
                                                      (float)(_l) * scale.z)
                    ));

        octree.Build();

    }

    public Object cubeObj;
    public GameObject temp;

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            foreach (Transform child in temp.transform)
            {
                Destroy(child.gameObject);
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            List<OcTree> nodeList = new List<OcTree>();
            List<float> dist = new List<float>();
            octree.find(Camera.main.ScreenPointToRay(Input.mousePosition), ref nodeList, ref dist, _w, _h, _l, voxelData);

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
                        OcTree.Calc3rdDimIdx(nodeList[i].corners[j], _w, _h, _l, out x, out y, out z);
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
                return;

            for (int i = 0; i < 8; ++i)
            {
                int x, y, z;
                OcTree.Calc3rdDimIdx(nodeList[si].corners[i], _w, _h, _l, out x, out y, out z);

                List<Vector3> lists = new List<Vector3>();
                generator.Test(x, y, z, new Vector3((float)width / 2.0f, (float)height / 2.0f, (float)length / 2.0f), voxelData, lists);

                for (int h = 0; h < lists.Count; ++h)
                {
                    GameObject newObj = Instantiate(cubeObj) as GameObject;
                    newObj.transform.localPosition = lists[h];
                    newObj.transform.parent = temp.transform;
                }
            }
        }
    }
}
