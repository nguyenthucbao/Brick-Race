using MarchingBytes;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Character : NetworkBehaviour
{
    public Animator animator;
    private string currentAnim = "Idle";
    public Transform mesh;
    public SkinnedMeshRenderer meshbody;
    public Transform container;
    public int totalBrick = 0;
    public LayerMask groundLayer;
    public LayerMask stairLayer;

    public StageController stageController;

    public List<Brick> listBrick = new List<Brick>();

    public NetworkVariable<int> colorIndex = new NetworkVariable<int>(0, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<FixedString32Bytes> currentAnimName = new NetworkVariable<FixedString32Bytes>("Idle", 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkObject containerNetObj = container.GetComponent<NetworkObject>();
            if (containerNetObj != null && !containerNetObj.IsSpawned)
            {
                // Spawn và set parent là Player ngay lập tức
                containerNetObj.Spawn();
                containerNetObj.TrySetParent(this.NetworkObject, worldPositionStays: false);
            }
        }

        colorIndex.OnValueChanged += OnColorChanged;
        UpdateMeshColor(colorIndex.Value);

        currentAnimName.OnValueChanged += OnAnimNameChanged;
        PlayAnimationLocal(currentAnimName.Value.ToString());
    }
    private void OnAnimNameChanged(FixedString32Bytes oldAnim, FixedString32Bytes newAnim)
    {
        PlayAnimationLocal(newAnim.ToString());
    }

    // Hàm thực tế bật trigger animation cục bộ trên từng máy
    private void PlayAnimationLocal(string animName)
    {
        if (currentAnim != animName && currentAnim != "Victory")
        {
            animator.ResetTrigger(currentAnim);
            currentAnim = animName;
            animator.SetTrigger(currentAnim);
        }
    }

    // Hàm thay đổi Anim được gọi từ Player.cs
    public void ChangeAnim(string animName)
    {
        // Nếu là Máy chủ sở hữu (Owner Client) và trạng thái thay đổi
        if (IsOwner && currentAnim != animName)
        {
            // 1. Chạy luôn trên máy mình ngay lập tức để mượt (Prediction)
            PlayAnimationLocal(animName);

            // 2. Gửi yêu cầu lên Server để đổi trạng thái cho các máy khác thấy
            UpdateAnimServerRpc(animName);
        }
    }

    // RPC gửi từ Client lên Server để cập nhật trạng thái mạng
    [ServerRpc]
    private void UpdateAnimServerRpc(string animName)
    {
        // Server thay đổi giá trị NetworkVariable -> Tự động đồng bộ xuống TẤT CẢ các Client khác
        currentAnimName.Value = animName;
    }

    private void OnColorChanged(int oldColor, int newColor)
    {
        UpdateMeshColor(newColor);
    }

    private void UpdateMeshColor(int color)
    {
        if (ColorController.Instance != null && meshbody != null)
        {
            meshbody.material = ColorController.Instance.GetMaterialColors(color);
        }
    }


    public bool CanMove(Vector3 newPoint)
    {
        RaycastHit hit;
        if (Physics.Raycast(newPoint, Vector3.down, out hit, 2f, stairLayer))
        {
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            return true;
        }
        return Physics.Raycast(newPoint, Vector3.down, 2f, groundLayer);
    }

    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Brick"))
        {
            Brick brick = other.GetComponent<Brick>();

            // Kiểm tra gạch hợp lệ và trùng màu với Nhân vật (colorIndex.Value)
            if (brick != null && colorIndex.Value == brick.brickColor.Value)
            {
                // Gọi hàm xử lý nhặt gạch tập trung trên Server
                ServerPickUpBrick(brick);
            }
        }
    }

    //private void ServerPickUpBrick(Brick brick)
    //{
    //    Vector3 targetLocalPosition = new Vector3(0f, totalBrick * 0.2f, 0f);

    //    // 1. Tắt Collider
    //    Collider brickCollider = brick.GetComponent<Collider>();
    //    if (brickCollider != null) brickCollider.enabled = false;

    //    NetworkObject brickNetObj = brick.GetComponent<NetworkObject>();
    //    if (brickNetObj != null)
    //    {
    //        // 2. Server tự xử lý transform của mình (Server cũng là một "client" cần update)
    //        brick.transform.SetParent(container);
    //        brick.transform.localPosition = targetLocalPosition;
    //        brick.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

    //        // 3. Gửi RPC để các Client khác đồng bộ theo
    //        //    KHÔNG dùng TrySetParent nữa để tránh race condition
    //        SyncBrickTransformClientRpc(brickNetObj, targetLocalPosition);
    //    }
    //    else
    //    {
    //        //////// Không có networkobject
    //    }

    //    listBrick.Add(brick);
    //    totalBrick++;
    //}
    void Update()
    {
        if (!IsServer) return; // NetworkTransform tự đồng bộ position xuống client

        for (int i = 0; i < listBrick.Count; i++)
        {
            if (listBrick[i] == null) continue;

            Vector3 targetLocal = new Vector3(0f, i * 0.2f, 0f);
            listBrick[i].transform.position = container.TransformPoint(targetLocal);
            listBrick[i].transform.rotation = container.rotation * Quaternion.Euler(0f, 90f, 0f);
        }
    }

    private void ServerPickUpBrick(Brick brick)
    {
        Debug.Log("Nhat brick");

        Collider brickCollider = brick.GetComponent<Collider>();
        if (brickCollider != null) brickCollider.enabled = false;

        Rigidbody rb = brick.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }

        listBrick.Add(brick);
        totalBrick++;

        // Thông báo cho các client tắt collider/rb
        NetworkObject brickNetObj = brick.GetComponent<NetworkObject>();
        if (brickNetObj != null)
        {
            SyncBrickPickedUpClientRpc(brickNetObj);
        }
    }
    [ClientRpc]
    private void SyncBrickPickedUpClientRpc(NetworkObjectReference brickRef)
    {
        if (IsServer) return;

        if (brickRef.TryGet(out NetworkObject brickNetObj))
        {
            Collider col = brickNetObj.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            Rigidbody rb = brickNetObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
            }
        }
    }

    //[ClientRpc]
    //private void SyncBrickTransformClientRpc(NetworkObjectReference brickRef, Vector3 localPos)
    //{
    //    // ClientRpc chạy trên tất cả client KỂ CẢ host, nên cần loại trừ Server/Host
    //    // để tránh chạy 2 lần trên Host
    //    if (IsServer) return;

    //    if (brickRef.TryGet(out NetworkObject brickNetObj))
    //    {
    //        brickNetObj.transform.SetParent(container);
    //        brickNetObj.transform.localPosition = localPos;
    //        brickNetObj.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
    //    }
    //}

    //public void RemoveBrick()
    //{
    //    if (totalBrick > 0)
    //    {
    //        //Destroy(listBrick[totalBrick - 1].gameObject);
    //        listBrick[totalBrick - 1].gameObject.SetActive(false);
    //        EasyObjectPool.instance.ReturnObjectToPool(listBrick[totalBrick - 1].gameObject);
    //        listBrick.RemoveAt(totalBrick - 1);
    //        totalBrick--;
    //    }
    //}


    //if (other.gameObject.tag == "Brick" && colorIndex == other.GetComponent<Brick>().brickColor)
    //{
    //    other.gameObject.transform.SetParent(container);
    //    other.transform.localPosition = new Vector3(0f, (totalBrick - 1) * 0.2f, 0f);
    //    other.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
    //    other.enabled = false;

    //    other.GetComponent<Brick>().RemoveBrick();

    //    totalBrick++;
    //    Brick brick = other.GetComponent<Brick>();
    //    listBrick.Add(other.GetComponent<Brick>());

    //    other.GetComponent<Brick>().stage.CreateNewBrick(other.GetComponent<Brick>().brickPosition);     
    //}


    //if (other.gameObject.tag == "Starter")
    //{
    //    Debug.Log("start");
    //    this.stageController = other.gameObject.GetComponent<Stage>().stage;
    //    other.gameObject.GetComponent<Stage>().stage.CharacterStartGame(colorIndex);
    //    //StageController.Instance.CharacterStartGame(colorIndex);
    //}

    //if (other.gameObject.tag == "FinishPoint")
    //{
    //    GameController.Instance.EndGame(this);
    //}

    //}

    //public bool CanMove(Vector3 newPoint)
    //{
    //    RaycastHit hit;

    //    Debug.Log("Ground" + Physics.Raycast(newPoint, Vector3.down, out hit, 2f, groundLayer));


    //    if (Physics.Raycast(newPoint, Vector3.down, out hit, 2f, stairLayer))
    //    {


    //        int stairColor = hit.collider.gameObject.GetComponent<Stair>().stairColor;

    //        if (colorIndex != stairColor)
    //        {
    //            if (totalBrick > 0)
    //            {
    //                RemoveBrick();
    //                hit.collider.gameObject.GetComponent<Stair>().SetStairColor(colorIndex);
    //            }


    //            return false;
    //        }
    //    }



    //    return Physics.Raycast(newPoint, Vector3.down, groundLayer);
    //}


    //public void SetCharacterColor(int color)
    //{
    //    colorIndex = color;
    //    meshbody.material = ColorController.Instance.GetMaterialColors(colorIndex);
    //}

}
