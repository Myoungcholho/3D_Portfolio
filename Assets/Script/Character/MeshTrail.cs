using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class MeshTrail : MonoBehaviour
{
    private SkinnedMeshRenderer[] skinnedMeshRenderers;     // ĳ���� ��ü ������
    private MeshRenderer[] meshRenderers;                   // ĳ���� ���� ������

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
    private List<GameObject> afterImages = new List<GameObject>(); // �ܻ��� ������ ����Ʈ
    public float transparency = 0.5f; // �ܻ� ���� ����

    // ȣ��� �ܻ� 1ȸ ����
    private void CreateAfterImage(int removeTime = 0)
    {
        if (skinnedMeshRenderers == null)
            skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        if (meshRenderers == null)
            meshRenderers = GetComponentsInChildren<MeshRenderer>();

        foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
        {
            // ���ο� �� GameObject ����
            GameObject gObj = new GameObject("AfterImage");
            gObj.transform.SetPositionAndRotation(skinnedMeshRenderer.transform.position, skinnedMeshRenderer.transform.rotation);

            // MeshRenderer �߰��ϰ� �׸��� ĳ���� ��Ȱ��ȭ
            MeshRenderer mr = gObj.AddComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // MeshFilter �߰�
            MeshFilter mf = gObj.AddComponent<MeshFilter>();

            // ���ο� Mesh�� �����ϰ�, ��Ų�� �޽��� ���� ���¸� ����ũ�Ͽ� ����
            Mesh bakedMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(bakedMesh);

            // MeshFilter�� ����ũ�� Mesh �Ҵ�
            mf.mesh = bakedMesh;

            // Material�� ��Ų�� �޽����� �����Ͽ� MeshRenderer�� �Ҵ�
            mr.materials = skinnedMeshRenderer.materials;

            // ������ ����: Surface Type�� Transparent�� ���� �� ���� ��� ����
            foreach (Material mat in mr.materials)
            {
                // Surface Type�� Transparent�� ����
                mat.SetFloat("_Surface", 1); // 0�� Opaque, 1�� Transparent
                mat.SetFloat("_AlphaClip", 0); // ���� Ŭ���� ����

                // ���� ��带 ���� �������� ����
                mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha); // �ҽ� ����
                mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha); // ��� ����

                // ���� ť�� ���� ��ü�� ť�� ���� (���� ��ü�� 3000 �̻��� ť�� �־�� ��)
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                if (mat.name.Contains("Alpha_Surface"))
                {
                    mat.renderQueue = 3000;
                }
                else if (mat.name.Contains("Alpha_Joints"))
                {
                    mat.renderQueue = 3100; // joint�� ���߿� �׸����� ����
                }

                // ���� �� ����
                Color color = mat.color;
                color.a = 0.5f; // ���İ� ���� (0 = ���� ����, 1 = ������)
                mat.color = color;
            }

            // �ܻ� ����Ʈ�� �߰�
            if (removeTime == 0)
                afterImages.Add(gObj);
            else
                StartCoroutine(FadeOutAfterImages(gObj));


        }

        // MeshRenderer ó�� (���� ��)
        foreach (var meshRenderer in meshRenderers)
        {
            // ���ο� �� GameObject ����
            GameObject gObj = new GameObject("AfterImage");
            gObj.transform.SetPositionAndRotation(meshRenderer.transform.position, meshRenderer.transform.rotation);

            // MeshRenderer �߰��ϰ� �׸��� ĳ���� ��Ȱ��ȭ
            MeshRenderer mr = gObj.AddComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // MeshFilter �߰�
            MeshFilter mf = gObj.AddComponent<MeshFilter>();

            // ���� MeshFilter���� �޽� ����
            mf.mesh = meshRenderer.GetComponent<MeshFilter>().mesh;

            // Material�� MeshRenderer���� �����Ͽ� MeshRenderer�� �Ҵ�
            mr.materials = meshRenderer.materials;

            // ������ ����: Surface Type�� Transparent�� ���� �� ���� ��� ����
            foreach (Material mat in mr.materials)
            {
                // Surface Type�� Transparent�� ����
                mat.SetFloat("_Surface", 1); // 0�� Opaque, 1�� Transparent
                mat.SetFloat("_AlphaClip", 0); // ���� Ŭ���� ����

                // ���� ��带 ���� �������� ����
                mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha); // �ҽ� ����
                mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha); // ��� ����

                // ���� ť�� ���� ��ü�� ť�� ���� (���� ��ü�� 3000 �̻��� ť�� �־�� ��)
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                // �ʿ�� Alpha_Surface�� Alpha_Joints�� ���� �ٸ��� ����
                if (mat.name.Contains("Alpha_Surface"))
                {
                    mat.renderQueue = 3000;
                }
                else if (mat.name.Contains("Alpha_Joints"))
                {
                    mat.renderQueue = 3100; // joint�� ���߿� �׸����� ����
                }

                // ���� �� ����
                Color color = mat.color;
                color.a = transparency; // ���İ� ���� (0 = ���� ����, 1 = ������)
                mat.color = color;
            }

            // �ܻ� ����Ʈ�� �߰�
            if (removeTime == 0)
                afterImages.Add(gObj);
            else
                StartCoroutine(FadeOutAfterImages(gObj));
        }

    }


    public float fadeDuration = 1.0f; // �ܻ��� ������µ� �ɸ��� �ð�

    // �ܻ��� �����ϴ� �ڷ�ƾ
    private IEnumerator FadeOutAfterImages(GameObject afterImage)
    {
        // SpriteRenderer�� MeshRenderer�� ������ ���� �����Ѵٰ� ����
        Renderer renderer = afterImage.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = renderer.material;
            Color color = material.color;
            float startAlpha = color.a;

            // 1�ʵ��� ���İ��� 0���� ������ ���̱�
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float newAlpha = Mathf.Lerp(startAlpha, 0, elapsedTime / fadeDuration);
                color.a = newAlpha;
                material.color = color;
                yield return null;
            }

            // ���İ��� 0�� �Ǿ����� �ܻ� ����
            Destroy(afterImage);
        }
    }

    // ��� �ܻ��� ����, �ִϸ��̼� event���� ȣ�� ��
    private void RemoveAfterImages()
    {
        // List�� �ִ� ��� �ܻ��� ���İ��� ������ ���̴� �ڷ�ƾ ����
        foreach (var afterImage in afterImages)
        {
            StartCoroutine(FadeOutAfterImages(afterImage));
        }
        afterImages.Clear(); // List �ʱ�ȭ
    }

    #endregion
}
