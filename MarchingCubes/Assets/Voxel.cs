﻿using UnityEngine;
using System.Collections;

public class Voxel : MonoBehaviour
{
    public Material material;
	public Material depthMaterial;

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

					//max((y*y-1),(x*x-1),(z*z-1)) 
                    voxelData[x, y, z] = _x * _x + _y * _y + _z * _z - radius * radius;
					float _maxXY = Mathf.Max(_x*_x-1, _y*_y-1);
					float _maxXYZ = Mathf.Max(_maxXY, _z*_z-1);
					//voxelData[x, y, z] = _maxXYZ;

					//if(voxelData[x,y,z] == 0)
					{
//						Debug.Log("x : "+x+" y : "+y+" z : "+z+" Voxel Data : "+voxelData[x,y,z]);
					}
                }
            }
        }

		Mesh cubeMesh = generator.CreateMesh (voxelData, new Vector3 ((float)width / 2.0f, (float)height / 2.0f, (float)length / 2.0f));
        cubeMesh.uv = new Vector2[cubeMesh.vertices.Length];
        cubeMesh.RecalculateNormals();

        GameObject child = new GameObject("0");
        child.AddComponent<MeshFilter>();
        child.AddComponent<MeshRenderer>();
		child.renderer.materials = new Material[]
		{
			depthMaterial, material
		};
        child.GetComponent<MeshFilter>().mesh = cubeMesh;
        child.transform.parent = this.transform;
//		child.transform.localScale = new Vector3 (10, 10, 10);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
