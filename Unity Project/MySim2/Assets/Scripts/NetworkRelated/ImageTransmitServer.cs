using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;

public class ImageTransmitServer : MonoBehaviour
{
    public bool Enable = true;

    [Header("Transmition Config")]
    public String ServerHost = "127.0.0.1";

    public enum ImageTypeEnum
    {
        RGB = 0,
        Depth,
        Panoramic
    }
    private String[] ImageTypeString = {"RGB", "Depth", "Panoramic" };

    public ImageTypeEnum ImageType = ImageTypeEnum.RGB;

    public int ServerPort = 0;

    [Header("Image Config")]
    public Texture Image;
    public int ResolutionWidth = 640;
    public int ResolutionHeight = 480;

    [Header("Sample Time")]
    [Tooltip("image server send interval / ms")]
    public int SampleTime = 1000;

    [HideInInspector]
    TcpListener imageServer = null;
    TcpClient myClient = null;
    NetworkStream clientStream;
    StreamWriter clientWriter;

    // Texture to byte
    private Texture2D image2D;
    private Rect rect;
    byte[] bytedIMG;
    bool consumed;
    bool first;

    // Start is called before the first frame update
    void Start()
    {
        Assert.IsNotNull(Image);
        if (ImageType != ImageTypeEnum.RGB && ImageType != ImageTypeEnum.Depth && ImageType != ImageTypeEnum.Panoramic)
        {
            Debug.Log("ImageType Mismatch");
            Assert.IsTrue(false);
        }
        if (Image.width != ResolutionWidth || Image.height != ResolutionHeight)
        {
            Debug.Log(ImageTypeString[(int)ImageType] + "-Server Texture size mismatch");
            Assert.IsTrue(false);
        }
        Assert.IsTrue(ServerPort != 0);
        // mark bytedIMG as not consumed
        consumed = true;

        new Thread(ImageServerThread).Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (consumed)
        {
            image2D = TextureToTexture2D(Image);
            
            bytedIMG = image2D.GetRawTextureData();
            Assert.AreEqual(bytedIMG.Length, ResolutionWidth * ResolutionHeight * 3);
            // mark as consumed
            consumed = false;
        }
    }

    void LateUpdate()
    {
        
    }

    private Texture2D TextureToTexture2D(Texture texture)
    {
        Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
        Graphics.Blit(texture, renderTexture);

        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(renderTexture);
        return texture2D;
    }

    void ImageServerThread()
    {
        imageServer = null;
        try
        {
            IPAddress localAddr = IPAddress.Parse(ServerHost);
            // Create TCP listener
            imageServer = new TcpListener(IPAddress.Any, ServerPort);
            // Start listening for client requests
            imageServer.Start();

            // Enter listening loop
            while (true)
            {
                Debug.Log(ImageTypeString[(int)ImageType] + "-Server Waiting for a connection... ");

                // Perform a blocking call to accept requests.
                // could also use server.AcceptSocket()
                myClient = imageServer.AcceptTcpClient();

                if (myClient != null)
                {
                    Debug.Log(ImageTypeString[(int)ImageType] + "-Server Connected with a client!");
                }

                clientStream = myClient.GetStream();
                clientWriter = new StreamWriter(clientStream);

                
                while (myClient.Connected)
                {
                    clientStream.Write(bytedIMG, 0, bytedIMG.Length);
                    Debug.Log(ImageTypeString[(int)ImageType] + "-Server send image ok, length: " + bytedIMG.Length);
                    // mark as consumed
                    consumed = true;

                    Thread.Sleep(SampleTime);
                }

                // End connection
                myClient.Close();
            }
        } 
        catch (SocketException e)
        {
            Debug.Log("SocketException in image server: " + e);
        }
        finally
        {
            // stop listening
            if (myClient != null && myClient.Connected)
            {
                myClient.Close();
            }
            
            imageServer.Stop();
        }
    }

    private void OnApplicationQuit()
    {
        if (myClient != null && myClient.Connected)
        {// close TCP connection
            myClient.Close();
        }
    }
}
