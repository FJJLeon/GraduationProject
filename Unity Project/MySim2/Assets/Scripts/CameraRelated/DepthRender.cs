using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]   // 可以在编辑界面看到深度绘制效果
public class DepthRender : MonoBehaviour
{
    public Material mat;
    //public int width = 512;
    //public int height = 512;

    private Camera targetCam;
    private RenderTexture rt;

    // Start is called before the first frame update
    void Start()
    {
        targetCam = this.GetComponent<Camera>();   //获取当前绑定到脚本的相机

        targetCam.depthTextureMode = DepthTextureMode.Depth; // 手动设置相机，使得其提供场景的深度信息
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, mat);
        //mat is the material which contains the shader we are passing the destination RenderTexture to
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
