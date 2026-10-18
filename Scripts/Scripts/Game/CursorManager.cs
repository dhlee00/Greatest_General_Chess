using UnityEngine;

// 커서 상태
public enum CursorState
{
    Default,  // 기본
    Move,     // 기동
    Attack    // 공격
}

public class CursorManager : MonoBehaviour
{
    // 현재 커서 상태
    [SerializeField] CursorState state;
    public CursorState State => state;

    // 커서 텍스처
    [SerializeField] Texture2D[] cursorType;
    public Texture2D[] CursorType => cursorType;

    // 커서 변경
    public void ChangeCursor(CursorState cursorState)
    {
        // 이미 변경하려는 상태인 경우 실행 안함
        if (state == cursorState) return;

        // 커서 변경
        Cursor.SetCursor(CursorType[(int)cursorState], Vector2.zero, CursorMode.Auto);
        
        // 상태 최신화
        state = cursorState;
    }
}
