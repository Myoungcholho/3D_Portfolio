using UnityEngine;
using UnityEngine.Playables;

public class TimeLineController : MonoBehaviour
{
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
            pd.Play();
        }
    }

    public void RecevSignalBossTarget()
    {
        bossAI.SetBossTarget(gameObject);
        bossUICanvas.SetActivehpPanel(true);
    }
}
