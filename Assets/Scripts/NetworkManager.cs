using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Text connectionText;
    [SerializeField]
    private Transform[] spawnPoints;
    [SerializeField]
    private Camera sceneCamera;
    [SerializeField]
    private GameObject[] playerModel;
    [SerializeField]
    private GameObject serverWindow;
    [SerializeField]
    private GameObject messageWindow;
    [SerializeField]
    private GameObject sightImage;
    [SerializeField]
    private InputField username;
    [SerializeField]
    private InputField roomName;
    [SerializeField]
    private InputField roomList;
    [SerializeField]
    private InputField messagesLog;
    // Timer
    [SerializeField]
    private Text timerText;

    // Kills & deaths UI elements for the local player
    [SerializeField]
    private Text killCountText;
    [SerializeField]
    private Text deathCountText;

    private GameObject player;
    private Queue<string> messages;
    private const int messageCount = 10;
    private string nickNamePrefKey = "PlayerName";
    private PhotonView photonView;

    // Timer variables
    public float GameDuration = 60f; // Game duration in seconds
    private float elapsedTime = 0f;
    private bool isGameOver = false;

    // Kill & Death counts for all players
    private Dictionary<string, int> killCounts = new Dictionary<string, int>();
    private Dictionary<string, int> deathCounts = new Dictionary<string, int>();

    void Start()
    {
        messages = new Queue<string>(messageCount);
        photonView = GetComponent<PhotonView>();
        if (PlayerPrefs.HasKey(nickNamePrefKey))
        {
            username.text = PlayerPrefs.GetString(nickNamePrefKey);
        }
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
        connectionText.text = "Connecting to lobby...";
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        connectionText.text = cause.ToString();
    }

    public override void OnJoinedLobby()
    {
        serverWindow.SetActive(true);
        connectionText.text = "";
    }

    public override void OnRoomListUpdate(List<RoomInfo> rooms)
    {
        roomList.text = "";
        foreach (RoomInfo room in rooms)
        {
            roomList.text += room.Name + "\n";
        }
    }

    public void JoinRoom()
    {
        serverWindow.SetActive(false);
        connectionText.text = "Joining room...";
        PhotonNetwork.LocalPlayer.NickName = username.text;
        PlayerPrefs.SetString(nickNamePrefKey, username.text);
        RoomOptions roomOptions = new RoomOptions()
        {
            IsVisible = true,
            MaxPlayers = 8
        };
        if (PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.JoinOrCreateRoom(roomName.text, roomOptions, TypedLobby.Default);
        }
        else
        {
            connectionText.text = "PhotonNetwork connection is not ready, try restarting it.";
        }
    }

    public override void OnJoinedRoom()
    {
        connectionText.text = "";
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        StartGame();
    }

    void StartGame()
    {
        Debug.Log("Starting game with GameDuration: " + GameDuration);
        Respawn(0.0f);
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(GameTimer());
        }
    }

    void Respawn(float spawnTime)
    {
        sightImage.SetActive(false);
        sceneCamera.enabled = true;
        StartCoroutine(RespawnCoroutine(spawnTime));
    }

    // Method to update the local player's UI
    private void UpdateLocalPlayerUI()
    {
        string localPlayerName = PhotonNetwork.LocalPlayer.NickName;
        int kills = killCounts.ContainsKey(localPlayerName) ? killCounts[localPlayerName] : 0;
        int deaths = deathCounts.ContainsKey(localPlayerName) ? deathCounts[localPlayerName] : 0;
        killCountText.text = "Kills: " + kills.ToString();
        deathCountText.text = "Deaths: " + deaths.ToString();
    }

    // Method to handle kill event
    public void HandleKill(string playerName)
    {
        photonView.RPC("RPC_HandleKill", RpcTarget.All, playerName);
    }

    // Method to handle death event
    public void HandleDeath(string playerName)
    {
        photonView.RPC("RPC_HandleDeath", RpcTarget.All, playerName);
    }

    [PunRPC]
    void RPC_HandleKill(string playerName)
    {
        if (killCounts.ContainsKey(playerName))
        {
            killCounts[playerName]++;
        }
        else
        {
            killCounts.Add(playerName, 1); // If player's entry doesn't exist, add it with a kill count of 1
        }
        if (playerName == PhotonNetwork.LocalPlayer.NickName)
        {
            UpdateLocalPlayerUI(); // Only update the UI if it's the local player
        }
    }

    [PunRPC]
    void RPC_HandleDeath(string playerName)
    {
        if (deathCounts.ContainsKey(playerName))
        {
            deathCounts[playerName]++;
        }
        else
        {
            deathCounts.Add(playerName, 1); // If player's entry doesn't exist, add it with a death count of 1
        }
        if (playerName == PhotonNetwork.LocalPlayer.NickName)
        {
            UpdateLocalPlayerUI(); // Only update the UI if it's the local player
        }
    }

    IEnumerator RespawnCoroutine(float spawnTime)
    {
        yield return new WaitForSeconds(spawnTime);
        messageWindow.SetActive(true);
        sightImage.SetActive(true);
        int playerIndex = Random.Range(0, playerModel.Length);
        int spawnIndex = Random.Range(0, spawnPoints.Length);
        player = PhotonNetwork.Instantiate(playerModel[playerIndex].name, spawnPoints[spawnIndex].position, spawnPoints[spawnIndex].rotation, 0);
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        playerHealth.RespawnEvent += Respawn;
        playerHealth.AddMessageEvent += AddMessage;
        sceneCamera.enabled = false;
        if (spawnTime == 0)
        {
            AddMessage("Player " + PhotonNetwork.LocalPlayer.NickName + " Joined Game.");
        }
        else
        {
            AddMessage("Player " + PhotonNetwork.LocalPlayer.NickName + " Respawned.");
        }
    }

    void AddMessage(string message)
    {
        photonView.RPC("AddMessage_RPC", RpcTarget.All, message);
    }

    [PunRPC]
    void AddMessage_RPC(string message)
    {
        messages.Enqueue(message);
        if (messages.Count > messageCount)
        {
            messages.Dequeue();
        }
        messagesLog.text = "";
        foreach (string m in messages)
        {
            messagesLog.text += m + "\n";
        }
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            AddMessage("Player " + other.NickName + " Left Game.");
        }
    }

    IEnumerator GameTimer()
    {
        while (elapsedTime < GameDuration && !isGameOver)
        {
            yield return new WaitForSeconds(1f);
            elapsedTime += 1f;
            photonView.RPC("UpdateTimer", RpcTarget.All, elapsedTime);
        }

        if (!isGameOver)
        {
            photonView.RPC("EndGame", RpcTarget.All);
        }
    }

    [PunRPC]
    void UpdateTimer(float time)
    {
        elapsedTime = time;
        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        float remainingTime = GameDuration - elapsedTime;
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    [PunRPC]
    void EndGame()
    {
        isGameOver = true;
        Debug.Log("Game Over!");
        HandleGameEnd();
    }

    void HandleGameEnd()
    {
        // Stop all player actions and show end-game screen or results
        AddMessage("Game Over!");

        // Print kill and death data for all players
        Debug.Log("Kill and Death Data:");
        foreach (var kvp in killCounts)
        {
            string playerName = kvp.Key;
            int kills = kvp.Value;
            int deaths = deathCounts.ContainsKey(playerName) ? deathCounts[playerName] : 0;
            Debug.Log(playerName + ": Kills - " + kills + ", Deaths - " + deaths);
            AddMessage(playerName + ": Kills - " + kills + ", Deaths - " + deaths);
        }
        
        // Implement additional end-game logic here
    }
}
