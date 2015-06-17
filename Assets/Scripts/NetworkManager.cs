using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {
    public GameObject[] availablePrefabs;
    private int selected = 0;

    void OnServerInitialized() {
        Debug.Log("Server started");
        SpawnPlayer();
    }

    void OnConnectedToServer() {
        Debug.Log("Connected");
        SpawnPlayer();
    }
    void changeChar() {
        selected++;
        if (selected >= availablePrefabs.Length) {
            selected = 0;
        }
      
    }
    private void SpawnPlayer() {
        Network.Instantiate(availablePrefabs[selected], new Vector3(0f, 0.1f, 0f), Quaternion.identity, 0);
    }
    private const string typeName = "GameName";
    private const string gameName = "Csaréna";

    private void StartServer() {
        Network.InitializeServer(4, 25000, !Network.HavePublicAddress());
        MasterServer.RegisterHost(typeName, gameName);
    }
    private void JoinServer(HostData hostData) {
        Network.Connect(hostData);
    }
    private HostData[] hostList;

    private void RefreshHostList() {
        MasterServer.RequestHostList(typeName);
    }

    void OnMasterServerEvent(MasterServerEvent msEvent) {
        if (msEvent == MasterServerEvent.HostListReceived)
            hostList = MasterServer.PollHostList();
    }
    void OnGUI() {
        if (!Network.isClient && !Network.isServer) {
            if(GUI.Button(new Rect(100, 250, 200, 50), availablePrefabs[selected].name + " selected")){
                changeChar();
            }
            if (GUI.Button(new Rect(100, 50, 200, 50), "Start Server")) {
                StartServer();
            }

            if (GUI.Button(new Rect(100, 150, 200, 50), "Refresh Hosts")) {
                RefreshHostList();
            }
            if (hostList != null) {
                for (int i = 0; i < hostList.Length; i++) {
                    if (GUI.Button(new Rect(400, 100 + (110 * i), 200, 50), hostList[i].gameName))
                        JoinServer(hostList[i]);
                }
            }
        }
    }
}
