using UnityEngine;
using System.Collections;

public class GetDepthBuffer : MonoBehaviour
{
	public Shader shader;
	private GameObject shaderCam = null;
	public RenderTexture output;
	
	// Use this for initialization
	void Start () {
		camera.depthTextureMode = DepthTextureMode.Depth;
		output = RenderTexture.GetTemporary ((int)camera.pixelWidth,
		                                     (int)camera.pixelHeight,
		                                     24,
		                                    RenderTextureFormat.ARGB32);
	}
	
	// Update is called once per frame
	void OnPostRender () {

		if (shaderCam == null)
		{
			shaderCam = new GameObject("ShaderCam");
			shaderCam.AddComponent<Camera>();
			shaderCam.camera.enabled = false;
			shaderCam.hideFlags = HideFlags.HideAndDontSave;
			shaderCam.camera.depthTextureMode = DepthTextureMode.Depth;
		}

		Camera cam = shaderCam.camera;
		cam.CopyFrom(camera);
		cam.backgroundColor = Color.white;
		cam.clearFlags = CameraClearFlags.SolidColor;
		cam.RenderWithShader(shader, "RenderType");

		camera.targetTexture = output;
	}

	void OnDisable()
	{
		DestroyImmediate (shaderCam);
	}
}
