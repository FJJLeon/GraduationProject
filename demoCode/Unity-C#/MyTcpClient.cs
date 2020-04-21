using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class MyTcpClient : MonoBehaviour
{
    [Header("Matlab Connect Settings")]
    public String MatlabServerAddress = "127.0.0.1";
    public int MatlabServerPort = 55001;

    [Header("Missile Camera Capture Settings")]
    public Camera MissileCamera;
    public int CaptureWidth = 1024;
    public int CaptureHeight = 768;
    public int CaptureFixedTimeGap = 2;
    public int FixedTimeCount = 0;
    // public string CaptureSaveAddress = "C:/capture/";

    // 截图尺寸
    public enum CaptureSize
    {
        CameraSize,
        ScreenResolution,
        FixedSize
    }
    // 目标摄像机
    public Camera targetCamera;
    // 截图尺寸
    public CaptureSize captureSize = CaptureSize.CameraSize;
    // 像素尺寸
    public Vector2 pixelSize;
    // 保存路径
    public string savePath = "picture/";
    // 文件名称
    public string fileName = "cameraCapture.png";




    [HideInInspector]
    TcpClient myClient;
    Thread myClientThread;

    Byte[] ScreenCaptureBytes;

    // Start is called before the first frame update
    void Start()
    {
        targetCamera = GetComponent<Camera>();
        pixelSize = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);

        myClientThread = new Thread(new ThreadStart(ClientThread));
        myClientThread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //Write("Hello!");

            Vector2 size = pixelSize;
            if (captureSize == CaptureSize.CameraSize)
            {
                size = new Vector2(targetCamera.pixelWidth, targetCamera.pixelHeight);
            }
            else if (captureSize == CaptureSize.ScreenResolution)
            {
                size = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
            }
            string path = Application.dataPath + "/" + savePath + fileName;
            saveTexture(path, capture(targetCamera, CaptureWidth, CaptureHeight));

        }
            
    }

    /// <summary> 相机截图 </summary>
    /// <param name="camera">目标相机</param>
    public Texture2D capture(Camera camera)
    {
        return capture(camera, Screen.width, Screen.height);
    }

    /// <summary> 相机截图 </summary>
    /// <param name="camera">目标相机</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    public Texture2D capture(Camera camera, int width, int height)
    {
        RenderTexture rt = new RenderTexture(width, height, 0);
        rt.depth = 24;
        rt.antiAliasing = 8;
        camera.targetTexture = rt;
        camera.RenderDontRestore();
        RenderTexture.active = rt;
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false, true);
        Rect rect = new Rect(0, 0, width, height);
        texture.ReadPixels(rect, 0, 0);
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        camera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
        return texture;
    }

    /// <summary> 保存贴图 </summary>
    /// <param name="path">保存路径</param>
    /// <param name="texture">Texture2D</param>
    public void saveTexture(string path, Texture2D texture)
    {
        //File.WriteAllBytes(path, texture.EncodeToPNG());
        Write(texture.EncodeToPNG());
        Write("ok");
#if UNITY_EDITOR
        Debug.Log("已保存截图到:" + path);
#endif
    }


    void ClientThread()
    {
        myClient = new TcpClient(MatlabServerAddress, MatlabServerPort);
        myClient.SendBufferSize = 102400;
        myClient.ReceiveBufferSize = 102400;
        if (myClient.Connected)
            print("Success to connect to matlab server!");
        else 
            return;

        //br = new BinaryReader(clientStream);
        Write("Hello!");

        while (myClient.Connected)
        {
            try
            {
                StreamReader sr = new StreamReader(myClient.GetStream());
                string receive = sr.ReadLine();
                print("Receive:" + receive);
                //Parse(receive);
                //Dispatch_write();
            }
            catch
            {
                print("tcpip connection cloesd!");
                myClient.Close();
            }
        }
        myClient.Close();
    }

    public void Write(byte[] bytes)
    {
        print(bytes);
        Debug.Log(bytes.Length);
        NetworkStream clientStream = myClient.GetStream();

        BinaryWriter bw = new BinaryWriter(clientStream);
        //bw.Write(bytes.Length);
        bw.Write(bytes);
        bw.Flush();
    }

    public void Write(String message)
    {
        byte[] byteArray = System.Text.Encoding.Default.GetBytes(message);
        Write(byteArray);
    }
}
