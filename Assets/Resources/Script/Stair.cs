using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stair : MonoBehaviour
{
    public int stairColor = 1;


    public void SetStairColor(int color)
    {
        stairColor = color;
        GetComponent<MeshRenderer>().material = ColorController.Instance.GetMaterialColors(color);
    }

}
