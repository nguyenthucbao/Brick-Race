using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameController : Singleton<GameController>
{
    public List<int> gameColor = new List<int>();
    public Player player;
    public List<Transform> startPoint = new List<Transform>();
    public Bot bot;
    public Transform finishPoint;
    public JoystickControl joystick;
    public List<Character> playerList = new List<Character>();


    public GameObject startGamePanel;
    public GameObject endGamePanel;
    public Button startGameButton;

    void Start()
    {
        RandomGameColor();
        SetUpPlayerColor();
    }

    // Update is called once per frame
    private void RandomGameColor()
    {
        for (int i = 0; i < 3; i++)
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
        }


    }
    private void SetUpPlayerColor()
    {
        player.SetCharacterColor(gameColor[0]);
        int rand_pos = Random.Range(0, startPoint.Count);
        player.transform.position = startPoint[rand_pos].position;
        startPoint.RemoveAt(rand_pos);

        playerList.Add(player);

        for (int i = 0; i < 2; i++)
        {
            Bot botInGame = Instantiate(bot);
            botInGame.SetCharacterColor(gameColor[i + 1]);
            //rand_pos = Random.Range(0, startPoint.Count);
            botInGame.transform.position = startPoint[i].position;
            //startPoint.RemoveAt(rand_pos);
            playerList.Add(botInGame);
        }

        for (int i = 0; i < playerList.Count; i++)
        {
            playerList[i].enabled = false;
        }
        joystick.enabled = false;

    }

    

    public void PlayGame()
    {
        for(int i = 0; i < playerList.Count; i++)
        {
            playerList[i].enabled = true;
        }
        joystick.enabled = true;
        startGamePanel.SetActive(false);
    }

    public void EndGame(Character winner)
    {
        Debug.Log("Endgame");
        for (int i = 0; i < playerList.Count; i++)
        {
            if (winner == playerList[i])
            {
                winner.ChangeAnim("Victory");
                winner.transform.position = finishPoint.transform.position + Vector3.up;
                joystick.gameObject.SetActive(false);
                endGamePanel.SetActive(true);
            }
            else
            {
                Destroy(playerList[i].gameObject);
            }


        }
    }



}
