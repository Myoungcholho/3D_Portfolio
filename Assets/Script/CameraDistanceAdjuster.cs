using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDistanceAdjuster : MonoBehaviour
{
    public Transform player; // �÷��̾��� Transform
    public Transform boss; // ������ Transform
    public CinemachineVirtualCamera virtualCamera; // Cinemachine Virtual Camera

    private Cinemachine3rdPersonFollow thirdPersonFollow;

    public float minDistance = 2f; // ī�޶� ���� ������ ���� ���� �Ÿ�
    public float maxDistance = 20f; // ī�޶� ���� �ָ� ���� ���� �Ÿ�
    public float minCameraDistance = 2f; // �ּ� ī�޶� �Ÿ�
    public float maxCameraDistance = 7f; // �ִ� ī�޶� �Ÿ�

    void Start()
    {
        thirdPersonFollow = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
    }

    void Update()
    {
        float distance = Vector3.Distance(player.position, boss.position);
        float t = Mathf.InverseLerp(maxDistance, minDistance, distance);
        thirdPersonFollow.CameraDistance = Mathf.Lerp(minCameraDistance, maxCameraDistance, t);
    }
}
