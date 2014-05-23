using UnityEngine;
using System.Collections;

public class Voxel : MonoBehaviour
{
    public Material material;

    public int width = 32;
    public int height = 32;
    public int length = 32;

    public float radius = 16.0f;

    void Start()
    {
		MarchingCubes generator = new MarchingCubes();
//		MarchingTetrahedra generator = new MarchingTetrahedra();

		generator.densityTarget = 0.0f;

        float[, ,] voxelData = new float[width, height, length];

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
                }
            }
        }

		Mesh cubeMesh = generator.CreateMesh (voxelData, new Vector3 ((float)width / 2.0f, (float)height / 2.0f, (float)length / 2.0f));
        cubeMesh.uv = new Vector2[cubeMesh.vertices.Length];
        cubeMesh.RecalculateNormals();

        GameObject child = new GameObject("0");
        child.AddComponent<MeshFilter>();
        child.AddComponent<MeshRenderer>();
        child.renderer.material = material;
        child.GetComponent<MeshFilter>().mesh = cubeMesh;
        child.transform.parent = this.transform;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
