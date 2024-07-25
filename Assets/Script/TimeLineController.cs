using UnityEngine;
using UnityEngine.Playables;

public class TimeLineController : MonoBehaviour
{
    private PlayableDirector pd;
    private BossAIController bossAI;

    private void Awake()
    {
        pd = GetComponent<PlayableDirector>();
        bossAI = GameObject.Find("Enemy_AI_Boss").GetComponent<BossAIController>();
        Debug.Assert(bossAI != null);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "HammerCinemachine")
        {
            other.gameObject.SetActive(false);
            pd.Play();
        }
    }

    public void RecevSignalBossTarget()
    {
        bossAI.SetBossTarget(gameObject);
    }
}
