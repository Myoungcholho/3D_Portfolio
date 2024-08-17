using System.Collections;
using System.ComponentModel;
using UnityEngine;

public class MeshTrail : MonoBehaviour
{
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
    private SkinnedMeshRenderer[] skinnedMeshRenderers;

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

}
