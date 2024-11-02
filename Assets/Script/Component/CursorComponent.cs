using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CursorState
{
    VisibleUnlocked,
    HiddenLocked
}

public class CursorComponent : MonoBehaviour
{
    private CursorState state;

    private int uiWindowsOpen = 0; // ���� UI â�� ������ ����
    public int UIWindowsIpen { get { return uiWindowsOpen; } }

    private void Start()
    {
        HideAndLockCursor();
    }

    // UI â�� ���� �� ȣ���Ͽ� Ŀ���� ���̰� ��
    public void ShowCursorForUI()
    {
        if (uiWindowsOpen == 0)
        {
            ShowAndUnlockCursor();
        }
        uiWindowsOpen++;
    }

    // UI â�� ���� �� ȣ���Ͽ� Ŀ���� ����
    public void HideCursorForUI()
    {
        uiWindowsOpen = Mathf.Max(0, uiWindowsOpen - 1); // ���� â ������ ����
        if (uiWindowsOpen == 0)
        {
            HideAndLockCursor();
        }
    }

    private void ShowAndUnlockCursor()
    {
        if (state == CursorState.VisibleUnlocked)
            return;

        state = CursorState.VisibleUnlocked;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void HideAndLockCursor()
    {
        if (state == CursorState.HiddenLocked)
            return;

        state = CursorState.HiddenLocked;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}