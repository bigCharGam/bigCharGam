using UnityEngine;
using UnityEngine.InputSystem;

// 이 스크립트는 "메뉴를 띄우고 싶은 오브젝트"에 붙입니다.
// 해당 오브젝트에는 Collider2D(또는 Collider) + Is Trigger 체크가 필요합니다.
public class InteractMenuTrigger : MonoBehaviour
{
    [Header("팝업으로 켜고 끌 메뉴 UI (씬에 미리 배치된 오브젝트)")]
    [SerializeField] private GameObject menuPanel;

    [Header("플레이어를 구분할 태그")]
    [SerializeField] private string playerTag = "Player";

    private bool isPlayerInRange = false;

    // 범위 안에 들어온 플레이어의 조작 스크립트를 캐싱해둡니다.
    // (PlayerAttack : PlayerMovement : PlayerBattle : PlayerStats 상속 구조이므로,
    //  실제로는 PlayerAttack이 붙어있어도 PlayerMovement로 참조를 잡아 그냥 꺼버릴 수 있습니다.)
    private PlayerMovement playerMovementScript;

    // 현재 열려 있는 메뉴가 "어느 트리거의 것인지"를 저장 (null이면 전부 닫힌 상태).
    // static bool 대신 이렇게 인스턴스를 참조해야, 화톳불이 여러 개일 때 서로 입력을 가로채지 않음.
    private static InteractMenuTrigger activeTrigger = null;

    public static bool IsMenuOpen => activeTrigger != null;

    private void Start()
    {
        // 시작할 땐 메뉴가 꺼져 있어야 함
        if (menuPanel != null)
            menuPanel.SetActive(false);
    }

    private void Update()
    {
        if (activeTrigger == null)
        {
            // 메뉴가 닫혀 있을 때: 범위 안에서 F키를 누르면 메뉴 열기
            if (isPlayerInRange && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                ToggleMenu();
            }
            return;
        }

        // 다른 트리거가 열어놓은 메뉴라면 이 인스턴스는 입력을 무시함
        if (activeTrigger != this) return;

        // ---- 이 트리거가 연 메뉴가 열려 있을 때 ----
        // F(닫기) / 좌클릭 / 우클릭 외의 입력은 여기서 아무것도 하지 않으므로 자동으로 무시됩니다.
        HandleMenuInput();
    }

    private void HandleMenuInput()
    {
        // F키 -> 메뉴 닫기
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            ToggleMenu();
            return;
        }

        if (Mouse.current != null)
        {
            // 마우스 좌클릭
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                OnMenuLeftClick();
                return;
            }

            // 마우스 우클릭
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                OnMenuRightClick();
                return;
            }
        }
    }

    // 메뉴가 열려 있을 때 좌클릭 시 실행할 로직을 여기에 채워 넣으세요.
    private void OnMenuLeftClick()
    {
        // TODO: 메뉴 항목 선택/확인 등
    }

    // 메뉴가 열려 있을 때 우클릭 시 실행할 로직을 여기에 채워 넣으세요.
    private void OnMenuRightClick()
    {
        // TODO: 뒤로가기/취소 등
    }

    private void ToggleMenu()
    {
        if (menuPanel == null)
        {
            Debug.LogWarning("[InteractMenuTrigger] menuPanel이 연결되어 있지 않습니다.");
            return;
        }

        bool newState = !menuPanel.activeSelf;
        menuPanel.SetActive(newState);
        activeTrigger = newState ? this : null;

        // 메뉴가 열리면 플레이어 조작 스크립트를 꺼서 이동을 막고,
        // 메뉴가 닫히면 다시 켭니다.
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = !newState;
        }

        // 메뉴 켜질 때 게임 정지시키고 싶으면 아래 주석 해제
        // Time.timeScale = newState ? 0f : 1f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;

            // 콜라이더가 붙은 오브젝트 기준으로 먼저 찾고, 없으면 부모에서 찾습니다.
            playerMovementScript = other.GetComponent<PlayerMovement>();
            if (playerMovementScript == null)
            {
                playerMovementScript = other.GetComponentInParent<PlayerMovement>();
            }

            // 필요하면 여기서 "F 눌러서 상호작용" 안내 UI 띄우기
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;

            // 범위 벗어나면 메뉴도 자동으로 닫고, 조작 스크립트도 다시 켜줍니다.
            // 단, 이 트리거가 연 메뉴일 때만 처리 (다른 화톳불이 열어놓은 메뉴는 건드리지 않음)
            if (activeTrigger == this)
            {
                if (menuPanel != null) menuPanel.SetActive(false);

                if (playerMovementScript != null)
                {
                    playerMovementScript.enabled = true;
                }

                activeTrigger = null;
            }
        }
    }
}