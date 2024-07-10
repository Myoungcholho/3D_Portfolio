using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationObject : MonoBehaviour
{
    public float rotationSpeed = 90f;  // 초당 회전 속도 (도 단위)

    void Update()
    {
        // y축을 기준으로 월드 축에서 일정한 속도로 회전
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
    }
}
