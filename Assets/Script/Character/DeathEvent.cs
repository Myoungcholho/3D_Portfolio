using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class DeathEvent : MonoBehaviour
{
    public AnimationCurve fadeCurve;                        // ����Ƽ �ν����Ϳ��� �Ҵ��� �
    public float fadeDuration = 5f;                        // ��� ����� �ð�
    public string bossDeathParticlePrefab = "BossDeathParticle"; // Resource���� BossDeathParticle���ӿ�����Ʈ�� ã�ƿ� Instantiate�� ������
    public GameObject objectToActivate;                     // Ȱ��ȭ�� ���� ������Ʈ
    public Transform particlePositionTransform;             // ��ƼŬ ������ ��ġ


    private Material[] materials;                           // ��ü�� ���� ��� ���׸������ �����ϱ� ���� �迭
    private float startTime;
    private BossUICanvas bossUICanvas;

    private PlayableDirector playableDirector;

    private void Awake()
    {
        bossUICanvas = GameObject.Find("BossCanvas").GetComponent<BossUICanvas>();
        Debug.Assert(bossUICanvas != null);

        playableDirector = GetComponent<PlayableDirector>();
        // Ÿ�Ӷ��� ���� �� ȣ��� �̺�Ʈ �߰�
        playableDirector.stopped += OnPlayableDirectorStopped;
    }

    void Start()
    {
        // ��� ���׸����� ����
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        materials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            materials[i] = renderers[i].material;
        }
    }

    public void OnDeath()
    {
        // �׳� ���� Transparent�� �����ϴ� �ڵ�
        // Opaque -> Transparent�� �����ϴ°� �´�. ������ ������ �� �����ϱⰡ �����.
        for (int i = 0; i < materials.Length; i++)
        {
            // Opaque ���׸������ Transparent�� ����
            // ���׸����� ǥ�� Ÿ���� Transparent�� ����
            materials[i].SetFloat("_Surface", 1); // 1 = Transparent, 0 = Opaque
            materials[i].SetOverrideTag("RenderType", "Transparent");
            materials[i].renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            // ���� ��带 Alpha �������� ����
            materials[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            materials[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            materials[i].SetInt("_ZWrite", 0);

            // ���ʿ��� Ű���� ������ ��Ȱ��ȭ�ϰ�, ���� ������ Ȱ��ȭ
            materials[i].DisableKeyword("_ALPHATEST_ON");
            materials[i].EnableKeyword("_ALPHABLEND_ON");
            materials[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");

            // ���׸����� ������ ĳ���� ��Ȱ��ȭ
            materials[i].SetShaderPassEnabled("ShadowCaster", false);
        }

        startTime = Time.time;
        StartCoroutine(FadeOut());
        bossUICanvas.StartCoroutineClearPanel();        // boss end UI active
        Invoke("PlayDeathParticle", 2f);
    }

    // ĳ���� ������ ������� Fade ȿ��
    private IEnumerator FadeOut()
    {
        while (Time.time < startTime + fadeDuration)
        {
            float elapsed = (Time.time - startTime) / fadeDuration;
            float t = Mathf.Clamp01(elapsed);

            // ��� ������� ���� ���� ���
            float alpha = fadeCurve.Evaluate(t);

            for (int i = 0; i < materials.Length; i++)
            {
                // right arrow�� �߰��� �ڵ����� �������鼭 ���װ� �߻�
                if (materials[i] == null)
                    continue;

                Color color = materials[i].color;
                color.a = alpha;
                materials[i].color = color;
            }

            yield return null;
        }

        // ������ ���¸� Ȯ���� ����
        foreach (Material material in materials)
        {
            Color color = material.color;
            color.a = fadeCurve.Evaluate(1f);
            //material.color = color;
            material.SetColor("_BaseColor", color);
        }

        // ��Ż ������ TimeLine ����
        playableDirector.Play();
    }
    // ĳ���� ����� �� ��Ÿ�� particle ȿ�� , �������� ���� ��ġ�� ..
    private void PlayDeathParticle()
    {
        // Resources �������� BossDeathParticle �������� �̸����� �ε�
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
    // TimeLine ���� �� Ȱ��ȭ�� ������Ʈ
    private void OnPlayableDirectorStopped(PlayableDirector director)
    {
        // ������Ʈ ��Ȱ��ȭ �Ǵ� ����
        gameObject.SetActive(false);

        // Ÿ�Ӷ����� ����� �� ������Ʈ�� Ȱ��ȭ
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
        }
    }
}