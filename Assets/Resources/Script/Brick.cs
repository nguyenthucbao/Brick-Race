using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class Brick : NetworkBehaviour
{
    public StageController stage;

    public NetworkVariable<int> brickColor = new NetworkVariable<int>(0, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);
    public NetworkVariable<int> brickPosition = new NetworkVariable<int>(0, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);

    private MeshRenderer meshRenderer;
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }
    public override void OnNetworkSpawn()
    {
        brickColor.OnValueChanged += OnColorChanged;
        UpdateMaterialColor(brickColor.Value);
    }

    public void SetBrickColor(int color)
    {
        if (IsServer)
        {
            brickColor.Value = color;

            UpdateMaterialColor(color);
        }
    }

    private void OnColorChanged(int oldColor, int newColor)
    {
        UpdateMaterialColor(newColor);
    }

    private void UpdateMaterialColor(int color)
    {
        if (ColorController.Instance != null)
        {
            meshRenderer.material = ColorController.Instance.GetMaterialColors(color);
        }
    }

    // Các hàm thiết lập giá trị từ Server
    public void SetBrickPosition(int position)
    {
        if (IsServer) brickPosition.Value = position;
    }

    public void SetStage(StageController Stage)
    {
        stage = Stage;
    }

    public override void OnNetworkDespawn()
    {
        brickColor.OnValueChanged -= OnColorChanged;
    }
}

    //public void SetBrickPosition(int position)
    //{
    //    brickPosition = position;
    //}
    //public void SetBrickColor(int color)
    //{
    //    brickColor = color;
    //    GetComponent<MeshRenderer>().material = ColorController.Instance.GetMaterialColors(color);
    //}
    //public void SetStage(StageController Stage)
    //{ 
    //    stage = Stage;
    //}


