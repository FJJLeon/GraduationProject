using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class MyTcpClient : MonoBehaviour
{
    [Header("Matlab Connect Config")]
    public String matlabServerHost = "localhost";
    public int matlabServerPort = 55001;

    [Header("Camera Capture Config")]
    public int resolutionWidth = 1024;
    public int resolutionHeight = 768;

    public int CaptureFixedTimeGap = 2;
    public int FixedTimeCount = 0;

    public bool isWriteImg2File = false;

    // public string CaptureSaveAddress = "C:/capture/";

    [HideInInspector]
    TcpClient myClient = null;
    NetworkStream myStream = null;
    StreamWriter myWriter = null;
    private const int SEND_RECEIVE_LENGTH = 4;

    Camera normalCamera;
    private Texture2D texture2D;
    private Rect rect;

    // Start is called before the first frame update
    void Start()
    {
        //Application.runInBackground = true;

        // get the Camera from GameObject who added this script
        normalCamera = this.GetComponent<Camera>();

        // TODO, what does it do
        //pixelSize = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
        InitGameObject();

        myClient = new TcpClient();
        if (SetupClient())
        {
            Debug.Log("client socket is set up");
        }

        // make SendRenderCamera called each time Camera finish rendering one frame
        // Camera.onPostRender += SendRenderedCamera;
    }

    // Update is called once per frame
    void Update()
    {
        if (!myClient.Connected)
        {// check TCP connection situation, re-setup if not connected
            SetupClient();
        }

        // make SendRenderCamera called once left mouse button clicked
        if (Input.GetMouseButtonDown(0))
        {
            SendRenderedCamera(normalCamera);
        }
        
    }

    private void OnApplicationQuit()
    {
        // cancel SendRenderedCamera call
        Camera.onPostRender -= SendRenderedCamera;
        
        if (myClient != null && myClient.Connected)
        {// close TCP connection
            myClient.Close();
        }
    }

    private void InitGameObject()
    {
        texture2D = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.RGB24, false);
        rect = new Rect(0, 0, resolutionWidth, resolutionHeight);
        // TODO: what does this do
        //normalCamera.targetTexture = new RenderTexture(resolutionWidth, resolutionHeight, 24);
    }

    // TODO: blog is public
    private bool SetupClient()
    {
        try
        {
            // connect matlab server @ matlabServerHost:matlabServerPort
            myClient.Connect(matlabServerHost, matlabServerPort);
            // init Stream and Writer for later transportation
            myStream = myClient.GetStream();
            myWriter = new StreamWriter(myStream);
            return true;
        }
        catch (Exception e)
        {
            Debug.Log("Client Socket Error: " + e);
            return false;
        }
    }


    // converts data size to byte array, put result to the byteData array
    private void byteLengthToFrameByteArray(int byteLength, byte[] byteData)
    {
        // clear
        Array.Clear(byteData, 0, byteData.Length);
        // convert int to bytes
        byte[] length2byte = BitConverter.GetBytes(byteLength);
        // convert byte array to Big Endian (Network Endian)
        Array.Reverse(length2byte);
        // copy result to byteData
        length2byte.CopyTo(byteData, 0);
    }

    public void SendRenderedCamera(Camera camera)
    {
        if (myClient == null || camera != normalCamera)
        {
            Debug.Log("Wrong camear assigned");
            return;
        }

        if (texture2D != null)
        {
            texture2D.ReadPixels(rect, 0, 0);
            Texture2D t2d = texture2D;

            byte[] bytedIMG = t2d.GetRawTextureData();

            // write to file if flag set
            if (isWriteImg2File)
            {
                string imgPath = Application.dataPath + @"/../" + normalCamera.name + DateTime.Now.Ticks.ToString() + @"WZNL.png";
                Debug.Log("Writing Camear frame to: " + imgPath);
                File.WriteAllBytes(imgPath, texture2D.EncodeToPNG());
            }

            // fill header info
            byte[] header = new byte[SEND_RECEIVE_LENGTH];
            byteLengthToFrameByteArray(bytedIMG.Length, header);

            try
            {
                // send header first
                if (myClient.Connected)
                {
                    myStream.Write(header, 0, header.Length);
                    Debug.Log("Send header length: " + header.Length + ", header: " + header);
                }

                // send image data
                if (myClient.Connected)
                {
                    myStream.Write(bytedIMG, 0, bytedIMG.Length);
                    Debug.Log("image frame send, length: " + bytedIMG.Length);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Send camera frame error: " + e);
            }
        }
    }


}
