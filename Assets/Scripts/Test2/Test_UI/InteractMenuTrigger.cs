using UnityEngine;

// 이 스크립트는 "메뉴를 띄우고 싶은 오브젝트"에 붙입니다.
// 해당 오브젝트에는 Collider2D(또는 Collider) + Is Trigger 체크가 필요합니다.
public class InteractMenuTrigger : MonoBehaviour
{
    [Header("팝업으로 켜고 끌 메뉴 UI (씬에 미리 배치된 오브젝트)")]
    [SerializeField] private GameObject menuPanel;

    [Header("플레이어를 구분할 태그")]
    [SerializeField] private string playerTag = "Player";

    [Header("상호작용 키")]
    [SerializeField] private KeyCode interactKey = KeyCode.F;

    private bool isPlayerInRange = false;

    private void Start()
    {
        // 시작할 땐 메뉴가 꺼져 있어야 함
        if (menuPanel != null)
            menuPanel.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            ToggleMenu();
        }
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

        // 메뉴 켜질 때 게임 정지시키고 싶으면 아래 주석 해제
        // Time.timeScale = newState ? 0f : 1f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            // 필요하면 여기서 "F 눌러서 상호작용" 안내 UI 띄우기
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;

            // 범위 벗어나면 메뉴도 자동으로 닫고 싶으면 아래 주석 해제
            if (menuPanel != null) menuPanel.SetActive(false);
        }
    }
}