using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

// Player가 참조하고 있음
public class TimeLineController : MonoBehaviour
{
    [SerializeField]
    private TimelineAsset bossStartTimeLine;

    private PlayableDirector pd;
    private BossAIController bossAI;
    private CameraDistanceAdjuster cameraDistance;
    private BossUICanvas bossUICanvas;

    private void Awake()
    {
        pd = GetComponent<PlayableDirector>();

        GameObject obj = GameObject.Find("Enemy_AI_Boss");
        Debug.Assert(obj != null);
        bossAI = obj.GetComponent<BossAIController>();
        Debug.Assert(bossAI != null);

        GameObject virtualCamera = GameObject.Find("VirtualCamera1");
        Debug.Assert(virtualCamera != null);
        cameraDistance = virtualCamera.GetComponent<CameraDistanceAdjuster>();

        bossUICanvas = GameObject.Find("BossCanvas").GetComponent<BossUICanvas>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "HammerCinemachine")
        {
            cameraDistance.SetObject(transform, bossAI.transform);
            other.gameObject.SetActive(false);

            if (bossStartTimeLine == null)
                return;

            pd.playableAsset = bossStartTimeLine;
            pd.Play();
        }
    }

    public void RecevSignalBossTarget()
    {
        bossAI.SetBossTarget(gameObject);
        bossUICanvas.SetActivehpPanel(true);
    }
}
