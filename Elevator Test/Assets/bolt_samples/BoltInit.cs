using UnityEngine;
using System;
using UdpKit;
using UnityEngine.SceneManagement;

public class BoltInit : Bolt.GlobalEventListener
{
    public class RoomProtocolToken : Bolt.IProtocolToken
    {
        public String ArbitraryData;

        public void Read(UdpPacket packet)
        {
            ArbitraryData = packet.ReadString();
        }

        public void Write(UdpPacket packet)
        {
            packet.WriteString(ArbitraryData);
        }
    }

    enum State
    {
        SelectMode,
        SelectMap,
        EnterServerIp,
        StartServer,
        StartClient,
        Started,
    }

    State state;

    string map;
    string serverAddress = "127.0.0.1";
    string serverPort = "25000";

    //int serverPort = 25000;

    void Awake()
    {
        serverPort = BoltRuntimeSettings.instance.debugStartPort.ToString();

#if !BOLT_CLOUD
        BoltLauncher.SetUdpPlatform(new DotNetPlatform());
#endif
    }

    void OnGUI()
    {
        Rect tex = new Rect(10, 10, 140, 75);
        Rect area = new Rect(10, 90, Screen.width - 20, Screen.height - 100);

        GUI.Box(tex, Resources.Load("BoltLogo") as Texture2D);
        GUILayout.BeginArea(area);

        switch (state)
        {
            case State.SelectMode: State_SelectMode(); break;
            case State.SelectMap: State_SelectMap(); break;
            case State.EnterServerIp: State_EnterServerIp(); break;
            case State.StartClient: State_StartClient(); break;
            case State.StartServer: State_StartServer(); break;
        }

        GUILayout.EndArea();
    }

    public override void BoltStartBegin()
    {
        BoltNetwork.RegisterTokenClass<RoomProtocolToken>();
    }

    private void State_EnterServerIp()
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label("Server IP: ");
        serverAddress = GUILayout.TextField(serverAddress);

        GUILayout.Label("Server Port: ");
        serverPort = GUILayout.TextField(serverPort);

        if (GUILayout.Button("Connect"))
        {
            state = State.StartClient;
        }

        GUILayout.EndHorizontal();
    }


    void State_SelectMode()
    {
        if (ExpandButton("Server"))
        {
            state = State.SelectMap;
        }
        if (ExpandButton("Client"))
        {
            state = State.EnterServerIp;
        }
    }

    void State_SelectMap()
    {
        foreach (string value in BoltScenes.AllScenes)
        {
            if (SceneManager.GetActiveScene().name != value)
            {
                if (ExpandButton(value))
                {
                    map = value;
                    state = State.StartServer;
                }
            }
        }
    }

    void State_StartServer()
    {
        ushort port = 0;
        ushort.TryParse(serverPort, out port);

        BoltLauncher.StartServer(new UdpEndPoint(UdpIPv4Address.Any, port));

        state = State.Started;
    }

    void State_StartClient()
    {
        BoltLauncher.StartClient(UdpEndPoint.Any);
        state = State.Started;
    }

    bool ExpandButton(string text)
    {
        return GUILayout.Button(text, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
    }

    public override void BoltStartDone()
    {
        if (BoltNetwork.isClient)
        {
#if BOLT_CLOUD
            Debug.LogError("This call is only valid on Bolt Server version");
#else
            ushort port = 0;
            ushort.TryParse(serverPort, out port);

            UdpEndPoint endPoint = new UdpEndPoint(UdpIPv4Address.Parse(serverAddress), port);

            RoomProtocolToken token = new RoomProtocolToken();
            token.ArbitraryData = "Room Token";

            BoltNetwork.Connect(endPoint, token);
#endif
        }
        else
        {
            BoltNetwork.LoadScene(map);
        }
    }
}

