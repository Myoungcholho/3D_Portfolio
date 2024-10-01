using UnityEngine;

/// <summary>
// ��ġ�ؼ� ����ϴ� ���� �ƴ�
// �����ؼ� ����ؾ� �Ѵ�.
// �Ÿ��� Mask�� ������� ������ �׸��� ����.
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

        /*// ��ġ�� ����
        position += normal * 0.05f;
        transform.localPosition = position;*/

        // ȸ�� ���� ����
        Vector3 up = Quaternion.Euler(-90, 0, 0) * Vector3.up;
        Quaternion rotation = Quaternion.FromToRotation(up, normal);
        transform.localRotation = rotation;

        Debug.DrawLine(position, position + normal * 2, Color.blue);
    }
}
