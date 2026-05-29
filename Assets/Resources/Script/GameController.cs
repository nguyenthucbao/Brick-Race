using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class GameController : NetworkBehaviour
{
    public static GameController Instance { get; private set; }

    public List<int> gameColor = new List<int>();
    //public Player player;
    public List<int> startPointInt = new List<int>();
    public List<Transform> startPoint = new List<Transform>();
    public Bot bot;
    public Transform finishPoint;
    public JoystickControl joystick;
    public List<Character> playerList = new List<Character>();

    public StageController stageController;

    public GameObject startGamePanel;
    public GameObject endGamePanel;

    public Button startGameButton;
    public Button startHostButton;
    public Button startClientButton;

    public TextMeshProUGUI playerListText;  
    public TextMeshProUGUI warningText;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    void Start()
    {
        startGameButton.gameObject.SetActive(false);
        if (warningText != null) warningText.text = "";

        // Gán sự kiện cho UI UI ban đầu để test
        startHostButton.onClick.AddListener(StartAsHost);
        startClientButton.onClick.AddListener(StartAsClient);
        startGameButton.onClick.AddListener(RequestStartGame);

        // Đăng ký sự kiện khi có Player kết nối vào mạng (Chỉ chạy trên Server/Host)
        NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerDisconnected;
    }
    private void StartAsHost()
    {
        //NetworkManager.Singleton.StartHost();
        //startGamePanel.SetActive(false);

        if (NetworkManager.Singleton.StartHost())
        {
            if (warningText != null) warningText.text = "Đã tạo phòng (Host). Chờ người chơi...";
            startGameButton.gameObject.SetActive(true);
            UpdatePlayerListUI();
        }
        else
        {
            ShowWarning("Không thể khởi tạo Host!");
        }
    }
    private void StartAsClient()
    {
        //NetworkManager.Singleton.StartClient();
        //startGamePanel.SetActive(false);

        if (warningText != null) warningText.text = "Đang kết nối đến Host...";

        if (!NetworkManager.Singleton.StartClient())
        {
            ShowWarning("CẢNH BÁO: Không thể kết nối! Chưa có Host nào được tạo.");
        }
    }
    private void UpdatePlayerListUI()
    {
        if (playerListText == null) return;

        playerListText.text = "DANH SÁCH NGƯỜI CHƠI:\n";
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            string role = client.ClientId == NetworkManager.Singleton.LocalClientId ? " (Bạn)" : "";
            playerListText.text += $"- Player ID: {client.ClientId}{role}\n";
        }
    }

    private void OnPlayerConnected(ulong clientId)
    {
        // Cập nhật danh sách hiển thị UI cho toàn bộ các máy
        UpdatePlayerListUI();
    }

    private void OnPlayerDisconnected(ulong clientId)
    {
        UpdatePlayerListUI();
    }

    private void RequestStartGame()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        // Gọi RPC gửi tới tất cả Client để đồng loạt tắt UI phòng chờ và bắt đầu game
        StartGameClientRpc();
        AssignPlayersStartSetup();

        if (NetworkManager.Singleton.IsServer && stageController != null)
        {

            // Truyền thẳng list gameColor (chứa tất cả ID màu của trận này) vào Stage
            stageController.ServerInitializeStageBricks(gameColor);
        }
    }

    [ClientRpc]
    private void StartGameClientRpc()
    {
        // Ẩn panel phòng chờ ở TẤT CẢ các máy Client và Host
        if (startGamePanel != null)
        { 
            startGamePanel.SetActive(false); 
        }
    }
    private void AssignPlayersStartSetup()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            
            NetworkObject playerNetObj = client.PlayerObject;          

            if (playerNetObj != null)
            {
                Player playerScript = playerNetObj.GetComponent<Player>();
                if (playerScript != null)
                {
                    //Spwwn position
                    int randPosIndex = RandomPlayerSpawn();
                    playerScript.transform.position = startPoint[randPosIndex].position;

                    //Color
                    int assignedColor = RandomPlayerColor();
                    playerScript.colorIndex.Value = assignedColor;
                }
            }
        }
    }
    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerDisconnected;
        }
    }

    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    private void ShowWarning(string message)
    {
        if (warningText != null)
        {
            warningText.color = Color.red;
            warningText.text = message;
        }
        Debug.LogWarning(message);
    }

    private int RandomPlayerColor()
    {
        int randomColor;
        while (true)
        {
            randomColor = Random.Range(0, 10);
            bool sameColor = false;
            for (int j = 0; j < gameColor.Count; j++)
            {
                if (gameColor[j] == randomColor)
                {
                    sameColor = true;
                    break;
                }
            }
            if (!sameColor)
            { break; }
        }
        gameColor.Add(randomColor);

        return randomColor;
    }
    private int RandomPlayerSpawn()
    {
        int randPosIndex;
        while (true)
        {
            randPosIndex = Random.Range(0, startPoint.Count);
            bool samePosIndex = false;
            for (int j = 0; j < startPointInt.Count; j++)
            {
                if (startPointInt[j] == randPosIndex)
                {
                    samePosIndex = true;
                    break;
                }
            }
            if (!samePosIndex)
            { break; }
        }
        startPointInt.Add(randPosIndex);

        return randPosIndex;
    }

    //private void SetUpPlayerColor()
    //{
    //    player.SetCharacterColor(gameColor[0]);
    //    int rand_pos = Random.Range(0, startPoint.Count);
    //    player.transform.position = startPoint[rand_pos].position;
    //    startPoint.RemoveAt(rand_pos);

    //    playerList.Add(player);

    //    for (int i = 0; i < 2; i++)
    //    {
    //        Bot botInGame = Instantiate(bot);
    //        botInGame.SetCharacterColor(gameColor[i + 1]);
    //        //rand_pos = Random.Range(0, startPoint.Count);
    //        botInGame.transform.position = startPoint[i].position;
    //        //startPoint.RemoveAt(rand_pos);
    //        playerList.Add(botInGame);
    //    }

    //    for (int i = 0; i < playerList.Count; i++)
    //    {
    //        playerList[i].enabled = false;
    //    }
    //    joystick.enabled = false;
    //}

        //public void PlayGame()
        //{
        //    for(int i = 0; i < playerList.Count; i++)
        //    {
        //        playerList[i].enabled = true;
        //    }
        //    joystick.enabled = true;
        //    startGamePanel.SetActive(false);
        //}
        //public void EndGame(Character winner)
        //{
        //    Debug.Log("Endgame");
        //    for (int i = 0; i < playerList.Count; i++)
        //    {
        //        if (winner == playerList[i])
        //        {
        //            winner.ChangeAnim("Victory");
        //            winner.transform.position = finishPoint.transform.position + Vector3.up;
        //            joystick.gameObject.SetActive(false);
        //            endGamePanel.SetActive(true);
        //        }
        //        else
        //        {
        //            Destroy(playerList[i].gameObject);
        //        }
        //    }
        //}
    }
