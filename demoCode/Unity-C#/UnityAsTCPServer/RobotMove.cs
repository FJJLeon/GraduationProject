using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;

public class RobotMove : MonoBehaviour
{
    [Header("Matlab Connect Config")]
    public String matlabServerHost = "127.0.0.1";
    public int matlabServerPort = 55002;

    Transform robotTF;

    #region TCP Connection
    /// <summary>
    /// TCP Server
    /// Note: Unity as TCP server, simulink Instrument Control client only
    /// </summary>
    TcpListener myServer;
    NetworkStream serverStream;

    TcpClient myClient;
    NetworkStream clientStream;
    StreamReader clientReader;
    #endregion

    bool moveFlag = false;
    Vector3 move;
    public int MoveSpeed = 5;

    // Start is called before the first frame update
    void Start()
    {
        robotTF = this.GetComponent<Transform>();

        /*
        myClient = new TcpClient();
        if (SetupClient())
        {
            Debug.Log("client socket is set up");
        }
        */
        new Thread(TcpServerThread).Start();
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (!myClient.Connected)
        {// check TCP connection situation, re-setup if not connected
            SetupClient();
        }
        */

        if (moveFlag)
        {
            robotTF.Translate(move * MoveSpeed);

            moveFlag = false;
        }
        
        
    }

    private void OnApplicationQuit()
    {
        if (myServer != null)
        {
            myServer.Stop();
        }
    }

    void TcpServerThread()
    {
        myServer = null;
        try
        {
            IPAddress localAddr = IPAddress.Parse(matlabServerHost);
            // Create TCP listener
            myServer = new TcpListener(IPAddress.Any, matlabServerPort);
            // Start listening for client requests
            myServer.Start();

            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            String message = null;

            // Enter listening loop
            while (true)
            {
                //Thread.Sleep(1);

                Debug.Log("Waiting for a connection... ");

                // Perform a blocking call to accept requests.
                // could also use server.AcceptSocket() here.
                myClient = myServer.AcceptTcpClient();
                if (myClient != null)
                {
                    Debug.Log("Connected with a client!");
                }

                clientStream = myClient.GetStream();
                StreamWriter clientWriter = new StreamWriter(clientStream);

                int len = 0;
                while ((len = clientStream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    message = System.Text.Encoding.UTF8.GetString(bytes, 0, len);
                    Debug.Log("get data length:" + len + ", data : " + message);

                    string[] cmd = message.Split(';');
                    for (int i = 0; i < cmd.Length; ++i)
                        Debug.Log(cmd[i]);
                    Assert.AreEqual(cmd.Length, 2);
                    char directX = cmd[0][0];
                    Assert.AreEqual(directX, 'x');
                    string stepX = cmd[0].Substring(2);
                    float x = float.Parse(stepX);

                    char directZ = cmd[1][0];
                    Assert.AreEqual(directZ, 'z');
                    string stepZ = cmd[1].Substring(2);
                    float z = float.Parse(stepZ);

                    moveFlag = true;
                    move = new Vector3(x, 0, z);
                }

                // End connection
                myClient.Close();
            }
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException in TCP server: " + e);
        }
        finally
        {
            // stop listening
            myServer.Stop();
        }
    }





    private bool SetupClient()
    {
        try
        {
            // connect matlab server @ matlabServerHost:matlabServerPort
            myClient.Connect(matlabServerHost, matlabServerPort);
            // init Stream and Writer for later transportation
            clientStream = myClient.GetStream();
            clientReader = new StreamReader(clientStream);
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

            byte[] data = new byte[31];
            int length = clientStream.Read(data, 0, data.Length);
            string message = Encoding.UTF8.GetString(data, 0, length);
            Debug.Log("收到了消息：" + message);


            /*
            char direct = message[0];
            string stepS = message.Substring(2);
            int step = int.Parse(stepS);

            moveFlag = true;
            move = new Vector3(
                direct == 'x' ? step : 0,
                direct == 'y' ? step : 0,
                direct == 'z' ? step : 0);
            */

        }
    }
}
