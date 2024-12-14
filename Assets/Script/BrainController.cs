using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class BrainController : MonoBehaviour
{
    private CinemachineBrain brain;

    private string prevBlendStyle;
    private float prevDuration;

    private void Awake()
    {
        brain = GetComponent<CinemachineBrain>();
    }

    public void SetDefaultBlend(string blendStyle, float duration)
    {
        var blend = new CinemachineBlendDefinition();

        // 변환 전 현재 값 저장, 복구 용
        CinemachineBlendDefinition currentBlend = brain.m_DefaultBlend;
        prevBlendStyle = currentBlend.m_Style.ToString();
        prevDuration = currentBlend.m_Time;

        switch (blendStyle)
        {
            case "EaseInOut":
                blend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
                blend.m_Time = duration;
                break;
            case "Cut":
                blend.m_Style = CinemachineBlendDefinition.Style.Cut;
                blend.m_Time = 0f;
                break;
            default:
                blend.m_Style = CinemachineBlendDefinition.Style.Cut;
                blend.m_Time = 0f;
                break;
        }

        brain.m_DefaultBlend = blend;
    }

    public void RollBackBlend()
    {
        StartCoroutine(DelayRollBackBlend());
    }

    private IEnumerator DelayRollBackBlend()
    {
        for(int i=0; i<2; ++i)
            yield return new WaitForEndOfFrame();

        var blend = new CinemachineBlendDefinition();

        switch (prevBlendStyle)
        {
            case "EaseInOut":
                blend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
                blend.m_Time = prevDuration;
                break;
            case "Cut":
                blend.m_Style = CinemachineBlendDefinition.Style.Cut;
                blend.m_Time = prevDuration;
                break;
            default:
                blend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
                blend.m_Time = 2f;
                break;
        }

        // 초기화 및 설정
        prevBlendStyle = "";
        prevDuration = 0f;
        brain.m_DefaultBlend = blend;

        yield return null;
    }
}
