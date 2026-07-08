using UnityEngine;
using UnityEngine.InputSystem;

// 한국어 주석 유지를 위해 UTF-8 인코딩으로 저장하여 사용하세요.

public enum PlayerState
{
    None,
    MoveLeft
}

public class NewPlayer : MonoBehaviour
{
    [Header("컴포넌트 참조")]
    [SerializeField] private Rigidbody2D rb;

    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("현재 플레이어 상태")]
    [SerializeField] private PlayerState currentState = PlayerState.None;

    // On함수들이 기록하고 Update가 참조할 원시 입력 데이터들
    private bool isWPressed;
    private bool isAPressed;
    private bool isSPressed;
    private bool isDPressed;
    private bool isRPressed;
    private bool isSpacePressed;
    private bool isCtrlPressed;
    private bool isShiftPressed;

    private void Update()
    {
        // 1. switch 조건 판정을 위해 입력 상태를 기준으로 분기 처리
        // 나중에 다른 키(D, Space 등)가 추가되면 이 아래에 조건문이나 복합 판정 로직을 넣기 편해집니다.
        if (isAPressed)
        {
            currentState = PlayerState.MoveLeft;
        }
        else
        {
            currentState = PlayerState.None;
        }
    }

    private void FixedUpdate()
    {
        // 2. switch 문을 사용하여 현재 상태(State)에 따른 독립적인 물리 주입
        switch (currentState)
        {
            case PlayerState.MoveLeft:
                // 왼쪽 방향으로 이동 물리 주입
                rb.linearVelocity = new Vector2(-1f * moveSpeed, rb.linearVelocity.y);
                break;

            case PlayerState.None:
                // 멈춘 상태일 때는 X축 속도를 0으로 고정
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                break;

            default:
                break;
        }
    }

    #region Input System Callbacks

    public void OnW(InputValue value)
    {
        isWPressed = value.isPressed;
        if (value.isPressed) { Debug.Log("W 키를 눌렀습니다."); }
        else { Debug.Log("W 키를 뗐습니다."); }
    }

    public void OnA(InputValue value)
    {
        isAPressed = value.isPressed;
        if (value.isPressed) { Debug.Log("A 키를 눌렀습니다."); }
        else { Debug.Log("A 키를 뗐습니다."); }
    }

    public void OnS(InputValue value)
    {
        isSPressed = value.isPressed;
        if (value.isPressed) { Debug.Log("S 키를 눌렀습니다."); }
        else { Debug.Log("S 키를 뗐습니다."); }
    }

    public void OnD(InputValue value)
    {
        isDPressed = value.isPressed;
        if (value.isPressed) { Debug.Log("D 키를 눌렀습니다."); }
        else { Debug.Log("D 키를 뗐습니다."); }
    }

    public void OnR(InputValue value)
    {
        isRPressed = value.isPressed;
        if (value.isPressed) { Debug.Log("R 키를 눌렀습니다."); }
        else { Debug.Log("R 키를 뗐습니다."); }
    }

    public void OnSpace(InputValue value)
    {
        isSpacePressed = value.isPressed;
        if (value.isPressed) { Debug.Log("Space 키를 눌렀습니다."); }
        else { Debug.Log("Space 키를 뗐습니다."); }
    }

    public void OnCtrl(InputValue value)
    {
        isCtrlPressed = value.isPressed;
        if (value.isPressed) { Debug.Log("Ctrl 키를 눌렀습니다."); }
        else { Debug.Log("Ctrl 키를 뗐습니다."); }
    }

    public void OnShift(InputValue value)
    {
        isShiftPressed = value.isPressed;
        if (value.isPressed) { Debug.Log("Shift 키를 눌렀습니다."); }
        else { Debug.Log("Shift 키를 뗐습니다."); }
    }

    #endregion
}