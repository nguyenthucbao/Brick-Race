using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : Istate<Bot>
{
    float timer = 0f;
    public void OnEnter(Bot bot)
    {
        bot.agent.enabled = false;
        timer = 0f;
        
    }

    public void OnExecute(Bot bot)
    {
        timer += Time.deltaTime;
        if (timer > 0.1f)
        {
            bot.ChangState(new RuningState());
        }
    }

    public void OnExit(Bot bot)
    {

    }
}
