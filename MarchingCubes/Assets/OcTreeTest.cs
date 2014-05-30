using UnityEngine;
using System.Collections;

public class OcTreeTest : MonoBehaviour
{

	void Start ()
    {
        OcTree t = new OcTree(4, 4, 4, new Bounds(Vector3.zero, new Vector3(100, 100, 100)));
        t.Build();
	}
}
