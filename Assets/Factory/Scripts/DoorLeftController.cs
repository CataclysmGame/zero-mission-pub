using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorLeftController : MonoBehaviour
{
    private DoorController parentController;

    private void Awake()
    {
        parentController = GetComponentInParent<DoorController>();
    }
    
    public void OnOpenDone()
    {
        parentController.DisableCollider();
    }
}
