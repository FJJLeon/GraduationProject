using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Unity.UIWidgets.rendering;
using UnityEngine;
using UnityEngine.Assertions;

public class guideUnity : MonoBehaviour
{
    [Header("Guide Client Congig")]
    public string GuideHost = "127.0.0.1";
    public int GuidePort = 56000;

    public GameObject car;
    public GameObject dataTran;
    public GameObject canvas;

    public Texture rgb;
    public Texture depth;
    public Texture panor;


    [HideInInspector]
    TcpClient myClient = null;
    NetworkStream clientStream;

    Thread guideThread;
    private int rt_width;
    private int rt_height;
    private const int channel = 3;
    private bool already_sim;
    private bool remote_start;
    // Start is called before the first frame update
    void Start()
    {
        if (car == null || dataTran == null || canvas == null || rgb == null || depth == null || panor == null)
        {
            Debug.Log("guide module missing gameobject or texture");
            Assert.IsTrue(false);
        }

        already_sim = false;

        myClient = new TcpClient();
        if (SetupClient())
        {
            Debug.Log("Guide client connected to guide server");
        }
        else
        {
            Assert.IsFalse(true);
        }
        rt_width = rgb.width;
        rt_height = rgb.height;

        remote_start = false;

        guideThread = new Thread(guide_thread);
        guideThread.Start();
    }

    void guide_thread()
    {
        byte[] sendbuffer;

        string role = "_Unity";
        sendbuffer = Encoding.Default.GetBytes(role);
        Debug.Log("buffer length:" + sendbuffer.Length);
        clientStream.Write(sendbuffer, 0, sendbuffer.Length);

        byte[] recvbuffer_1 = new byte[5];
        clientStream.Read(recvbuffer_1, 0, 5);
        string recv_1 = Encoding.UTF8.GetString(recvbuffer_1);
        Debug.Log("recv 1:" + recv_1);
        if (recv_1 == "CHECK")
        {
            Debug.Log("CEHCK cmd get");
        }
        else
        {
            Debug.Log("CHECK cmd wrong");
        }

        string resolv = String.Format("_Unity:[{0:D},{1:D},{2:D}]", rt_width, rt_height, channel);
        string send_1 = resolv.PadLeft(30, ' ');
        sendbuffer = Encoding.Default.GetBytes(send_1);
        Debug.Log("send_1: " + send_1 + " byte length:" + sendbuffer.Length);
        clientStream.Write(sendbuffer, 0, sendbuffer.Length);


        byte[] recvbuffer_2 = new byte[5];
        clientStream.Read(recvbuffer_2, 0, 5);
        string recv_2 = Encoding.UTF8.GetString(recvbuffer_2);
        Debug.Log("recv 2:" + recv_2);
        if (recv_2 == "START")
        {
            Debug.Log("START cmd get");
        }
        else
        {
            Debug.Log("START cmd wrong");
        }

        string response_ok = "_Unity:[OK]";
        string send_2 = response_ok.PadLeft(30, ' ');
        sendbuffer = Encoding.Default.GetBytes(send_2);
        Debug.Log("send_2: " + send_2 + " byte length:" + sendbuffer.Length);
        clientStream.Write(sendbuffer, 0, sendbuffer.Length);

        // ActivateSim();
        remote_start = true;
    }


    // Update is called once per frame
    void Update()
    {
        if ((Input.GetKeyDown("b") || remote_start == true) && already_sim == false)
        {
            ActivateSim();
        }
    }

    private void ActivateSim()
    {
        car.SetActive(true);
        dataTran.SetActive(true);
        canvas.SetActive(true);

        already_sim = true;
    }

    private bool SetupClient()
    {
        try
        {
            Debug.Log("Guide-Client setup to " + GuideHost + ":" + GuidePort);
            // connect to server @ matlabServerHost:matlabServerPort
            myClient.Connect(GuideHost, GuidePort);
            // init Stream and Writer for later transportation
            clientStream = myClient.GetStream();
            return true;
        }
        catch (Exception e)
        {
            Debug.Log("Guide-Client Socket Error: " + e);
            return false;
        }
    }
}
