using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]   // 可以在编辑界面看到全景绘制效果
public class MonoPanorRender : MonoBehaviour
{
    [Header("Target Render Texture")]
    public RenderTexture cubemapLeft;
    public RenderTexture cubemapRight;
    public RenderTexture equirect;

    public bool renderStereo = false;
    public float stereoSeparation = 0.064f;

    private Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam =this.GetComponent<Camera>();
        if (cam == null)
        {
            Debug.Log("stereo 360 capture node has no camera or parent camera");
        }
    }

    void LateUpdate()
    {
        // render panoramic RT
        // stereo render 
        if (renderStereo)
        {
            cam.stereoSeparation = stereoSeparation;
            cam.RenderToCubemap(cubemapLeft, 63, Camera.MonoOrStereoscopicEye.Left);
            cam.RenderToCubemap(cubemapRight, 63, Camera.MonoOrStereoscopicEye.Right);
        }
        // mono render
        else
        {
            cam.RenderToCubemap(cubemapLeft, 63, Camera.MonoOrStereoscopicEye.Mono);
        }
        // convet cubemaps to equirect if equirect RT is set
        if (equirect == null)
        {
            return;
        }
        // stereo render 
        if (renderStereo)
        {
            cubemapLeft.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Left);
            cubemapRight.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Right);
        }
        // mono render
        else
        {
            cubemapLeft.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Mono);
        }
    }
}
