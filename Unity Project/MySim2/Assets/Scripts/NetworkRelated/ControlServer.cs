using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.UIElements;

public struct MyPosture
{
    public struct MyPosition
    {
        public float x;
        public float y;
        public float z;
    }
    public MyPosition myPosition;
    public struct MyQuaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;
    }
    public MyQuaternion myQuaternion;
}

public class ControlServer : MonoBehaviour
{
    [Header("Control Server Config")]
    public String ServerHost = "127.0.0.1";
    public int ServerPort = 0;

    [Header("Control Target")]
    public GameObject Target;

    // no sample time, this is a mutual server, need send and recv

    [HideInInspector]
    TcpListener controlServer = null;
    Thread serverThread;
    TcpClient myClient = null;
    NetworkStream clientStream;

    MyPosture targetPosture;
    byte[] postureBytes;

    // Start is called before the first frame update
    void Start()
    {
        Assert.IsTrue(ServerPort != 0);
        if (Target == null)
        {
            Debug.Log("Control target not assigned");
        }
        Assert.IsNotNull(Target);

        serverThread = new Thread(DataThread);
        serverThread.Start();

        targetPosture = TransformToPosture(Target.GetComponent<Transform>());

        postureBytes = MarshalPosture(targetPosture);
        Debug.Log("posture bytes size:" + postureBytes.Length+ "Data:" + BitConverter.ToString(postureBytes));
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void DataThread()
    {
        try
        {
            IPAddress localAddr = IPAddress.Parse(ServerHost);
            // Create TCP listener
            controlServer = new TcpListener(IPAddress.Any, ServerPort);
            // Start listening for client requests
            controlServer.Start();

            while (true)
            {
                Debug.Log("Control-Server Waiting for a connection... ");

                // Perform a blocking call to accept requests.
                // could also use server.AcceptSocket()
                myClient = controlServer.AcceptTcpClient();

                if (myClient != null)
                {
                    Debug.Log("Control-Server Connected with a client!");
                }

                clientStream = myClient.GetStream();

                clientStream.Write(postureBytes, 0, postureBytes.Length);

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

            controlServer.Stop();
        }
    }

    private void OnDestroy()
    {
        serverThread.Abort();
    }

    private MyPosture TransformToPosture(Transform tf)
    {
        MyPosture res = new MyPosture();

        Vector3 p = tf.position;
        res.myPosition.x = p.x;
        res.myPosition.y = p.y;
        res.myPosition.z = p.z;

        Quaternion q = tf.rotation;
        res.myQuaternion.x = q.x;
        res.myQuaternion.y = q.y;
        res.myQuaternion.z = q.z;
        res.myQuaternion.w = q.w;

        return res;
    }

    private byte[] MarshalPosture(MyPosture p)
    {
        int size = Marshal.SizeOf(p);
        byte[] bytes = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(p, ptr, true);
        Marshal.Copy(ptr, bytes, 0, size);
        Marshal.FreeHGlobal(ptr);

        return bytes;
    }
    private MyPosture fromBytes(byte[] arr)
    {
        MyPosture p = new MyPosture();

        int size = Marshal.SizeOf(p);
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.Copy(arr, 0, ptr, size);

        p = (MyPosture)Marshal.PtrToStructure(ptr, p.GetType());
        Marshal.FreeHGlobal(ptr);

        return p;
    }
}
