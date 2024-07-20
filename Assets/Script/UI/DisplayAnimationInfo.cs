using TMPro;
using UnityEngine;

public class DisplayAnimationInfo : MonoBehaviour
{
    public string animationInfoTextName = "ImpactTypeText";
    public int layerIndex = 4;

    private TextMeshProUGUI animationInfoText; // AnimationInfoText UI 요소를 참조하기 위한 변수
    private Animator animator;
    private int[] layerIndices = { 3, 4 }; // 4,5번 layer 검사

    private void Awake()
    {
        animator = transform.root.transform.GetComponent<Animator>();
        animationInfoText = transform.FindChildByName(animationInfoTextName).GetComponent<TextMeshProUGUI>();
        Debug.Assert(animationInfoText != null);
    }

    void Start()
    {
        Debug.Assert(animationInfoText != null);
    }

    void Update()
    {
        transform.rotation = Camera.main.transform.rotation;

        string infoText = "";

        foreach (int layerIndex in layerIndices)
        {

            AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(layerIndex);

            //infoText += $"Layer {layerIndex + 1}clip Info\n";

            foreach (var clipInfo in clipInfos)
            {
                string clipName = clipInfo.clip.name;
                float clipLength = clipInfo.clip.length;
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
                float clipProgress = stateInfo.normalizedTime % 1;
                float currentTime = clipProgress * clipLength;

                infoText += $"Clip: {clipName}\n";
                infoText += $"Time: {currentTime:F2}/{clipLength:F2}s \n(run: {clipProgress * 100:F2}%)\n";
                infoText += "-------------------------\n";
            }
        }
        animationInfoText.text = infoText;
    }
}