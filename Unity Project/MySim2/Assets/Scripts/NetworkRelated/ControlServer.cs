using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.UIWidgets.foundation;
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
    public float moveSpeed = 5f;
    public float rotateSpeed = 50f;
    float moveVertical;
    float moveHorizonal;

    private float moveInterval = 0;
    // no sample time, this is a mutual server, need send and recv

    [HideInInspector]
    TcpListener controlServer = null;
    Thread serverThread;
    TcpClient myClient = null;
    NetworkStream clientStream;

    MyPosture targetPosture;
    Transform targetTransform;
    byte[] postureBytes;
    Rigidbody targetRig;

    Queue<Command> recvCmdQueue = new Queue<Command>();

    // Start is called before the first frame update
    void Start()
    {
        moveVertical = 0;
        moveHorizonal = 0;

        Assert.IsTrue(ServerPort != 0);
        if (Target == null)
        {
            Debug.Log("Control target not assigned");
        }
        Assert.IsNotNull(Target);

        serverThread = new Thread(DataThread);
        serverThread.Start();

        targetRig = Target.GetComponent<Rigidbody>();

        targetTransform = Target.GetComponent<Transform>();
        targetPosture = TransformToPosture(targetTransform);
        postureBytes = MarshalPosture(targetPosture);
        // Debug.Log("posture bytes size:" + postureBytes.Length+ "Data:" + BitConverter.ToString(postureBytes));
    }

    // Update is called once per frame
    void Update()
    {
        targetPosture = TransformToPosture(targetTransform);
        postureBytes = MarshalPosture(targetPosture);

        if (!recvCmdQueue.isEmpty())
        {
            Command cmd = recvCmdQueue.Dequeue();
            moveVertical = cmd.cmdMove.vertical;
            moveHorizonal = cmd.cmdMove.horizonal;
        }

        moveInterval += Time.deltaTime;
        if (moveInterval > 0.25)// move per 0.05s 
        {
            moveInterval = 0;
            Target.transform.Translate(Vector3.forward * moveVertical * Time.deltaTime * moveSpeed);
            Target.transform.Rotate(Vector3.up * moveHorizonal * Time.deltaTime * rotateSpeed);
            
        }
        // Debug.Log("velocity x: " + targetRig.velocity.x + " y: " + targetRig.velocity.y + " z: " + targetRig.velocity.z);
    }


    private enum CmdTypeEnum
    {
        CMDTYPE_MOVE = 5000
    }
    public struct CmdBodyMove
    {
        public float vertical;
        public float horizonal;
    }
    public struct Command
    {
        public int type;
        public int length;
        public CmdBodyMove cmdMove;
    };

    void DataThread()
    {
        try
        {
            IPAddress localAddr = IPAddress.Parse(ServerHost);
            // Create TCP listener
            controlServer = new TcpListener(IPAddress.Any, ServerPort);
            // Start listening for client requests
            controlServer.Start();

            int readlen;
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

                while (myClient.Connected)
                {
                    /* send posture */
                    
                    clientStream.Write(postureBytes, 0, postureBytes.Length);
                    Debug.Log("posture bytes size:" + postureBytes.Length + "Data:" + BitConverter.ToString(postureBytes));

                    /* recv command */
                    byte[] intByte = new byte[sizeof(int)];
                    Command cmd = new Command();
                    // read tag
                    readlen = clientStream.Read(intByte, 0, sizeof(int));
                    Assert.AreEqual(readlen, sizeof(int));
                    cmd.type = BitConverter.ToInt32(intByte, 0);
                    Debug.Log("read type len: " + readlen + " type: " + cmd.type);
                    // read length
                    readlen = clientStream.Read(intByte, 0, sizeof(int));
                    Assert.AreEqual(readlen, sizeof(int));
                    cmd.length = BitConverter.ToInt32(intByte, 0);
                    Debug.Log("read length len: " + readlen + " length: " + cmd.length);
                    // switch read body
                    switch (cmd.type)
                    {
                        case (int)CmdTypeEnum.CMDTYPE_MOVE:
                            int size = Marshal.SizeOf(cmd.cmdMove);
                            byte[] moveBytes = new byte[size];

                            readlen = clientStream.Read(moveBytes, 0, size);
                            Assert.AreEqual(readlen, size);
                            cmd.cmdMove = UnmarshalCmdBodyMove(moveBytes);

                            Debug.Log("cmd type" + cmd.type + "length:" + cmd.length + "body: " + cmd.cmdMove.vertical + " " + cmd.cmdMove.horizonal);

                            recvCmdQueue.Enqueue(cmd);
                            break;
                        default:
                            break;
                    }
                    Thread.Sleep(400);
                }

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
    private MyPosture UnmarshalPosture(byte[] arr)
    {
        MyPosture p = new MyPosture();

        int size = Marshal.SizeOf(p);
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.Copy(arr, 0, ptr, size);

        p = (MyPosture)Marshal.PtrToStructure(ptr, p.GetType());
        Marshal.FreeHGlobal(ptr);

        return p;
    }

    private CmdBodyMove UnmarshalCmdBodyMove(byte[] arr)
    {
        CmdBodyMove p = new CmdBodyMove();

        int size = Marshal.SizeOf(p);
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.Copy(arr, 0, ptr, size);

        p = (CmdBodyMove)Marshal.PtrToStructure(ptr, p.GetType());
        Marshal.FreeHGlobal(ptr);

        return p;
    }
}
