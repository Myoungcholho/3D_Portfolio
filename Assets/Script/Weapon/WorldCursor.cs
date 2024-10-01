using UnityEngine;

/// <summary>
// 배치해서 사용하는 것이 아닌
// 생성해서 사용해야 한다.
// 거리와 Mask를 등록하지 않으면 그리지 않음.
/// </summary>
public class WorldCursor : MonoBehaviour
{
    private float traceDistance;
    public float TraceDistance
    {
        set => traceDistance = value;
    }

    private LayerMask mask;
    public LayerMask Mask
    {
        set => mask = value;
    }


    private void Awake()
    {
        Transform scene = GameObject.Find("Scenes").transform;
        transform.SetParent(scene, false);
    }

    private void Update()
    {
        Vector3 position;
        Vector3 normal;
        if (CameraHelpers.GetCursorLocation(out position, out normal, traceDistance, mask) == false)
            return;

        /*// 위치를 설정
        position += normal * 0.05f;
        transform.localPosition = position;*/

        // 회전 값을 설정
        Vector3 up = Quaternion.Euler(-90, 0, 0) * Vector3.up;
        Quaternion rotation = Quaternion.FromToRotation(up, normal);
        transform.localRotation = rotation;

        Debug.DrawLine(position, position + normal * 2, Color.blue);
    }
}
