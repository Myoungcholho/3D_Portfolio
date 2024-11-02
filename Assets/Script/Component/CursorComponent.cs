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

    private int uiWindowsOpen = 0; // 열린 UI 창의 개수를 추적
    public int UIWindowsIpen { get { return uiWindowsOpen; } }

    private void Start()
    {
        HideAndLockCursor();
    }

    // UI 창이 열릴 때 호출하여 커서를 보이게 함
    public void ShowCursorForUI()
    {
        if (uiWindowsOpen == 0)
        {
            ShowAndUnlockCursor();
        }
        uiWindowsOpen++;
    }

    // UI 창이 닫힐 때 호출하여 커서를 숨김
    public void HideCursorForUI()
    {
        uiWindowsOpen = Mathf.Max(0, uiWindowsOpen - 1); // 열린 창 개수를 감소
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