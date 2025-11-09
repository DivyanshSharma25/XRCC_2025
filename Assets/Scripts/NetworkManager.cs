using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance { get; private set; }

    [Header("Player Prefabs")]
    public GameObject networkPlayerPrefab;
    public GameObject nonNetworkPlayerPrefab;

    [Header("Active Players")]
    public GameObject LocalNetworkedPlayer { get; private set; }
    public GameObject LocalNonNetworkedPlayer { get; private set; }

    [Header("Spawn Points")]
    [Tooltip("Assign spawn Transforms here. Players will be placed using ActorNumber-based indexing.")]
    public Transform[] spawnPoints;

    [Header("Connection Settings")]
    [SerializeField] private string gameVersion = "1.0";
    [SerializeField] private int maxPlayersPerRoom = 20;
    [SerializeField] private string defaultRoomName = "MainRoom";

    private bool isConnecting = false;



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        ConnectToPhoton();
    }

    public void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            isConnecting = true;
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("Connecting to Photon...");
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server");
        if (isConnecting)
        {
            JoinDefaultRoom();
        }
    }

    public void JoinDefaultRoom()
    {
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = maxPlayersPerRoom,
            PublishUserId = true,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.JoinOrCreateRoom(defaultRoomName, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined Room: {PhotonNetwork.CurrentRoom.Name}");
        Debug.Log($"Player Count: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}");
        isConnecting = false;

        // Determine spawn transform. Use ActorNumber to pick a spawn point so each player spawns in a different slot.
        Transform chosenSpawn = null;
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int actorIndex = 0;
            try
            {
                actorIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1; // ActorNumber starts at 1
            }
            catch
            {
                actorIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
            }

            int spawnIndex = 0;
            if (spawnPoints.Length > 0)
                spawnIndex = Mathf.Abs(actorIndex) % spawnPoints.Length;

            chosenSpawn = spawnPoints[spawnIndex];
        }

        Vector3 spawnPos = chosenSpawn != null ? chosenSpawn.position : Vector3.zero;
        Quaternion spawnRot = chosenSpawn != null ? chosenSpawn.rotation : Quaternion.identity;

        // Spawn the networked player using PhotonNetwork
        if (networkPlayerPrefab != null)
        {
            LocalNetworkedPlayer = PhotonNetwork.Instantiate(networkPlayerPrefab.name, spawnPos, spawnRot);
            Debug.Log($"Networked player instantiated at {spawnPos}");

            if (nonNetworkPlayerPrefab != null)
            {
                LocalNonNetworkedPlayer = Instantiate(nonNetworkPlayerPrefab, spawnPos, spawnRot);
                Debug.Log($"Non-networked player instantiated at {spawnPos}");
                setNetworkPlayerRefs(LocalNetworkedPlayer, LocalNonNetworkedPlayer);
            }
            else
            {
                Debug.LogError("Non-Network Player Prefab is not assigned!");
            }

        }
        else
        {
            Debug.LogError("Network Player Prefab is not assigned!");
        }



    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to join room: {message}");
        // You might want to implement a retry mechanism here
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Disconnected from server: {cause}");
        isConnecting = false;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.NickName} joined the room");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player {otherPlayer.NickName} left the room");
    }

    public bool IsInRoom()
    {
        return PhotonNetwork.InRoom;
    }

    public bool IsConnected()
    {
        return PhotonNetwork.IsConnected;
    }

    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    public void DisconnectFromPhoton()
    {
        PhotonNetwork.Disconnect();
    }

    private void OnApplicationQuit()
    {
        DisconnectFromPhoton();
    }

    public void setNetworkPlayerRefs(GameObject networkedPlayer, GameObject nonNetworkedPlayer)
    {
        NetworkPlayerRefs refs = nonNetworkedPlayer.GetComponent<NetworkPlayerRefs>();
        IKTargetFollowVRRig followVRRig = networkedPlayer.GetComponent<IKTargetFollowVRRig>();
        followVRRig.xr_orgin = nonNetworkedPlayer.transform;
        followVRRig.head.vrTarget = refs.VR_head;
        followVRRig.leftHand.vrTarget = refs.VR_leftHand;
        followVRRig.rightHand.vrTarget = refs.VR_rightHand;

    }
}
