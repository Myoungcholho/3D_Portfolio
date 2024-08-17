using System.Collections;
using UnityEngine;

public class ActivateColliderAfterDelay : MonoBehaviour
{
    public float delay = 3f; // 지연 시간 (초 단위)
    private BoxCollider boxCollider;

    private void Awake()
    {
        // BoxCollider 컴포넌트를 가져옴
        boxCollider = GetComponent<BoxCollider>();

        // 초기에는 BoxCollider를 비활성화
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }
    }

    private void Start()
    {
        // 코루틴 시작
        StartCoroutine(ActivateCollider());
    }

    private IEnumerator ActivateCollider()
    {
        // 지정된 시간 동안 대기
        yield return new WaitForSeconds(delay);

        // BoxCollider 활성화
        if (boxCollider != null)
        {
            boxCollider.enabled = true;
        }
    }
}