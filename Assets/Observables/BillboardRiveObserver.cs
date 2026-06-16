using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using DG.Tweening;

public class BillboardRiveObserver : KQObserver
{
    public RiveBillboard billboard;
    float nextSwitch = 5f;
    private void Update()
    {
        if(nextSwitch > 0f)
        {
            nextSwitch -= Time.deltaTime;
            if (nextSwitch <= 0f)
                RotateImage();
        }
    }
    void RotateImage()
    {
        billboard.switchTrigger.property = true;
        nextSwitch = 5f;
    }
}