using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public static GameOver Instance { get; private set; }

    private Canvas gameOverCanvas;
    private PlayerDeathHandler player;
    private bool gameOverShown = false; // 중복 실행 방지

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 게임 시작 시 GameOver 씬을 미리 로드해둠 (안 보이는 상태로)
        SceneManager.LoadScene("GameOver", LoadSceneMode.Additive);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameOver")
        {
            // 씬 안에서 캔버스 찾아서 캐싱
            foreach (var root in scene.GetRootGameObjects())
            {
                gameOverCanvas = root.GetComponentInChildren<Canvas>(true);
                if (gameOverCanvas != null) break;
            }
            gameOverCanvas.gameObject.SetActive(false); // 평소엔 꺼두기
        }
    }

    private void Update()
    {
        if (gameOverShown) return; // 이미 게임오버 떴으면 더 확인 안 함

        // 플레이어 참조가 없으면 찾아봄 (멀티씬 구조상 나중에 스폰될 수 있으니)
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerDeathHandler>();
            if (player == null) return; // 아직 플레이어가 씬에 없으면 다음 프레임에 재시도
        }

        if (player.IsDead)
        {
            ShowGameOver();
        }
    }

    private void ShowGameOver()
    {
        gameOverShown = true;
        Time.timeScale = 0f;
        gameOverCanvas.gameObject.SetActive(true);
    }

    public void OnRetryOrMenu()
    {
        gameOverShown = false;

        // 다음 판을 위해 플레이어 입력/상태도 같이 리셋
        if (player != null)
        {
            player.RevivePlayer();
        }
        player = null;

        Time.timeScale = 1f;
        gameOverCanvas.gameObject.SetActive(false);
    }
}