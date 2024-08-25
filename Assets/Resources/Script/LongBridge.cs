using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongBridge : MonoBehaviour
{
    public List<Stair> listStair = new List<Stair>();
    public int GetTotalBrickColor(int color)
    {
        int count = 0;
        for (int i = 0; i < listStair.Count; i++)
        {
            if (color == listStair[i].GetComponent<Stair>().stairColor)
            {
                count++;
            }
        }
        return count;
    }
}
