using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapDepth : MonoBehaviour
{
	public Material mat;
	public int width = 640;
	public int height = 480;

	private Camera cam;
	private RenderTexture rt;
	private int image_id = 0;
	private bool SnapFlag = false;

	void Start()
	{
		cam = GetComponent<Camera>();   //获取当前绑定到脚本的相机

		cam.depthTextureMode = DepthTextureMode.Depth;

		rt = new RenderTexture(width, height, 24);  // 24 bit depth
		cam.targetTexture = rt;
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, mat);
		//mat is the material which contains the shader
		//we are passing the destination RenderTexture to

		if (SnapFlag)
		{
			RenderTexture currentRT = RenderTexture.active;   // save current active rendertexture
			RenderTexture.active = destination;
			Debug.Log("dest's resolution: " + destination.width + "*" + destination.height);
			Texture2D image = new Texture2D(destination.width, destination.height, TextureFormat.Alpha8, false);
			image.ReadPixels(new Rect(0, 0, destination.width, destination.height), 0, 0);

			image.Apply();

			savePNG(image, "./depthImages/camera_image" + image_id + ".png");
			Debug.Log("a depth imag saved");

			image_id++;
			RenderTexture.active = currentRT; // restore 
		}
	}

	private void savePNG(Texture2D image, string path_file)
	{
		// store the texture into a .PNG file
		byte[] bytes = image.EncodeToPNG();

		// save the encoded image to a file
		System.IO.File.WriteAllBytes(path_file, bytes);
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown("p"))
		{
			SnapFlag = true;
		}
		else
			SnapFlag = false;
	}

}
