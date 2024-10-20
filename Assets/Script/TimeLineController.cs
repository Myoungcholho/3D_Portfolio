using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

// Player�� �����ϰ� ����
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
    private CameraDistanceAdjuster cameraDistance;          // ī�޶� �Ÿ��� ��������� �������� �ܶ���� ������Ʈ
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
            other.gameObject.SetActive(false);                      // �ε��� Line�� ����

            if (bossStartTimeLine == null)
                return;

            pd.playableAsset = bossStartTimeLine;
            pd.Play();
        }
    }

    // �ñ׳� ȣ�� �޼���
    public void RecevSignalBossTarget()
    {
        bossAI.SetBossTarget(gameObject);
        bossUICanvas.SetActivehpPanel(true);
    }
}
