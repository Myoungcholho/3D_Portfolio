using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class DeathEvent : MonoBehaviour
{
    public AnimationCurve fadeCurve;                        // 유니티 인스펙터에서 할당할 곡선
    public float fadeDuration = 5f;                        // 곡선이 적용될 시간
    public string bossDeathParticlePrefab = "BossDeathParticle"; // Resource에서 BossDeathParticle게임오브젝트를 찾아와 Instantiate할 프리팹
    public GameObject objectToActivate;                     // 활성화할 게임 오브젝트
    public Transform particlePositionTransform;             // 파티클 생성할 위치


    private Material[] materials;                           // 객체의 하위 모든 메테리얼들을 저장하기 위한 배열
    private float startTime;
    private BossUICanvas bossUICanvas;

    private PlayableDirector playableDirector;

    private void Awake()
    {
        bossUICanvas = GameObject.Find("BossCanvas").GetComponent<BossUICanvas>();
        Debug.Assert(bossUICanvas != null);

        playableDirector = GetComponent<PlayableDirector>();
        // 타임라인 종료 시 호출될 이벤트 추가
        playableDirector.stopped += OnPlayableDirectorStopped;
    }

    void Start()
    {
        // 모든 메테리얼을 저장
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        materials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            materials[i] = renderers[i].material;
        }
    }

    public void OnDeath()
    {
        // 그냥 단일 Transparent로 변경하는 코드
        // Opaque -> Transparent로 변경하는게 맞다. 일일히 같은걸 다 셋팅하기가 어려움.
        for (int i = 0; i < materials.Length; i++)
        {
            // Opaque 메테리얼들을 Transparent로 변경
            // 메테리얼의 표면 타입을 Transparent로 설정
            materials[i].SetFloat("_Surface", 1); // 1 = Transparent, 0 = Opaque
            materials[i].SetOverrideTag("RenderType", "Transparent");
            materials[i].renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            // 블렌딩 모드를 Alpha 블렌딩으로 설정
            materials[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            materials[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            materials[i].SetInt("_ZWrite", 0);

            // 불필요한 키워드 설정을 비활성화하고, 알파 블렌딩을 활성화
            materials[i].DisableKeyword("_ALPHATEST_ON");
            materials[i].EnableKeyword("_ALPHABLEND_ON");
            materials[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");

            // 메테리얼의 섀도우 캐스팅 비활성화
            materials[i].SetShaderPassEnabled("ShadowCaster", false);
        }

        startTime = Time.time;
        StartCoroutine(FadeOut());
        bossUICanvas.StartCoroutineClearPanel();        // boss end UI active
        Invoke("PlayDeathParticle", 2f);
    }

    // 캐릭터 서서히 사라지는 Fade 효과
    private IEnumerator FadeOut()
    {
        while (Time.time < startTime + fadeDuration)
        {
            float elapsed = (Time.time - startTime) / fadeDuration;
            float t = Mathf.Clamp01(elapsed);

            // 곡선을 기반으로 알파 값을 계산
            float alpha = fadeCurve.Evaluate(t);

            for (int i = 0; i < materials.Length; i++)
            {
                // right arrow가 중간에 자동으로 없어지면서 버그가 발생
                if (materials[i] == null)
                    continue;

                Color color = materials[i].color;
                color.a = alpha;
                materials[i].color = color;
            }

            yield return null;
        }

        // 마지막 상태를 확실히 적용
        foreach (Material material in materials)
        {
            Color color = material.color;
            color.a = fadeCurve.Evaluate(1f);
            //material.color = color;
            material.SetColor("_BaseColor", color);
        }

        // 포탈 열리는 TimeLine 실행
        playableDirector.Play();
    }
    // 캐릭터 사라질 때 나타날 particle 효과 , 쓰러지고 나서 위치를 ..
    private void PlayDeathParticle()
    {
        // Resources 폴더에서 BossDeathParticle 프리팹을 이름으로 로드
        GameObject particlePrefab = Resources.Load<GameObject>(bossDeathParticlePrefab);

        if (particlePrefab != null)
        {
            Instantiate(particlePrefab, particlePositionTransform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("BossDeathParticle prefab not found in Resources folder.");
        }
    }
    // TimeLine 끝날 시 활성화할 오브젝트
    private void OnPlayableDirectorStopped(PlayableDirector director)
    {
        // 오브젝트 비활성화 또는 제거
        gameObject.SetActive(false);

        // 타임라인이 종료된 후 오브젝트를 활성화
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
        }
    }
}