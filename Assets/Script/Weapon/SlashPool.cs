using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashPool : MonoBehaviour
{
    public GameObject slashPrefab;
    public int poolSize = 10;
    private List<GameObject> poolList;

    
    void Start()
    {
        poolList = new List<GameObject>();
        for(int i=0; i<poolSize; i++) 
        {
            GameObject obj = Instantiate<GameObject>(slashPrefab);
            obj.SetActive(false);
            poolList.Add(obj);
        }
    }

    public GameObject GetPooledObject()
    {
        foreach(GameObject obj in poolList)
        {
            if (!obj.activeInHierarchy)
                return obj;
        }
        return null;
    }

    public void ActivateSlash(Vector3 position, Quaternion rotation)
    {
        GameObject slash = GetPooledObject();
        if (slash != null)
        {
            slash.transform.position = position;
            slash.transform.rotation = rotation;
            slash.SetActive(true);
            StartCoroutine(DeactivateSlash(slash));
        }
    }

    System.Collections.IEnumerator DeactivateSlash(GameObject slash)
    {
        yield return new WaitForSeconds(0.5f);
        slash.SetActive(false);
    }


}
