using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventPipe
{
    public static event Action OnActionTaken;
    public static void TakeAction()
    {
        OnActionTaken?.Invoke();
    }
    
}
