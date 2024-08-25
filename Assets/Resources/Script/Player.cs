using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    
    // Start is called before the first frame update
    void Start()
    {

    }

   

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = JoystickControl.direct;
        direction = direction.normalized;

        if (direction.magnitude > 0f)
        {
            Vector3 newPoint = transform.position + JoystickControl.direct * Time.deltaTime * 5f;
            if (CanMove(newPoint))
            {
                //transform.position = CheckGround(newPoint);
                transform.position = newPoint;
            }

            ChangeAnim("Run");
            mesh.forward = direction;
        }
        else
        {
            ChangeAnim("Idle");
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
