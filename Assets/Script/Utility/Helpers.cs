using UnityEditor.Animations;
using UnityEngine;

public static class Extend_TransformHelpers
{
    public static Transform FindChildByName(this Transform transform, string name)
    {
        Transform[] transforms = transform.GetComponentsInChildren<Transform>();

        foreach (Transform t in transforms)
        {
            if (t.gameObject.name.Equals(name))
                return t;
        }

        return null;
    }
}

public static class AnimatorHelper
{
    /// <summary>
    /// Ư�� �ִϸ����� ���̾ Ư�� �ִϸ��̼� ���°� �����ϴ��� Ȯ���մϴ�.
    /// </summary>
    /// <param name="animator">�˻��� �ִϸ�����</param>
    /// <param name="stateName">Ȯ���� ����(�ִϸ��̼� Ŭ��) �̸�</param>
    /// <param name="layerIndex">Ȯ���� ���̾� �ε���</param>
    /// <returns>���°� �����ϸ� true, �׷��� ������ false</returns>
    public static bool DoesStateExistInLayer(Animator animator, string stateName, int layerIndex)
    {
        if (animator == null)
        {
            Debug.LogError("Animator�� �Ҵ���� �ʾҽ��ϴ�.");
            return false;
        }

        AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
        if (animatorController == null)
        {
            Debug.LogError("AnimatorController�� ã�� �� �����ϴ�.");
            return false;
        }

        if (layerIndex < 0 || layerIndex >= animatorController.layers.Length)
        {
            Debug.LogError("���̾� �ε����� �߸��Ǿ����ϴ�.");
            return false;
        }

        AnimatorControllerLayer layer = animatorController.layers[layerIndex];
        foreach (ChildAnimatorState state in layer.stateMachine.states)
        {
            if (state.state.name == stateName)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Ư�� �ִϸ����� �Ķ���Ͱ� �����ϴ��� Ȯ���մϴ�.
    /// </summary>
    /// <param name="animator">�˻��� �ִϸ�����</param>
    /// <param name="parameterName">Ȯ���� �Ķ���� �̸�</param>
    /// <returns>�Ķ���Ͱ� �����ϸ� true, �׷��� ������ false</returns>
    public static bool DoesParameterExist(Animator animator, string parameterName)
    {
        if (animator == null)
        {
            Debug.LogError("Animator�� �Ҵ���� �ʾҽ��ϴ�.");
            return false;
        }

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName)
            {
                return true;
            }
        }

        return false;
    }
}

public static class CameraHelpers
{
    // üũ�� , ��ų�� ������ �������� �ϸ� �ȵǹǷ�
    public static bool GetCursorLocation(float distance, LayerMask mask)
    {
        Vector3 position;
        Vector3 normal;

        return GetCursorLocation(out position, out normal, distance, mask);
    }

    // ��ȿ ��ġ���� Ȯ��
    public static bool GetCursorLocation(out Vector3 position, float distance, LayerMask mask)
    {
        Vector3 normal;

        return GetCursorLocation(out position, out normal, distance, mask);
    }

    //
    public static bool GetCursorLocation(out Vector3 position, out Vector3 normal, float distance, LayerMask mask)
    {
        position = Vector3.zero;
        normal = Vector3.zero;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * distance, Color.green, 0.5f);

        // ù ��° Ray�� Wall �Ǵ� Floor�� �浹�ϴ��� Ȯ��
        if (Physics.Raycast(ray, out hit, distance, mask))
        {
            // Wall�� �浹�� ���
            if (((1 << hit.collider.gameObject.layer) & (1 << 15)) != 0)
            {
                // Wall�� �ε��� ��� �Ʒ��� ���ο� Ray �߻�
                Ray downRay = new Ray(hit.point, Vector3.down);

                Debug.DrawRay(downRay.origin, downRay.direction * distance, Color.yellow, 0.5f);

                // �Ʒ� �������� �ٴ��� ���� Ray �߻�
                if (Physics.Raycast(downRay, out hit, distance, mask & (1 << 8)))
                {
                    // �ٴڰ� �浹�� ��� �ٴ� ������ ����� ��ȯ
                    position = hit.point;
                    normal = hit.normal;

                    // �Ķ��� Ray�� �ٴڿ� �浹 ǥ��
                    Debug.DrawRay(hit.point, hit.normal * 1f, Color.blue, 2f);

                    return true;
                }
                return false;
            }
            else
            {
                // �ٴڿ� �浹�� ��� �״�� ���
                position = hit.point;
                normal = hit.normal;

                // ������ Ray�� �ٴڿ� �浹 ǥ��
                Debug.DrawRay(hit.point, hit.normal * 1f, Color.red, 2f);

                return true;
            }
        }
        // Ray�� Wall �Ǵ� Floor�� �浹���� �ʾҴٸ� �Ʒ��� ���� ���͸� �߻�
        else
        {
            // �浹���� ���� ���: -Vector3.up �������� ���ο� ���� �߻�
            Ray downRay = new Ray(ray.origin + ray.direction * distance, Vector3.down);

            Debug.DrawRay(downRay.origin, downRay.direction * distance, Color.green, 0.5f);

            // �Ʒ� �������� Ray �߻��ؼ� ���� Ȯ��
            if (Physics.Raycast(downRay, out hit, distance, mask & (1 << 8)))
            {
                // �浹 ������ ��� ��ȯ
                position = hit.point;
                normal = hit.normal;

                // �Ķ��� Ray�� �ٴڿ� �浹 ǥ��
                Debug.DrawRay(hit.point, hit.normal * 1f, Color.blue, 2f);

                return true;
            }

            return false;
        }
    }
}