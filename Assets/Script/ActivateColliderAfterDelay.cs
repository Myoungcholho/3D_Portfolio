using System.Collections;
using UnityEngine;

public class ActivateColliderAfterDelay : MonoBehaviour
{
    public float delay = 3f; // ���� �ð� (�� ����)
    private BoxCollider boxCollider;

    private void Awake()
    {
        // BoxCollider ������Ʈ�� ������
        boxCollider = GetComponent<BoxCollider>();

        // �ʱ⿡�� BoxCollider�� ��Ȱ��ȭ
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }
    }

    private void Start()
    {
        // �ڷ�ƾ ����
        StartCoroutine(ActivateCollider());
    }

    private IEnumerator ActivateCollider()
    {
        // ������ �ð� ���� ���
        yield return new WaitForSeconds(delay);

        // BoxCollider Ȱ��ȭ
        if (boxCollider != null)
        {
            boxCollider.enabled = true;
        }
    }
}