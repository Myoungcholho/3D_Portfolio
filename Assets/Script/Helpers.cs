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

public static class CameraHelpers
{
    // 체크용 , 스킬의 범위를 무한으로 하면 안되므로
    public static bool GetCursorLocation(float distance, LayerMask mask)
    {
        Vector3 position;
        Vector3 normal;

        return GetCursorLocation(out position, out normal, distance, mask);
    }

    // 유효 위치인지 확인
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

        // 첫 번째 Ray가 Wall 또는 Floor에 충돌하는지 확인
        if (Physics.Raycast(ray, out hit, distance, mask))
        {
            // Wall에 충돌한 경우
            if (((1 << hit.collider.gameObject.layer) & (1 << 15)) != 0)
            {
                // Wall에 부딪힌 경우 아래로 새로운 Ray 발사
                Ray downRay = new Ray(hit.point, Vector3.down);

                Debug.DrawRay(downRay.origin, downRay.direction * distance, Color.yellow, 0.5f);

                // 아래 방향으로 바닥을 향해 Ray 발사
                if (Physics.Raycast(downRay, out hit, distance, mask & (1 << 8)))
                {
                    // 바닥과 충돌한 경우 바닥 지점과 노멀을 반환
                    position = hit.point;
                    normal = hit.normal;

                    // 파란색 Ray로 바닥에 충돌 표시
                    Debug.DrawRay(hit.point, hit.normal * 1f, Color.blue, 2f);

                    return true;
                }
                return false;
            }
            else
            {
                // 바닥에 충돌한 경우 그대로 사용
                position = hit.point;
                normal = hit.normal;

                // 빨간색 Ray로 바닥에 충돌 표시
                Debug.DrawRay(hit.point, hit.normal * 1f, Color.red, 2f);

                return true;
            }
        }
        // Ray가 Wall 또는 Floor에 충돌하지 않았다면 아래로 수직 벡터를 발사
        else
        {
            // 충돌하지 않은 경우: -Vector3.up 방향으로 새로운 레이 발사
            Ray downRay = new Ray(ray.origin + ray.direction * distance, Vector3.down);

            Debug.DrawRay(downRay.origin, downRay.direction * distance, Color.green, 0.5f);

            // 아래 방향으로 Ray 발사해서 지면 확인
            if (Physics.Raycast(downRay, out hit, distance, mask & (1 << 8)))
            {
                // 충돌 지점과 노멀 반환
                position = hit.point;
                normal = hit.normal;

                // 파란색 Ray로 바닥에 충돌 표시
                Debug.DrawRay(hit.point, hit.normal * 1f, Color.blue, 2f);

                return true;
            }

            return false;
        }
    }
}