using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;
using System;
using System.Threading;
 
public class TCPserver:MonoBehaviour{
    public bool isConnection,senddata1,senddata2,valuechanged,valuechanged2;
    //public bool isZoomed,isPointsSet,isZooming,Zoompressed,rotatearound1,rotatearound2,rotatearound3,rotatearound4,distanceChange1,distanceChange2;
    public List<string> stairways=new List<string>();
    public List<bool> floorsBool = new List<bool> ();
    public List<bool> roomsBool = new List<bool> ();
    public List<bool> stairwaysBool = new List<bool> ();
    public List<int> rooms = new List<int> ();
    public List<GameObject> flats = new List<GameObject> ();
    public int floors;
    Zoom zoom;
    ShowAndHide show;
    public Thread mThread;
    public TcpListener server;
    public TcpClient client;
    public NetworkStream stream;
    byte[] msg1;
    string toSend;
    bool available,hide,show1,selection,endpointChanged,parsedata;
    public String data,prevdata;
    float frametime=0.0f;
    int q=0;
    int FPS=0;
    GameObject building;
    Vector3 pos;
    Sun1 sun;
    WindowLight wl;
    GameObject shader;
    // Use this for initialization
    void Start(){
        shader=GameObject.FindGameObjectWithTag ("Shader");
        building=GameObject.FindGameObjectWithTag("Building");
        sun = GameObject.FindGameObjectWithTag ("Light").GetComponent<Sun1> ();
        wl = GameObject.FindGameObjectWithTag ("Light").GetComponent<WindowLight> ();
        flats = getFlats ();
        floorsBool=InitializeFloorsBool ();
        roomsBool = InitializeRoomsBool ();
        stairwaysBool = InitializeStairwaysBool ();
        zoom = Camera.main.GetComponent<Zoom> ();
        show = Camera.main.GetComponent<ShowAndHide> ();
        isConnection=false;
        senddata1 = false;
        senddata2 = false;
        //print ("StartThread");
        ThreadStart ts = new ThreadStart(Update1);
        mThread = new Thread(ts);
        mThread.Start();
    }
    IEnumerator startThings(){
        for(int j=0;j<flats.Count;j++){
           
            StartCoroutine(parseName(flats[j]));
            yield return null;
        }
        yield return null;
    }
    // Update is called once per frame
    void Update(){
        //print (data);
        //FPScounter();
        if(hide){
            //print ("Hide:"+building.name);
            StartCoroutine(show.Hide(building,4,5.8f,6.5f,0.0025f));
            StartCoroutine(show.Hide(building,3,6.1f,6.4f,0.0025f));
            StartCoroutine(show.Hide(building,2,5.8f,6.1f,0.0025f));
            StartCoroutine(show.Hide(building,1,6.1f,6.4f,0.0025f));
            StartCoroutine(show.Hide(building,0,5.8f,6.4f,0.0025f));
            hide=false;
            for(int i=0;i<533;i++){
                wl.windows[i].SetActive(false);
            }
            shader.SetActive(false);
        }
        if(show1){
            //print ("Hide:"+building.name);
            StartCoroutine(show.Show(building,4,5.8f,6.5f,0.0025f));
            StartCoroutine(show.Show(building,3,6.1f,6.4f,0.0025f));
            StartCoroutine(show.Show(building,2,5.8f,6.1f,0.0025f));
            StartCoroutine(show.Show(building,1,6.1f,6.4f,0.0025f));
            StartCoroutine(show.Show(building,0,5.8f,6.4f,0.0025f));
            show1=false;
 
        }
        if (parsedata) {
            parsedata=false;
            StartCoroutine(parseData(prevdata));
        }
        if(selection){
            selection=false;
            StartCoroutine(startThings());
        }
        if(senddata1){
            senddata1=false;
            ZoomIn();
        }
        if(senddata2){
            senddata2=false;
            ZoomBack();
        }
        if(valuechanged){
            toSend =zoom.isZooming.ToString()+",";
            prevdata=toSend;
            //print (toSend);
            msg1 = System.Text.Encoding.ASCII.GetBytes(toSend);
            stream.Write (msg1, 0, msg1.Length);
            valuechanged=false;
        }
        if(valuechanged2){
            toSend =zoom.isZoomed.ToString()+":";
            prevdata=toSend;
            //print (toSend);
            msg1 = System.Text.Encoding.ASCII.GetBytes(toSend);
            stream.Write (msg1, 0, msg1.Length);
            valuechanged2=false;
        }
        if(endpointChanged){
            endpointChanged=false;
            zoom.Zoompoint.position=pos;
            zoom.Zoompoint.rotation=Quaternion.Euler(40.0f,275.0f,0.0f);
            zoom.rotateAroundObject.position=new Vector3(pos.x-0.498778f,pos.y-0.423f,pos.z-0.074f);
        }
    }
    void FPScounter(){
        frametime+=Time.deltaTime;
        q++;
        if(frametime>=1.0f){
            FPS=q;
            //print ("FPS:"+FPS);
            q=0;
            frametime=0.0f;
        }
    }
    void OnApplicationQuit(){
        server.Stop();
        mThread.Abort ();
    }
    void Update1()
    {  
        server=null;  
        try
        {
            // Set the TcpListener on port 13000.
            Int32 port = 3333;
            IPAddress localAddr = IPAddress.Parse("192.168.0.242");
           
            // TcpListener server = new TcpListener(port);
            server = new TcpListener(IPAddress.Any, port);
           
            // Start listening for client requests.
            server.Start();
           
            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            String data = null;
           
            // Enter the listening loop.
            while(true)
            {
                Thread.Sleep(10);
               
                Debug.Log("Waiting for a connection... ");
               
                // Perform a blocking call to accept requests.
                // You could also user server.AcceptSocket() here.
                client = server.AcceptTcpClient();  
                if(client!=null){
                   
                    Debug.Log("Connected!");
                    //isConnection=true;
                    //client.Close();
                    //break;
                   
                }
                data = null;
               
                // Get a stream object for reading and writing
                stream = client.GetStream();
                StreamWriter swriter=new StreamWriter(stream);
                int i;
               
                // Loop to receive all the data sent by the client.
                while((i = stream.Read(bytes, 0, bytes.Length))!=0)
                {  
                    //msg1 = System.Text.Encoding.ASCII.GetBytes(prevdata);
                   
                    // Send back a response.
                    //stream.Write(msg1, 0, msg1.Length);
                    // Translate data bytes to a ASCII string.
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    //Debug.Log("Received:"+ data+"data");
                   
                   
                    //Debug.Log("Sent:"+ data);
                    // Process the data sent by the client.
                    bool isTrue=false;
                    switch (data)
                    {
                    case "params":
                        //senddata=true;
                        string stairways1 = Stairways ();
                        string rooms1 = Rooms ();
                        toSend = floors.ToString () + "/" + stairways1 + "/" + rooms1;
                        //print (toSend);
                        prevdata=toSend;
                        msg1 = System.Text.Encoding.ASCII.GetBytes(toSend);
                        stream.Write (msg1, 0, msg1.Length);
                        //stream.Flush();
                        break;
                       
                    case "isZooming":
                        toSend =zoom.isZooming.ToString()+",";
                        //print (toSend);
                        prevdata=toSend;
                        msg1 = System.Text.Encoding.ASCII.GetBytes(toSend);
                        stream.Write (msg1, 0, msg1.Length);
                        //stream.Flush();
                        break;
                    case "isZoomed":
                        toSend =zoom.isZoomed.ToString()+":";
                        prevdata=toSend;
                        //print (toSend);
                        msg1 = System.Text.Encoding.ASCII.GetBytes(toSend);
                        stream.Write (msg1, 0, msg1.Length);
                        stream.Flush();
                        break;
                        break;
                    case "Zoompressed1,true":
                        senddata1=true;
                       
                        //stream.Flush();
                        break;
                    case "Zoompressed2,true":
                        senddata2=true;
                       
                        break;
                    case "Left,true":
                        //msg1 = System.Text.Encoding.ASCII.GetBytes(data);
                        //stream.Write (msg1, 0, msg1.Length);
                        zoom.rotatearound1=true;
                        break;
                    case "Left,Stop":
                        //msg1 = System.Text.Encoding.ASCII.GetBytes(data);
                        //stream.Write (msg1, 0, msg1.Length);
                        zoom.rotatearound1=false;
                        break;
                    case "Right,true":
                        //msg1 = System.Text.Encoding.ASCII.GetBytes(data);
                        //stream.Write (msg1, 0, msg1.Length);
                        zoom.rotatearound2=true;
                        break;
                    case "Right,Stop":
                        //msg1 = System.Text.Encoding.ASCII.GetBytes(data);
                        //stream.Write (msg1, 0, msg1.Length);
                        zoom.rotatearound2=false;
                        break;
                    case "Up,true":
                        //msg1 = System.Text.Encoding.ASCII.GetBytes(data);
                        //stream.Write (msg1, 0, msg1.Length);
                        zoom.rotatearound3=true;
                        break;
                    case "Up,Stop":
                        //msg1 = System.Text.Encoding.ASCII.GetBytes(data);
                        //stream.Write (msg1, 0, msg1.Length);
                        zoom.rotatearound3=false;
                        break;
                    case "Down,true":
                        //msg1 = System.Text.Encoding.ASCII.GetBytes(data);
                        //stream.Write (msg1, 0, msg1.Length);
                        zoom.rotatearound4=true;
                        break;
                    case "Down,Stop":
                        //msg1 = System.Text.Encoding.ASCII.GetBytes(data);
                        //stream.Write (msg1, 0, msg1.Length);
                        zoom.rotatearound4=false;
                        break;
                    case "ZoomIn,true":
                        //msg1 = System.Text.Encoding.ASCII.GetBytes(data);
                        //stream.Write (msg1, 0, msg1.Length);
                        zoom.distanceChange1=true;
                        break;
                    case "ZoomIn,Stop":
                        //msg1 = System.Text.Encoding.ASCII.GetBytes(data);
                        //stream.Write (msg1, 0, msg1.Length);
                        zoom.distanceChange1=false;
                        break;
                    case "ZoomOut,true":
                        //msg1 = System.Text.Encoding.ASCII.GetBytes(data);
                        //stream.Write (msg1, 0, msg1.Length);
                        zoom.distanceChange2=true;
                        break;
                    case "ZoomOut,Stop":
                        //msg1 = System.Text.Encoding.ASCII.GetBytes(data);
                        //stream.Write (msg1, 0, msg1.Length);
                        zoom.distanceChange2=false;
                        break;
                    case "Disconnect":
                        goto q;
                        client.Close();
                        break;
                    case "Show":
                        show1=true;
                        break;
                    case "Hide":
                        hide=true;
 
                        break;
                    default:
                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(prevdata);
                       
                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                        //isTrue=true;
                        //msg1 = System.Text.Encoding.ASCII.GetBytes(data);
                        //stream.Write (msg1, 0, msg1.Length);
                        //senddata=true;
                        //StartCoroutine(sendString1(stream));
                        //sendString1(stream);
                        break;
                    }
                    bool contains1=data.Contains("Left,Stop");
                    if(contains1)zoom.rotatearound1=false;
                    bool contains2=data.Contains("Right,Stop");
                    if(contains2)zoom.rotatearound2=false;
                    bool contains3=data.Contains("Up,Stop");
                    if(contains3)zoom.rotatearound3=false;
                    bool contains4=data.Contains("Down,Stop");
                    if(contains4)zoom.rotatearound4=false;
                    bool contains5=data.Contains("ZoomIn,Stop");
                    if(contains5)zoom.distanceChange1=false;
                    bool contains6=data.Contains("ZoomOut,Stop");
                    if(contains6)zoom.distanceChange2=false;
                    if(data.StartsWith("(")){
                        //data=subS(data,'(');
                        parsedata=true;
                        //selection=true;
 
                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                        prevdata=data;
                        //Debug.Log("SentPrevdata:"+ prevdata);
                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                    }
                    if(data==" "){
                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(prevdata);
                        //Debug.Log("SentPrevdata:"+ prevdata);
                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                    }
                    if(data.StartsWith("!")){
                        //Debug.Log(data);
                        Vector3 vector;
                        try{
                        vector=parseVector3(data);
                        pos=vector;
                        endpointChanged=true;
                        //print (vector.ToString());
                        }
                        catch(Exception e){
                        }
                    }
                    if(data.StartsWith("&")){
                        //Debug.Log(data);
                        float timeOfDay=0.0f;
                        try{
                        timeOfDay=float.Parse(data.Substring(1,data.Length-1));
                        sun.currentTime=timeOfDay;
                        }
                        catch(FormatException e){
                        }
                        //endpointChanged=true;
                        //print (timeOfDay.ToString());
                    }
                    if(data.StartsWith("#")){
                        //Debug.Log(data);
                        float cloudiness=0.0f;
                        try{
                            cloudiness=float.Parse(data.Substring(1,data.Length-1));
                            sun.cloudiness=cloudiness;
                        }
                        catch(FormatException e){
                        }
                        //endpointChanged=true;
                        //print (cloudiness.ToString());
                    }
                    //Debug.Log("Sent:"+ data);
                }
            q:
                    show1=true;
                    // Shutdown and end connection
                    client.Close();
            }
        }
        catch(SocketException e)
        {
            Debug.Log("SocketException:"+ e);
        }
        finally
        {
            // Stop listening for new clients.
            server.Stop();
        }
       
        //yield return null;
       
       
    }
    Vector3 parseVector3(string toParse){
        Vector3 vector1;
        string[] splits0=toParse.Split (new char[]{'(',')'});
        string[] splits = splits0[1].Split (new char[]{','});
        //string[] splits2=splits[0].Split(new char[]{'!'});
        float x = float.Parse (splits[0]);
        float y = float.Parse (splits[1]);
        float z = float.Parse (splits[2]);
        vector1 = new Vector3 (x,y,z);
        return vector1;
    }
    List<GameObject> getFlats(){
        List<GameObject> flats1 = new List<GameObject> ();
        GameObject parent = GameObject.FindGameObjectWithTag ("Jsaari");
        Shader shader = Shader.Find("Transparent/Diffuse");
   
        Color color=new Color(1.0f,1.0f,1.0f,0.1f);
        //shader.material.color=color;
        for (int i = 0; i < parent.transform.childCount; i++)
        {
 
            GameObject obj = parent.transform.GetChild(i).gameObject;
       
 
            if(obj.name.StartsWith("_")){
                Renderer r=obj.GetComponent<Renderer>();
                Material material=r.sharedMaterial;
                r.sharedMaterial.shader=shader;
                r.sharedMaterial.color=color;
                /*material.SetColor("_EmissionColor",Color.white);
                material.SetFloat("_Mode", 2);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                material.color=color;
                material.SetColor("_EmissionColor",Color.white);
                obj.GetComponent<Renderer>().sharedMaterial=material;*/
                obj.tag="Available";
                flats1.Add(obj);
            }
        }
        return flats1;
    }
    IEnumerator parseName(GameObject obj){
        string name = obj.name;
        List<String> nameparts = new List<string> ();
        string[] splits = name.Split (new char[]{'_'});
        bool stairway=false, rooms1=false, floor=false;
        foreach(string s in splits){
            //print (s);
            if(s!="")nameparts.Add(s);
            yield return null;
        }
        //print (nameparts.Count);
        int index = 0;
        for(int i=0;i<nameparts.Count;i++){
            switch (i){
            case 0:
 
                index=0;
                //print (nameparts[i]);
                switch(nameparts[i]){
                case "a":
                    index=0;
                    yield return null;
                    break;
                case "b":
                    index=1;
                    yield return null;
                    break;
                case "c":
                    index=2;
                    yield return null;
                    break;
                case "d":
                    index=3;
                    yield return null;
                    break;
                case "e":
                    index=4;
                    yield return null;
                    break;
                case "f":
                    index=5;
                    yield return null;
                    break;
                case "g":
                    index=6;
                    yield return null;
                    break;
                case "h":
                    index=7;
                    yield return null;
                    break;
                case "i":
                    index=8;
                    yield return null;
                    break;
                case "j":
                    index=9;
                    yield return null;
                break;
                case "k":
                    index=10;
                    yield return null;
                    break;
                case "l":
                    index=11;
                    yield return null;
                    break;
                case "m":
                        index=12;
                    yield return null;
                    break;
                default:
                    yield return null;
                    break;
                }
                //print ("Index:"+index);
                //print ("Stairwaysbool:"+stairwaysBool[index]);
                if(stairwaysBool[index])stairway=true;
                yield return null;
                break;
            case 1:
                print ("Id");
                yield return null;
                break;
            case 2:
 
                index=Convert.ToInt32(nameparts[i]);
                //print (index);
                //print (roomsBool[index-1]);
                if(roomsBool[index-1])rooms1=true;
                yield return null;
                break;
            case 3:
                index=Convert.ToInt32(nameparts[i]);
                //print (index);
                //print (floorsBool[index-1]);
                if(floorsBool[index-1])floor=true;
                yield return null;
                break;
            default:
                print ("Error");
                yield return null;
                break;
            }
            yield return null;
        }
        //print (stairway.ToString() + rooms1.ToString()+floor.ToString() +obj.CompareTag("Available").ToString());
        if (stairway && rooms1 && floor && obj.CompareTag ("Available")) {
            //print ("show");
            Color color1 = new Color (0.0f, 0.5f, 0.8f, 0.6f);
            obj.GetComponent<Renderer> ().sharedMaterial.color = color1;
            //obj.GetComponent<Renderer> ().sharedMaterial.SetColor("_EmissionColor",Color.cyan);
            yield return null;
        }
        else {
            //print ("hide");
            Color color1 = new Color (1.0f, 1.0f, 1.0f, 0.1f);
            obj.GetComponent<Renderer> ().sharedMaterial.color = color1;
            //obj.GetComponent<Renderer> ().sharedMaterial.SetColor("_EmissionColor",Color.white);
            yield return null;
        }
        yield return null;
    }
    IEnumerator parseData(string data){
        string s1 = subS (data,'(');
 
        //print (s1);
        AssignList (ref stairwaysBool,s1);
        string s2 = subS (data,'_');
        //print (s2);
        AssignList (ref floorsBool, s2);
   
        string s3 = subS (data,'%');
        //print (s3);
        AssignList (ref roomsBool,s3);
        string s4 = subS (data,'$');
        available = Convert.ToBoolean (s4);
        selection = true;
        yield return null;
    }
    void AssignList(ref List<bool> sBool, string s){
 
        String[] s31 = s.Split (new char[]{','});
        for(int i=0;i<sBool.Count;i++){
            for(int j=0;j<s31.Length-1;j++){
               
                int a=Convert.ToInt32(s31[j]);
                //print (a);
                if(a==i){sBool[i]=true;
                    break;}
                sBool[i]=false;
            }
        }
    }
    string subS(string s, char c){
        string s1 = "";
        int first = s.IndexOf (c);
        if (first != null) {
            int last=s.LastIndexOf(c);
            int length=last-first-1;
            if(length>0){
                s1=s.Substring(first+1,length);
            }
        }
        return s1;
    }
    public void ZoomIn(){
        if(!zoom.isPointsSet){
            zoom.startpoint = zoom.transform.position;
            zoom.endpoint = zoom.Zoompoint.position;
            zoom.startrotation = zoom.transform.rotation;
            zoom.endrotation = zoom.Zoompoint.rotation;
            zoom.isPointsSet=true;
            zoom.isZoomed=false;
        }
        zoom.isZooming = true;
        toSend =zoom.isZooming.ToString()+",";
        //print (toSend);
        msg1 = System.Text.Encoding.ASCII.GetBytes(toSend);
        stream.Write (msg1, 0, msg1.Length);
        zoom.Zoompressed = true;
    }
    public void ZoomBack(){
        if(!zoom.isPointsSet){
            zoom.endpoint=zoom.transform.position;
            zoom.endrotation=zoom.transform.rotation;
            zoom.isPointsSet=true;
            zoom.isZoomed=true;
        }
        zoom.isZooming = true;
        toSend =zoom.isZooming.ToString()+",";
        //print (toSend);
        msg1 = System.Text.Encoding.ASCII.GetBytes(toSend);
        stream.Write (msg1, 0, msg1.Length);
        zoom.Zoompressed=true;
    }
    List<bool> InitializeFloorsBool(){
        List<bool> list = new List<bool> ();
        for(int i=0;i<floors;i++){
            list.Add(true);
        }
        return list;
    }
    List<bool> InitializeRoomsBool(){
        List<bool> list = new List<bool> ();
        for(int i=0;i<rooms.Count;i++){
            list.Add(true);
        }
        return list;
    }
    List<bool> InitializeStairwaysBool(){
        List<bool> list = new List<bool> ();
        for(int i=0;i<stairways.Count;i++){
            list.Add(true);
        }
        return list;
    }
    IEnumerator sendString1(NetworkStream stream1){
        string stairways1 = Stairways ();
        string rooms1 = Rooms ();
        string toSend = floors.ToString () + "/" + stairways1 + "/" + rooms1;
        //print (toSend);
        byte[] msg1 = System.Text.Encoding.ASCII.GetBytes(toSend);
        stream1.Write (msg1, 0, msg1.Length);
        yield return null;
    }
    string Stairways(){
        string s = "";
        for(int i=0; i<stairways.Count;i++){
            s+=stairways[i]+",";
        }
        return s;
    }
    string Rooms(){
        string s = "";
        for(int i=0; i<rooms.Count;i++){
            s+=rooms[i].ToString()+",";
        }
        return s;
    }
}
 