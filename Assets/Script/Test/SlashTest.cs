using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Metadata;

public class SlashTest : MonoBehaviour
{
    public GameObject childern;
    private bool bCheck;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SlashOnOFF());   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator SlashOnOFF()
    {
        while(true)
        {
            childern.SetActive(bCheck);
            bCheck = !bCheck;
            yield return new WaitForSeconds(0.8f);
        }
    }
}
