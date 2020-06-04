using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class snapthree : MonoBehaviour
{
    public RenderTexture rgb;
    public RenderTexture depth;
    public RenderTexture panor;

    // Start is called before the first frame update
    void Start()
    {
        Assert.IsNotNull(rgb);
        Assert.IsNotNull(depth);
        Assert.IsNotNull(panor);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("p"))
        {
            Snap();
        }
        
    }

    private void Snap()
    {
        snapOne(rgb, "./snapImages/rgb.png");
        snapOne(depth, "./snapImages/depth.png");
        snapOne(panor, "./snapImages/panor.png");
    }

    private void snapOne(RenderTexture rt, string path_file)
    {
        RenderTexture currentRT = RenderTexture.active;   // save current active rendertexture
        RenderTexture.active = rt;
        Debug.Log("rt's resolution: " + rt.width + "*" + rt.height);
        Texture2D image = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        image.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        image.Apply();

        savePNG(image, path_file);
    }
    private void savePNG(Texture2D image, string path_file)
    {
        // store the texture into a .PNG file
        byte[] bytes = image.EncodeToPNG();

        // save the encoded image to a file
        System.IO.File.WriteAllBytes(path_file, bytes);
    }
}
