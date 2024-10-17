using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSensor : MonoBehaviour
{
    /// <summary>
    /// 바닥에 발이 닿았는지 알려주는 델리게이트
    /// </summary>
    public Action<bool> onGround;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            onGround?.Invoke(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            onGround?.Invoke(false);
        }
    }
}
