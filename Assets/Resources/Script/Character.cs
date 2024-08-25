using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using MarchingBytes;

public class Character : MonoBehaviour
{
    public Animator animator;
    private string currentAnim = "Idle";
    public Transform mesh;
    public int colorIndex = 0;
    public SkinnedMeshRenderer meshbody;
    public Transform container;
    public int totalBrick = 0;
    public LayerMask groundLayer;
    public LayerMask stairLayer;

    public StageController stage;

    private List<Brick> listBrick = new List<Brick>();

    public void RemoveBrick()
    {
        if (totalBrick > 0)
        {
            //Destroy(listBrick[totalBrick - 1].gameObject);
            listBrick[totalBrick - 1].gameObject.SetActive(false);
            EasyObjectPool.instance.ReturnObjectToPool(listBrick[totalBrick - 1].gameObject);
            listBrick.RemoveAt(totalBrick - 1);
            totalBrick--;
        }
    }

    public void ChangeAnim(string animName)
    {
        if (currentAnim != animName && currentAnim != "Victory")
        {
            animator.ResetTrigger(currentAnim);
            currentAnim = animName;
            animator.SetTrigger(currentAnim);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Brick" && colorIndex == other.GetComponent<Brick>().brickColor)
        {
            other.gameObject.transform.SetParent(container);
            other.transform.localPosition = new Vector3(0f, (totalBrick - 1) * 0.2f, 0f);
            other.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            other.enabled = false;

            other.GetComponent<Brick>().RemoveBrick();

            totalBrick++;
            Brick brick = other.GetComponent<Brick>();
            listBrick.Add(other.GetComponent<Brick>());

            other.GetComponent<Brick>().stage.CreateNewBrick(other.GetComponent<Brick>().brickPosition);
            //StageController.Instance.CreateNewBrick(other.GetComponent<Brick>().brickPosition);        
        }

        if (other.gameObject.tag == "Starter")
        {
            Debug.Log("start");
            this.stage = other.gameObject.GetComponent<Stage>().stage;
            other.gameObject.GetComponent<Stage>().stage.CharacterStartGame(colorIndex);
            //StageController.Instance.CharacterStartGame(colorIndex);
        }

        if (other.gameObject.tag == "FinishPoint")
        {
            GameController.Instance.EndGame(this);
        }

    }

    public bool CanMove(Vector3 newPoint)
    {
        RaycastHit hit;

        //Debug.Log("Ground" + Physics.Raycast(newPoint, Vector3.down, out hit, 2f, groundLayer));


        if (Physics.Raycast(newPoint, Vector3.down, out hit, 2f, stairLayer))
        {


            int stairColor = hit.collider.gameObject.GetComponent<Stair>().stairColor;

            if (colorIndex != stairColor)
            {
                if (totalBrick > 0)
                {
                    RemoveBrick();
                    hit.collider.gameObject.GetComponent<Stair>().SetStairColor(colorIndex);
                }

               
                return false;
            }
        }



        return Physics.Raycast(newPoint, Vector3.down, groundLayer);
    }


    public void SetCharacterColor(int color)
    {
        colorIndex = color;
        meshbody.material = ColorController.Instance.GetMaterialColors(colorIndex);
    }

}
