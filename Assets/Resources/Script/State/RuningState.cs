using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuningState : Istate<Bot>
{
    List<Transform> target = new List<Transform>();
    int index = 0;
   
    public void OnEnter(Bot bot)
    {
        bot.ChangeAnim("Run");
        if(bot.stage.GetDestinationOfBot(bot))
        {
            target = bot.stage.GetPathDestination(bot);
            bot.SetDestination(target[0].position);
            index = 0;
        }
        else
        {
            SeekTarget(bot);
        }
        
    }

    public void OnExecute(Bot bot)
    {
        if(bot.isDestination && index == target.Count-1)
        {
            bot.isRotate = true;
            bot.ChangState(new IdleState());        
        }
        else if(bot.isDestination && index < target.Count-1)
        {
            bot.isRotate = true;
            index++;
            bot.SetDestination(target[index].position);
        }
    }

    public void OnExit(Bot bot)
    {
        bot.ChangeAnim("Idle");
    }

    public void SeekTarget(Bot bot)
    {
        if (bot.stage.GetNearestBricks(bot) != null)
        {
            index = 0;
            target.Add(bot.stage.GetNearestBricks(bot));
        }

        else
        {
            bot.ChangState(new IdleState());
        }
        bot.SetDestination(target[0].position);
    }
}
