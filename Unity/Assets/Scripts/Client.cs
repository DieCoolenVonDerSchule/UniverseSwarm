using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading;


public class Client : MonoBehaviour
{

    const int HEADER = 2048; //2048;
    const string DISCONNECT_MESSAGE = "!DISCONNECT";

    public static Client instance;
    //public static int dataBufferSize = 4096;
    public static int dataBufferSize = HEADER;

    public const string ip = "lazyloki.ddns.net";   //VM
    //public const string ip = "192.168.1.75";   //VM


    public const int port = 5550;
    public int myId = 0;



    public Socket socket;
    public Socket logSocket;

    //private NetworkStream stream;
    private byte[] receiveBuffer;



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object");
            Destroy(this);
        }

    }


    private void Start()
    {
        
    }


    public void send(String msg)
    {

        string sizeinfo = msg.Length.ToString();

        char[] padding = new char[HEADER - sizeinfo.Length];

        for (int i = 0; i < padding.Length; i++)
        {
            padding[i] = ' ';
        }

        sizeinfo += new String(padding);

        print("[SENDING] sizeinfo");
        socket.Send(Encoding.UTF8.GetBytes(sizeinfo), 0, Encoding.UTF8.GetByteCount(sizeinfo), SocketFlags.None);
        print("[SENDING] msg");
        socket.Send(Encoding.UTF8.GetBytes(msg), 0, Encoding.UTF8.GetByteCount(msg), SocketFlags.None);


    }

    public void sendLogs(String msg)
    {

        string sizeinfo = msg.Length.ToString();

        char[] padding = new char[HEADER - sizeinfo.Length];

        for (int i = 0; i < padding.Length; i++)
        {
            padding[i] = ' ';
        }

        sizeinfo += new String(padding);

        print("[SENDING] sizeinfo");
        logSocket.Send(Encoding.UTF8.GetBytes(sizeinfo), 0, Encoding.UTF8.GetByteCount(sizeinfo), SocketFlags.None);
        print("[SENDING] msg");
        logSocket.Send(Encoding.UTF8.GetBytes(msg), 0, Encoding.UTF8.GetByteCount(msg), SocketFlags.None);


    }

    public String receive()
    {
        socket.Poll(-1, SelectMode.SelectRead);

        // RECEIVE SIZEINFO
        print("[RECEIVING] sizeinfo");
        byte[] mapSizeInfo = new byte[HEADER];

        socket.Receive(mapSizeInfo, 0, HEADER, SocketFlags.None);

        print("MSG LENGTH:" + Encoding.UTF8.GetString(mapSizeInfo).Length);
        print("RECEIVED MAP SIZE INFO: " + Encoding.UTF8.GetString(mapSizeInfo));
        int size = Convert.ToInt32(Encoding.UTF8.GetString(mapSizeInfo));
        print("[RECEIVED SIZEINFO] " + size);


        // RECEIVE DENSITYMAP
        byte[] densitymap = new byte[size];
        int receivedBytes = 0;

        socket.Poll(-1, SelectMode.SelectRead);
        
        while (socket.Available > 0)
        {

            receivedBytes += socket.Receive(densitymap, receivedBytes, socket.Available, SocketFlags.None);

            socket.Poll(100000, SelectMode.SelectRead);



            print("[RECEIVED DENSITYMAP]");
            print("RECEIVED BYTES: " + receivedBytes);
        }
        

        string densitymapString = Encoding.UTF8.GetString(densitymap);

        print("DENSITYMAP LENGTH: " + densitymapString.Length);
        print(densitymapString);


        return densitymapString;
    }

    public void ConnectToServer(string systemID, string planetID, MeshGenerator meshGen, DateTime before)
    {


        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        socket.Connect(ip, port);

        logSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        logSocket.Connect(ip, 5080);

        if (!socket.Connected)
        {
            return;
        }


        // SEND SYSTEMID TO QUEEN
        send(systemID);


        // SEND PLANETID TO QUEEN
        send(planetID);

        string densitymap = receive();

        DateTime after = DateTime.UtcNow;

        double duration = (after - before).TotalMilliseconds / 1000.0;

        sendLogs("0");
        sendLogs("extern");
        sendLogs("CLIENT");
        sendLogs("computationTime");
        sendLogs("" + duration);

        logSocket.Disconnect(false);
        print("[SOCKET DISCONNECTED]");

        //SEND DISCONNECT TO SERVER
        send(DISCONNECT_MESSAGE);

        socket.Disconnect(false);
        print("[SOCKET DISCONNECTED]");

        float[,,] densityMapConverted = JsonConvert.DeserializeObject<float[,,]>(densitymap);

        logSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        logSocket.Connect(ip, 5080);

        DateTime cubeMarchingStart = DateTime.UtcNow;
        meshGen.generateMeshCubeMarching(densityMapConverted);
        DateTime cubeMarchingStop = DateTime.UtcNow;

        duration = (cubeMarchingStop - cubeMarchingStart).TotalMilliseconds / 1000.0;

        sendLogs("0");
        sendLogs("extern");
        sendLogs("CLIENT");
        sendLogs("cubeMarchingTime");
        sendLogs("" + duration);

        logSocket.Disconnect(false);
        print("[SOCKET DISCONNECTED]");
    }



  


}
