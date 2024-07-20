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