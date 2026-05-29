using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : Character
{
    public struct MoveData
    {
        public int tick;
        public Vector3 inputDirection;
        public Vector3 position;
        public Vector3 rotationForward;
    }

    private int currentTick = 0;
    private List<MoveData> moveBuffer = new List<MoveData>();
    private const float MOVE_SPEED = 5f;

    void Update()
    {
        // Chỉ Client sở hữu nhân vật này mới được đọc Input từ Joystick
        if (!IsOwner) return;

        Vector3 direction = JoystickControl.direct.normalized;

        if (direction.magnitude > 0f)
        {
            ChangeAnim("Run");
            mesh.forward = direction;
        }
        else
        {
            ChangeAnim("Idle");
        }
    }

    void FixedUpdate()
    {
        if (!IsSpawned) return;

        if (IsOwner)
        {
            currentTick++;
            Vector3 direction = JoystickControl.direct.normalized;

            // 1. Dự đoán vị trí mới trên Client
            Vector3 nextPosition = transform.position + direction * Time.fixedDeltaTime * MOVE_SPEED;

            if (CanMove(nextPosition))
            {
                transform.position = nextPosition;
            }

            // 2. Lưu lịch sử bao gồm cả hướng xoay hiện tại của mesh
            MoveData currentMove = new MoveData
            {
                tick = currentTick,
                inputDirection = direction,
                position = transform.position,
                rotationForward = mesh.forward // <-- Ghi nhận hướng xoay tại tick này
            };
            moveBuffer.Add(currentMove);

            // 3. Gửi cả Input và Hướng xoay hiện tại lên Server
            SendInputToServerRpc(direction, mesh.forward, currentTick);
        }
    }

    // Server nhận Input và Hướng xoay từ Client gửi lên
    [ServerRpc]
    private void SendInputToServerRpc(Vector3 direction, Vector3 clientForward, int clientTick)
    {
        // Server xử lý di chuyển của bóng nhân vật này trên Server
        Vector3 nextPosition = transform.position + direction * Time.fixedDeltaTime * MOVE_SPEED;

        if (CanMove(nextPosition))
        {
            transform.position = nextPosition;
        }

        // Trên Server, ta cũng xoay mesh của nhân vật này theo Client gửi lên
        mesh.forward = clientForward;

        // Gửi cả vị trí chuẩn và hướng xoay chuẩn từ Server về cho tất cả các Client khác thấy
        ReturnPositionToClientRpc(transform.position, mesh.forward, clientTick);
    }

    // Trả kết quả về để sửa sai (với Owner) và đồng bộ hình ảnh (với các Client khác)
    [ClientRpc]
    private void ReturnPositionToClientRpc(Vector3 serverPosition, Vector3 serverForward, int serverTick)
    {
        if (!IsOwner)
        {
            // --- ĐỐI VỚI CÁC CLIENT KHÁC (PROXY PLYERS) ---
            // Cập nhật cả vị trí và hướng xoay mặt của nhân vật này trên màn hình của họ
            transform.position = serverPosition;
            mesh.forward = serverForward; // <-- Giúp người chơi khác thấy bạn đang xoay hướng nào
            return;
        }

        // --- ĐỐI VỚI CLIENT CHỦ SỞ HỮU (RECONCILIATION) ---
        int index = moveBuffer.FindIndex(m => m.tick == serverTick);
        if (index != -1)
        {
            MoveData historicalMove = moveBuffer[index];

            // Kiểm tra sửa sai vị trí
            if (Vector3.Distance(historicalMove.position, serverPosition) > 0.05f)
            {
                //Debug.LogWarning("Sai số dự đoán vị trí! Đang đồng bộ lại.");//////////////////////////////////
                transform.position = serverPosition;

                // Tính toán lại các frame tiếp theo
                for (int i = index + 1; i < moveBuffer.Count; i++)
                {
                    Vector3 reSimulatedPos = transform.position + moveBuffer[i].inputDirection * Time.fixedDeltaTime * MOVE_SPEED;
                    if (CanMove(reSimulatedPos))
                    {
                        transform.position = reSimulatedPos;
                    }

                    MoveData updatedMove = moveBuffer[i];
                    updatedMove.position = transform.position;
                    moveBuffer[i] = updatedMove;
                }
            }

            // Xóa bỏ lịch sử cũ
            moveBuffer.RemoveRange(0, index + 1);
        }
    }
    private Vector3 CheckGround(Vector3 newPoint)
    {
        RaycastHit hit;
        if (Physics.Raycast(newPoint, Vector3.down, out hit, 2f, groundLayer))
        {
            return hit.point + Vector3.up * 1.3f;
        }
        return transform.position;
    }

    

}
