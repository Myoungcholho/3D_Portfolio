using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDistanceAdjuster : MonoBehaviour
{
    public Transform player; // 플레이어의 Transform
    public Transform boss; // 보스의 Transform
    public CinemachineVirtualCamera virtualCamera; // Cinemachine Virtual Camera

    private Cinemachine3rdPersonFollow thirdPersonFollow;

    public float minDistance = 2f; // 카메라가 가장 가까이 있을 때의 거리
    public float maxDistance = 20f; // 카메라가 가장 멀리 있을 때의 거리
    public float minCameraDistance = 2f; // 최소 카메라 거리
    public float maxCameraDistance = 7f; // 최대 카메라 거리

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
