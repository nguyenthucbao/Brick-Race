using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Brick : MonoBehaviour
{
    public int brickColor;
    public int brickPosition;
    public StageController stage;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void RemoveBrick()
    {
        stage.listBrickInStage.Remove(this);

    }

    public void SetBrickPosition(int position)
    {
        brickPosition = position;
    }
    public void SetBrickColor(int color)
    {
        brickColor = color;
        GetComponent<MeshRenderer>().material = ColorController.Instance.GetMaterialColors(color);
    }
    public void SetStage(StageController Stage)
    { 
        stage = Stage;
    }

}
