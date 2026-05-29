using JetBrains.Annotations;
using MarchingBytes;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class StageController : NetworkBehaviour
{
    [SerializeField] List<Transform> listBrickTransform = new List<Transform>();
    [SerializeField] GameObject brickPrefabs;


    public List<Brick> listBrickInStage = new List<Brick>();
    public List<LongBridge> listBridge = new List<LongBridge>();
    public bool isFinalStage = false;

    private List<int> listColorPlayGame = new List<int>();

    private List<int> listBrickInMap = new List<int>();

    void Start()
    {
        // Khởi tạo danh sách vị trí trống ban đầu
        for (int i = 0; i < listBrickTransform.Count; i++)
        {
            listBrickInMap.Add(i);
        }
    }

    public void ServerInitializeStageBricks(List<int> colorsInGame)
    {
        if (!IsServer) return;

        listColorPlayGame = new List<int>(colorsInGame);
        int brickPerPlayer = Mathf.CeilToInt((float)listBrickTransform.Count / colorsInGame.Count);

        //Debug.Log(colorsInGame.Count + " / " + listBrickTransform.Count + " / " + brickPerPlayer);

        foreach (int color in colorsInGame)
        {
            Debug.Log(color);
            for (int i = 0; i < brickPerPlayer; i++)
            {
                if (listBrickInMap.Count == 0) break;

                //Debug.Log("brick spawn");
                int randomIndex = Random.Range(0, listBrickInMap.Count);
                int pos_transform = listBrickInMap[randomIndex];
                listBrickInMap.RemoveAt(randomIndex);

                SpawnBrickOnServer(pos_transform, color);
            }
        }
    }
    
    private void SpawnBrickOnServer(int pos_transform, int color)
    {
        if (brickPrefabs == null)
        {
            Debug.LogError("//");
            return;
        }

        Vector3 spawnPosition = listBrickTransform[pos_transform].position;
        GameObject brickObj = Instantiate(brickPrefabs, spawnPosition, Quaternion.Euler(0, 90f, 0));

        Brick brick = brickObj.GetComponent<Brick>();
        if (brick == null)
        {
            Debug.LogError("LỖI: Prefab viên gạch chưa được gắn script Brick.cs!");
            return;
        }
        // Kiểm tra trước khi gọi
        NetworkObject netObj = brickObj.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("///");
            return;
        }
        netObj.Spawn(true);

        brick.SetStage(this);
        brick.SetBrickPosition(pos_transform);
        brick.SetBrickColor(color);

        listBrickInStage.Add(brick);
    }

    public void CreateNewBrick(int position)
    {
        if (!IsServer) return;
        StartCoroutine(CreateNewBrickAfterDelayTime(position));
    }
    IEnumerator CreateNewBrickAfterDelayTime(int position)
    {
        yield return new WaitForSeconds(3f);

        // Khi gạch cũ bị nhặt, viên gạch mới hồi lại sẽ random ngẫu nhiên 1 trong các màu đang chơi
        int randomColor = listColorPlayGame[Random.Range(0, listColorPlayGame.Count)];
        SpawnBrickOnServer(position, randomColor);
    }


    //private int GetTotalBrickInStair(int color)
    //{
    //    int count = 0;
    //    for (int i = 0; i < listBridge.Count; i++)
    //    {
    //        for (int j = 0; j < listBridge[i].listStair.Count; j++)
    //        {
    //            count += listBridge[i].GetTotalBrickColor(color);
    //        }
    //    }
    //    return count;
    //}


    //public void CharacterStartGame(int colorIndex)
    //{
    //    if (!listColorPlayGame.Contains(colorIndex))
    //    {
    //        listColorPlayGame.Add(colorIndex);
    //        CreateNewBrickForCharacter(colorIndex);
    //    }
    //    else Debug.Log("player existed");
    //}


    //public void CreateNewBrickForCharacter(int color)
    //{
    //    for (int i = 0; i < 10; i++)
    //    {
    //        int pos = Random.Range(0, listBrickInMap.Count);
    //        int pos_transform = listBrickInMap[pos];
    //        listBrickInMap.Remove(pos_transform);

    //        Brick brick = Instantiate(brickPrefabs).GetComponent<Brick>();
    //        brick.transform.position = listBrickTransform[pos_transform].transform.position;

    //        Brick brick = EasyObjectPool.instance.GetObjectFromPool("Brick", listBrickTransform[pos_transform].transform.position, Quaternion.identity).GetComponent<Brick>();
    //        brick.transform.rotation = Quaternion.Euler(0, 90f, 0);
    //        brick.GetComponent<BoxCollider>().enabled = true;
    //        brick.transform.SetParent(listBrickTransform[pos_transform].transform);
    //        brick.transform.localPosition = Vector3.zero;

    //        brick.SetBrickPosition(pos_transform);
    //        brick.SetBrickColor(color);
    //        brick.SetStage(this);

    //        listBrickInStage.Add(brick);
    //    }
    //}

    //IEnumerator CreateNewBrickAfterDelayTime(int position)
    //{
    //    yield return new WaitForSeconds(3f);

    //    Brick brick = Instantiate(brickPrefabs, listBrickTransform[position].transform).GetComponent<Brick>();

    //    Brick brick = EasyObjectPool.instance.GetObjectFromPool("Brick", listBrickTransform[position].transform.position, Quaternion.identity).GetComponent<Brick>();

    //    brick.transform.SetParent(listBrickTransform[position].transform);
    //    brick.GetComponent<BoxCollider>().enabled = true;
    //    brick.transform.localPosition = Vector3.zero;
    //    brick.transform.rotation = Quaternion.Euler(0, 90f, 0);


    //    brick.SetBrickPosition(position);
    //    brick.SetBrickColor(listColorPlayGame[Random.Range(0, listColorPlayGame.Count)]);
    //    brick.SetStage(this);
    //    listBrickInStage.Add(brick);
    //}
}
