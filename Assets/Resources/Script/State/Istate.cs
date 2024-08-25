using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Istate<T>
{
    void OnEnter(T t);


    void OnExecute(T t);


    void OnExit(T t);
  


}
