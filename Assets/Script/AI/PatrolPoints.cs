using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public class PatrolPoints : MonoBehaviour
{
    [Header(" - Waypoint Settings")]
    [SerializeField]
    private bool bLoop;
    [SerializeField]
    private bool bReverse;
    [SerializeField]
    private int toIndex;

    [Header(" - Draw Settings")]
    [SerializeField]
    private float drawHeight = 0.1f;
    [SerializeField]
    private Color drawSphereColor = Color.green;
    [SerializeField]
    private Color drawLineColor = Color.magenta;


    /// <summary>
    /// 다음 위치를 반환합니다.
    /// </summary>
    public Vector3 GetMoveToPosition()
    {
        Debug.Assert(toIndex >= 0 && toIndex < transform.childCount);

        return transform.GetChild(toIndex).position;
    }

    /// <summary>
    /// 다음 지점으로 업데이트 합니다.
    /// </summary>
    public void UpdateNextIndex()
    {
        int count = transform.childCount;

        if(bReverse)
        {
            if(toIndex>0)
            {
                toIndex--;
                return;
            }

            if(bLoop)
            {
                toIndex = count - 1;
                return;
            }

            bReverse = false;
            toIndex = 1;
            return;
        }

        if(toIndex < count -1)
        {
            toIndex++;
            return;
        }

        if(bLoop)
        {
            toIndex = 0;
            return;
        }

        bReverse = true;
        toIndex = count - 2;
    }


    private void OnDrawGizmos()
    {
        int count = transform.childCount;

        for (int i = 0; i < count; ++i)
        {
            DrawSphere(i);

            if (i < count - 1)
                DrawLine(i, i + 1);
        }

        if (bLoop)
            DrawLine(count - 1, 0);
    }

    private void DrawSphere(int index)
    {
        Vector3 position = transform.GetChild(index).position + new Vector3(0, drawHeight, 0);

        Gizmos.color = drawSphereColor;
        Gizmos.DrawSphere(position, 0.15f);
    }

    private void DrawLine(int startIndex, int endIndex)
    {
        Transform start = transform.GetChild(startIndex);
        Transform end = transform.GetChild(endIndex);

        Vector3 startPosition = start.position + new Vector3(0, drawHeight, 0);
        Vector3 endPosition = end.position + new Vector3(0, drawHeight, 0);

        Gizmos.color = drawLineColor;
        Gizmos.DrawLine(startPosition, endPosition);
    }
}