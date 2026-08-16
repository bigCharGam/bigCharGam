using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 플레이 중(Master/Gameover/Character/Map 씬이 로드된 상태)에
/// "타이틀로 나가기" 버튼을 눌렀을 때 Title 씬으로 복귀시키는 매니저.
///
/// TitleScreenManager의 로딩 흐름을 반대로 응용한 버전입니다.
/// 차이점: 돌아갈 때는 씬을 "Single" 모드로 로딩하기만 하면
/// Unity가 현재 로드된 다른 씬들(Master, Gameover, Character, Map)을
/// 자동으로 전부 언로드해줍니다. 그래서 TitleScreenManager처럼
/// 여러 개의 AsyncOperation을 리스트에 담아 개별로 언로드 완료를
/// 체크해줄 필요가 없습니다. (Additive로 "쌓았던" 것과 반대로,
/// Single 로딩은 "정리 + 교체"를 한 번에 해줍니다.)
/// </summary>
public class GameToTitleManager : MonoBehaviour
{
    [Tooltip("씬 전환 중 보여줄 로딩 스크린. " +
             "이 오브젝트가 현재 씬(예: Master)에 속해 있으면 " +
             "Single 모드 전환 시 함께 파괴되어 버리니 주의하세요. " +
             "loadingScreen을 이 매니저의 자식으로 두거나, " +
             "별도로 DontDestroyOnLoad 처리된 오브젝트로 준비해두는 것을 권장합니다.")]
    public GameObject loadingScreen;

    [Tooltip("돌아갈 타이틀 씬 이름")]
    public string titleSceneName = "Title";

    public void OnExitToTitleButtonClicked()
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
        }

        StartCoroutine(LoadTitleSceneCoroutine());
    }

    private System.Collections.IEnumerator LoadTitleSceneCoroutine()
    {
        // Single 모드 씬 전환 시 이 오브젝트(및 loadingScreen이 자식이라면 함께)가
        // 파괴되지 않도록 보호. TitleScreenManager와 동일한 이유입니다.
        DontDestroyOnLoad(gameObject);

        // Title 씬을 Single 모드로 로딩하면 현재 열려 있는 모든 씬
        // (Master, Gameover, Character, Map 등)이 자동으로 언로드되고
        // Title 씬 하나만 남습니다. 그래서 언로드용 AsyncOperation 리스트나
        // "전부 언로드됐는지" 체크하는 while 루프가 따로 필요 없습니다.
        AsyncOperation titleOp = SceneManager.LoadSceneAsync(titleSceneName, LoadSceneMode.Single);
        while (!titleOp.isDone)
        {
            yield return null;
        }

        Debug.Log("타이틀 씬으로 복귀 완료!");

        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }

        // 역할이 끝났으므로 DontDestroyOnLoad 씬에 남지 않도록 자기 자신을 파괴
        Destroy(gameObject);
    }
}
