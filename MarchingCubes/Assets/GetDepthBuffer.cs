using UnityEngine;
using System.Collections;

public class GetDepthBuffer : MonoBehaviour
{
	public Shader shader;
	private GameObject shaderCam = null;
	public RenderTexture output;
	
	// Use this for initialization
	void Start () {
		GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
		output = RenderTexture.GetTemporary ((int)GetComponent<Camera>().pixelWidth,
		                                     (int)GetComponent<Camera>().pixelHeight,
		                                     24,
		                                    RenderTextureFormat.ARGB32);
	}
	
	// Update is called once per frame
	void OnPostRender () {

		if (shaderCam == null)
		{
			shaderCam = new GameObject("ShaderCam");
			shaderCam.AddComponent<Camera>();
			shaderCam.GetComponent<Camera>().enabled = false;
			shaderCam.hideFlags = HideFlags.HideAndDontSave;
			shaderCam.GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
		}

		Camera cam = shaderCam.GetComponent<Camera>();
		cam.CopyFrom(GetComponent<Camera>());
		cam.backgroundColor = Color.white;
		cam.clearFlags = CameraClearFlags.SolidColor;
		cam.RenderWithShader(shader, "RenderType");

		GetComponent<Camera>().targetTexture = output;
	}

	void OnDisable()
	{
		DestroyImmediate (shaderCam);
	}
}
