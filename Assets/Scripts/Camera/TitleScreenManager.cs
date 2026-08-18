using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class TitleScreenManager : MonoBehaviour
{
    public GameObject loadingScreen;

    [Header("씬 설정 - 인스펙터에서 씬 파일을 드래그해서 등록")]
    [Tooltip("Single 모드로 먼저 로드될 마스터 씬")]
    public SceneField masterScene;

    [Tooltip("마스터 씬 로드 후 Additive로 함께 로드할 씬들. + 버튼으로 슬롯을 늘리고 씬 파일을 드래그하면 됨")]
    public List<SceneField> additiveScenes = new List<SceneField>();

    public void OnStartButtonClicked()
    {
        loadingScreen.SetActive(true);
        StartCoroutine(LoadScenesCoroutine());
    }

    private IEnumerator LoadScenesCoroutine()
    {
        // Single 모드 씬 전환 시 이 오브젝트가 파괴되지 않도록 보호
        // 이걸 안 넣으면 마스터씬을 Single로 부를 때 TitleScene가 파괴되어 코루틴 끊김
        DontDestroyOnLoad(gameObject);

        // 1. 마스터 씬을 싱글 모드로 먼저 로딩
        AsyncOperation masterOp = SceneManager.LoadSceneAsync(masterScene, LoadSceneMode.Single);
        while (!masterOp.isDone)
        {
            yield return null;
        }

        // 2. 동시에 로딩할 씬들의 비동기 오퍼레이션(로딩 상태 파악용)을 담을 리스트
        List<AsyncOperation> asyncOps = new List<AsyncOperation>();

        // 인스펙터에 등록해둔 씬들을 전부 Additive로 로딩 지시 (지시만 내리고 바로 다음 줄로 넘어감)
        foreach (SceneField scene in additiveScenes)
        {
            asyncOps.Add(SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive));
        }

        // 3. 등록된 씬이 '전부 다' 로딩될 때까지 코루틴을 붙잡아둡니다.
        bool allScenesLoaded = false;
        while (!allScenesLoaded)
        {
            allScenesLoaded = true; // 일단 다 됬다고 가정하고 검사 시작

            foreach (AsyncOperation op in asyncOps)
            {
                if (!op.isDone)
                {
                    allScenesLoaded = false;
                    Debug.Log("로딩 중");
                    break;
                }
            }

            yield return null; // 다음 프레임까지 대기 후 다시 검사
        }

        // 4. 이 줄에 도달했다는 것은 등록된 씬들이 전부 켜졌다는 뜻입니다!
        Debug.Log("모든 인게임 씬 로딩 완료! 게임을 시작합니다.");
        if (loadingScreen != null) //null체크 안하면 유니티가 경고띄움
        {
            loadingScreen.SetActive(false);
        }

        // 여기서 실제로 캐릭터의 중력을 켜거나, 페이드아웃 연출을 끝내고 게임을 시작시키면 됩니다.
        StartActualGame();

        // 역할이 끝났으므로 DontDestroyOnLoad 씬에 남지 않도록 자기 자신을 파괴
        Destroy(gameObject);
    }

    private void StartActualGame()
    {
        // 여기에 로딩 완료 후 처리할 로직 작성 (예: 플레이어 움직임 활성화 등)
    }
}