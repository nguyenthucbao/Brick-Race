using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;
using MarchingBytes;

public class StageController : MonoBehaviour
{
    [SerializeField] List<Transform> listBrickTransform = new List<Transform>();
    [SerializeField] GameObject brickPrefabs;
    

    public List<Brick> listBrickInStage = new List<Brick>();
    public List<LongBridge> listBridge = new List<LongBridge>();
    public bool isFinalStage = false;

    private List<int> listColorPlayGame = new List<int>();

    private List<int> listBrickInMap = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
       for (int i = 0; i < listBrickTransform.Count; i++)
        {
            listBrickInMap.Add(i);
        }
    }

    public List<Transform> GetPathDestination(Bot bot)
    {
        List<Transform> path = new List<Transform> ();
        if (GetTotalBrickInStair(bot.colorIndex) == 0)    //chua tha gach
        {
            int index = Random.Range(0, 3);
            path.Add(listBridge[index].listStair[0].transform);
            path.Add(listBridge[index].listStair[listBridge[index].listStair.Count-1].transform);
        }else    // da tha gach
        {
            int index = 0;
            int maxColor = 0;
            
            for (int i = 0; i < listBridge.Count; i++)
            {
                
                int count = listBridge[i].GetTotalBrickColor(bot.colorIndex);

                if (count > maxColor)
                {
                    maxColor = count;
                    index = i;
                }
            }
            path.Add(listBridge[index].listStair[0].transform);
            path.Add(listBridge[index].listStair[listBridge[index].listStair.Count-1].transform);

           
        }
        if (isFinalStage)
        {
            //path.Add(GameController.Instance)
        }
        return path;
    }

    public bool GetDestinationOfBot(Bot bot)
    {
        if (bot.totalBrick >= 3 && Random.Range(0,10) < 5)
        {
            return true;
        }
        return false;
    }

    private int GetTotalBrickInStair(int color)
    {
        int count = 0;
        for ( int i = 0; i < listBridge.Count; i++)
        {
            for( int j = 0; j < listBridge[i].listStair.Count; j++)
            {
                count += listBridge[i].GetTotalBrickColor(color);
            }
        }
        return count;
    }

    public Transform GetNearestBricks(Bot bot)
    {
        float distanecMin = float.MaxValue;
        int index = -1;

        for (int i = 0; i < listBrickInStage.Count; i++)
        {
            if (bot.colorIndex == listBrickInStage[i].brickColor)
            {
                float distance = (bot.transform.position - listBrickInStage[i].transform.position).magnitude;
                if (distance < distanecMin)
                {
                    index = i;
                    distanecMin = distance;
                }
            }        
        }

        if (index >= 0)
        {
            return listBrickInStage[index].transform;
        }
        return null;
    }

    public void CharacterStartGame(int color)
    {
        if (!listColorPlayGame.Contains(color))
        {
            listColorPlayGame.Add(color);
            CreateNewBrickForCharacter(color);
        }
    }

    public void CreateNewBrickForCharacter(int color)
    {
        for(int i = 0; i < 10; i++)
        {
            int pos = Random.Range(0, listBrickInMap.Count);
            int pos_transform = listBrickInMap[pos];
            listBrickInMap.Remove(pos_transform);

            //Brick brick = Instantiate(brickPrefabs).GetComponent<Brick>();
            //brick.transform.position = listBrickTransform[pos_transform].transform.position;

            Brick brick = EasyObjectPool.instance.GetObjectFromPool("Brick", listBrickTransform[pos_transform].transform.position, Quaternion.identity).GetComponent<Brick>();
            brick.transform.rotation = Quaternion.Euler(0, 90f, 0);
            brick.GetComponent<BoxCollider>().enabled = true;
            brick.transform.SetParent(listBrickTransform[pos_transform].transform);
            brick.transform.localPosition = Vector3.zero;

            brick.SetBrickPosition(pos_transform);
            brick.SetBrickColor(color);
            brick.SetStage(this);

            listBrickInStage.Add(brick);
        }
    }

    public void CreateNewBrick(int position)
    {
        StartCoroutine(CreateNewBrickAfterDelayTime(position));
    }

    IEnumerator CreateNewBrickAfterDelayTime(int position)
    {
        yield return new WaitForSeconds(3f);

        //Brick brick = Instantiate(brickPrefabs, listBrickTransform[position].transform).GetComponent<Brick>();
        
        Brick brick = EasyObjectPool.instance.GetObjectFromPool("Brick", listBrickTransform[position].transform.position, Quaternion.identity).GetComponent<Brick>();
        
        brick.transform.SetParent(listBrickTransform[position].transform);
        brick.GetComponent<BoxCollider>().enabled = true;
        brick.transform.localPosition = Vector3.zero;
        brick.transform.rotation = Quaternion.Euler(0, 90f, 0);


        brick.SetBrickPosition(position);
        brick.SetBrickColor(listColorPlayGame[Random.Range(0, listColorPlayGame.Count)]);
        brick.SetStage(this);
        listBrickInStage.Add(brick);
    }

}
