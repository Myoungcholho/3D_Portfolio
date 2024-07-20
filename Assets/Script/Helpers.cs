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
    /// 특정 애니메이터 레이어에 특정 애니메이션 상태가 존재하는지 확인합니다.
    /// </summary>
    /// <param name="animator">검사할 애니메이터</param>
    /// <param name="stateName">확인할 상태(애니메이션 클립) 이름</param>
    /// <param name="layerIndex">확인할 레이어 인덱스</param>
    /// <returns>상태가 존재하면 true, 그렇지 않으면 false</returns>
    public static bool DoesStateExistInLayer(Animator animator, string stateName, int layerIndex)
    {
        if (animator == null)
        {
            Debug.LogError("Animator가 할당되지 않았습니다.");
            return false;
        }

        AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
        if (animatorController == null)
        {
            Debug.LogError("AnimatorController를 찾을 수 없습니다.");
            return false;
        }

        if (layerIndex < 0 || layerIndex >= animatorController.layers.Length)
        {
            Debug.LogError("레이어 인덱스가 잘못되었습니다.");
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
    /// 특정 애니메이터 파라미터가 존재하는지 확인합니다.
    /// </summary>
    /// <param name="animator">검사할 애니메이터</param>
    /// <param name="parameterName">확인할 파라미터 이름</param>
    /// <returns>파라미터가 존재하면 true, 그렇지 않으면 false</returns>
    public static bool DoesParameterExist(Animator animator, string parameterName)
    {
        if (animator == null)
        {
            Debug.LogError("Animator가 할당되지 않았습니다.");
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