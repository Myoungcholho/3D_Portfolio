using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

// Player가 참조하고 있음
public class TimeLineController : MonoBehaviour
{
    [SerializeField]
    private TimelineAsset bossStartTimeLine;
    [SerializeField]
    private GameObject playerVirtualCamera;

    private PlayableDirector pd;

    [SerializeField]
    private string bossNameText = "Ganfaul M Aure_Boss";
    private BossAIController bossAI;
    private CameraDistanceAdjuster cameraDistance;          // 카메라 거리가 가까워지면 동적으로 줌땡기는 컴포넌트
    private BossUICanvas bossUICanvas;

    private void Awake()
    {
        pd = GetComponent<PlayableDirector>();

        GameObject obj = GameObject.Find(bossNameText);
        Debug.Assert(obj != null);

        if (obj != null)
        { 
            bossAI = obj.GetComponent<BossAIController>();
            Debug.Assert(bossAI != null);
        }

        cameraDistance = playerVirtualCamera.GetComponent<CameraDistanceAdjuster>();

        bossUICanvas = GameObject.Find("BossCanvas")?.GetComponent<BossUICanvas>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "HammerCinemachine")
        {
            cameraDistance.SetObject(transform, bossAI.transform);
            other.gameObject.SetActive(false);                      // 부딪힌 Line을 제거

            if (bossStartTimeLine == null)
                return;

            pd.playableAsset = bossStartTimeLine;
            pd.Play();
        }
    }

    // 시그널 호출 메서드
    public void RecevSignalBossTarget()
    {
        bossAI.SetBossTarget(gameObject);
        bossUICanvas.SetActivehpPanel(true);
    }
}
