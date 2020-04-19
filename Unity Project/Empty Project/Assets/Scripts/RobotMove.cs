using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class RobotMove : MonoBehaviour
{
    [Header("Matlab Connect Config")]
    public String matlabServerHost = "localhost";
    public int matlabServerPort = 55002;

    Transform robotTF;

    TcpClient myClient;
    NetworkStream myStream;
    StreamReader myReader;

    bool moveFlag = false;
    Vector3 move;
    public int MoveSpeed = 1;

    // Start is called before the first frame update
    void Start()
    {
        robotTF = this.GetComponent<Transform>();

        myClient = new TcpClient();
        if (SetupClient())
        {
            Debug.Log("client socket is set up");
        }

        new Thread(ReceiveMessage).Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (!myClient.Connected)
        {// check TCP connection situation, re-setup if not connected
            SetupClient();
        }

        if (moveFlag)
        {
            robotTF.Translate(move * MoveSpeed);

            moveFlag = false;
        }
        
        
    }

    private bool SetupClient()
    {
        try
        {
            // connect matlab server @ matlabServerHost:matlabServerPort
            myClient.Connect(matlabServerHost, matlabServerPort);
            // init Stream and Writer for later transportation
            myStream = myClient.GetStream();
            myReader = new StreamReader(myStream);
            return true;
        }
        catch (Exception e)
        {
            Debug.Log("Client Socket Error: " + e);
            return false;
        }
    }

    private void ReceiveMessage()
    {
        while (true)
        {
            if (!myClient.Connected)
            {// check TCP connection situation, re-setup if not connected
                SetupClient();
            }

            byte[] data = new byte[5];
            int length = myStream.Read(data, 0, data.Length);
            string message = Encoding.UTF8.GetString(data, 0, length);
            Debug.Log("收到了消息：" + message);

            char direct = message[0];
            string stepS = message.Substring(2);
            int step = int.Parse(stepS);

            moveFlag = true;
            move = new Vector3(
                direct == 'x' ? step : 0,
                direct == 'y' ? step : 0,
                direct == 'z' ? step : 0);
            

        }
    }
}
