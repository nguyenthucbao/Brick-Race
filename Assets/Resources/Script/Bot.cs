using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bot : Character
{
    public Vector3 target;
    public NavMeshAgent agent;

    Istate<Bot> currentState;
    public bool isDestination => Vector3.Distance(destination, Vector3.right * transform.position.x + Vector3.forward * transform.position.z) < 0.2f;
    private Vector3 destination;
    public bool isRotate = true;




    void Start()
    {
        ChangState(new IdleState());
    }

    public void SetDestination(Vector3 position)
    {
        agent.enabled = true;
        destination = position;
        destination.y = 0;
        agent.SetDestination(position);
    }

    void Update()
    {
        
        currentState.OnExecute(this);
        if(!CanMove(transform.position) && totalBrick == 0 && isRotate)
        {
            isRotate = false;
            ChangState(new IdleState());
        }
    }

    public void ChangState(Istate<Bot> state)
    {
        if (currentState != null)
        {
            currentState.OnExit(this);
        }
        currentState = state;
        if (currentState != null )
        {   
            currentState.OnEnter(this);
        }
        

    }

}
