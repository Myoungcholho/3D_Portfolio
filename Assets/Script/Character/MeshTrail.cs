using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class MeshTrail : MonoBehaviour
{
    private SkinnedMeshRenderer[] skinnedMeshRenderers;     // 캐릭터 몸체 복제용
    private MeshRenderer[] meshRenderers;                   // 캐릭터 무기 복제용

    #region BlueTrail
    public float activeTime = 2f;
    [Header("Mesh Related")]
    public float meshRefreshRate = 0.1f;
    public float meshDestoryDelay = 3.0f;
    public Transform positionToSpawn;

    [Header("Shader Related")]
    public Material mat;
    public string shaderVarRef;
    public float shaderVarRate = 0.1f;
    public float shaderVarRefreshRate = 0.05f;


    private bool isTrailActive;

    public void ActivateMeshTrail()
    {
        if (isTrailActive)
            return;

        isTrailActive = true;
        StartCoroutine(ActivateTrail(activeTime));
    }
    private IEnumerator ActivateTrail(float timeActive)
    {
        while(timeActive >0)
        {
            timeActive -= meshRefreshRate;

            if(skinnedMeshRenderers == null)
                skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

            for(int i=0; i<skinnedMeshRenderers.Length; i++)
            {
                GameObject gObj = new GameObject();
                gObj.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

                MeshRenderer mr = gObj.AddComponent<MeshRenderer>();
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                MeshFilter mf = gObj.AddComponent<MeshFilter>();
                
                

                Mesh mesh = new Mesh();
                skinnedMeshRenderers[i].BakeMesh(mesh);

                mf.mesh = mesh;
                mr.material = mat;

                StartCoroutine(AnimateMaterialFloat(mr.material, 0, shaderVarRate, shaderVarRefreshRate));

                Destroy(gObj, meshDestoryDelay);
            }

            yield return new WaitForSeconds(meshRefreshRate);
        }
        isTrailActive = false;
    }

    private IEnumerator AnimateMaterialFloat(Material mat, float goal, float rate, float refreshRate)
    {
        float valueToAnimate = mat.GetFloat(shaderVarRef);

        while(valueToAnimate > goal)
        {
            valueToAnimate -= rate;
            mat.SetFloat(shaderVarRef, valueToAnimate);
            yield return new WaitForSeconds(refreshRate);
        }
    }
    #endregion

    #region BasicTrail
    private List<GameObject> afterImages = new List<GameObject>(); // 잔상을 저장할 리스트
    public float transparency = 0.5f; // 잔상 투명도 설정

    // 호출시 잔상 1회 생성
    private void CreateAfterImage(int removeTime = 0)
    {
        if (skinnedMeshRenderers == null)
            skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        if (meshRenderers == null)
            meshRenderers = GetComponentsInChildren<MeshRenderer>();

        foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
        {
            // 새로운 빈 GameObject 생성
            GameObject gObj = new GameObject("AfterImage");
            gObj.transform.SetPositionAndRotation(skinnedMeshRenderer.transform.position, skinnedMeshRenderer.transform.rotation);

            // MeshRenderer 추가하고 그림자 캐스팅 비활성화
            MeshRenderer mr = gObj.AddComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // MeshFilter 추가
            MeshFilter mf = gObj.AddComponent<MeshFilter>();

            // 새로운 Mesh를 생성하고, 스킨드 메쉬의 현재 상태를 베이크하여 고정
            Mesh bakedMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(bakedMesh);

            // MeshFilter에 베이크된 Mesh 할당
            mf.mesh = bakedMesh;

            // Material을 스킨드 메쉬에서 복사하여 MeshRenderer에 할당
            mr.materials = skinnedMeshRenderer.materials;

            // 반투명 설정: Surface Type을 Transparent로 변경 및 블렌딩 모드 설정
            foreach (Material mat in mr.materials)
            {
                // Surface Type을 Transparent로 설정
                mat.SetFloat("_Surface", 1); // 0은 Opaque, 1은 Transparent
                mat.SetFloat("_AlphaClip", 0); // 알파 클리핑 해제

                // 블렌딩 모드를 알파 블렌딩으로 설정
                mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha); // 소스 알파
                mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha); // 대상 알파

                // 렌더 큐를 투명 객체용 큐로 설정 (투명 객체는 3000 이상의 큐에 있어야 함)
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                if (mat.name.Contains("Alpha_Surface"))
                {
                    mat.renderQueue = 3000;
                }
                else if (mat.name.Contains("Alpha_Joints"))
                {
                    mat.renderQueue = 3100; // joint를 나중에 그리도록 설정
                }

                // 알파 값 조절
                Color color = mat.color;
                color.a = 0.5f; // 알파값 설정 (0 = 완전 투명, 1 = 불투명)
                mat.color = color;
            }

            // 잔상 리스트에 추가
            if (removeTime == 0)
                afterImages.Add(gObj);
            else
                StartCoroutine(FadeOutAfterImages(gObj));


        }

        // MeshRenderer 처리 (무기 등)
        foreach (var meshRenderer in meshRenderers)
        {
            // 새로운 빈 GameObject 생성
            GameObject gObj = new GameObject("AfterImage");
            gObj.transform.SetPositionAndRotation(meshRenderer.transform.position, meshRenderer.transform.rotation);

            // MeshRenderer 추가하고 그림자 캐스팅 비활성화
            MeshRenderer mr = gObj.AddComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // MeshFilter 추가
            MeshFilter mf = gObj.AddComponent<MeshFilter>();

            // 원본 MeshFilter에서 메쉬 복사
            mf.mesh = meshRenderer.GetComponent<MeshFilter>().mesh;

            // Material을 MeshRenderer에서 복사하여 MeshRenderer에 할당
            mr.materials = meshRenderer.materials;

            // 반투명 설정: Surface Type을 Transparent로 변경 및 블렌딩 모드 설정
            foreach (Material mat in mr.materials)
            {
                // Surface Type을 Transparent로 설정
                mat.SetFloat("_Surface", 1); // 0은 Opaque, 1은 Transparent
                mat.SetFloat("_AlphaClip", 0); // 알파 클리핑 해제

                // 블렌딩 모드를 알파 블렌딩으로 설정
                mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha); // 소스 알파
                mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha); // 대상 알파

                // 렌더 큐를 투명 객체용 큐로 설정 (투명 객체는 3000 이상의 큐에 있어야 함)
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                // 필요시 Alpha_Surface와 Alpha_Joints에 따라 다르게 설정
                if (mat.name.Contains("Alpha_Surface"))
                {
                    mat.renderQueue = 3000;
                }
                else if (mat.name.Contains("Alpha_Joints"))
                {
                    mat.renderQueue = 3100; // joint를 나중에 그리도록 설정
                }

                // 알파 값 조절
                Color color = mat.color;
                color.a = transparency; // 알파값 설정 (0 = 완전 투명, 1 = 불투명)
                mat.color = color;
            }

            // 잔상 리스트에 추가
            if (removeTime == 0)
                afterImages.Add(gObj);
            else
                StartCoroutine(FadeOutAfterImages(gObj));
        }

    }


    public float fadeDuration = 1.0f; // 잔상이 사라지는데 걸리는 시간

    // 잔상을 제거하는 코루틴
    private IEnumerator FadeOutAfterImages(GameObject afterImage)
    {
        // SpriteRenderer나 MeshRenderer의 재질이 투명도 지원한다고 가정
        Renderer renderer = afterImage.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = renderer.material;
            Color color = material.color;
            float startAlpha = color.a;

            // 1초동안 알파값을 0으로 서서히 줄이기
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float newAlpha = Mathf.Lerp(startAlpha, 0, elapsedTime / fadeDuration);
                color.a = newAlpha;
                material.color = color;
                yield return null;
            }

            // 알파값이 0이 되었으면 잔상 제거
            Destroy(afterImage);
        }
    }

    // 모든 잔상을 제거, 애니메이션 event에서 호출 중
    private void RemoveAfterImages()
    {
        // List에 있는 모든 잔상의 알파값을 서서히 줄이는 코루틴 시작
        foreach (var afterImage in afterImages)
        {
            StartCoroutine(FadeOutAfterImages(afterImage));
        }
        afterImages.Clear(); // List 초기화
    }

    #endregion
}
