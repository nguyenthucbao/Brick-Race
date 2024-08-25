using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorController : Singleton<ColorController>
{
    // Start is called before the first frame update

    public enum Color
    {
        Black,       
        Blue,
        Green, 
        LightYellow,     
        Orange,
        Puple,
        Red,
        Sky,
        White,
        Yellow,
    }
    public List<Material> materials = new List<Material>();
    void Start()
    {
        
    }

    public Material GetMaterialColors(int colorIndex)
    {
        return materials[colorIndex];
    }
}
