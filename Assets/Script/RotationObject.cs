using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationObject : MonoBehaviour
{
    public float rotationSpeed = 90f;  // �ʴ� ȸ�� �ӵ� (�� ����)

    void Update()
    {
        // y���� �������� ���� �࿡�� ������ �ӵ��� ȸ��
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
    }
}
