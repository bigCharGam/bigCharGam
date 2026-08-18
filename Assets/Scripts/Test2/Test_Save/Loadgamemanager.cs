using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LoadGameManager : MonoBehaviour
{
    public GameObject loadingScreen;

    public void OnLoadButtonClicked()
    {
        if (!SaveManager.HasSave())
        {
            Debug.LogWarning("[로드] 저장된 파일이 없습니다.");
            return;
        }

        if (loadingScreen != null) loadingScreen.SetActive(true);
        StartCoroutine(LoadScenesAndApplySaveCoroutine());
    }

    private System.Collections.IEnumerator LoadScenesAndApplySaveCoroutine()
    {
        // Single 모드 씬 전환 시 이 오브젝트가 파괴되지 않도록 보호
        DontDestroyOnLoad(gameObject);

        // 저장된 데이터는 씬 전환 전에 미리 읽어둠 (읽기 자체는 씬과 무관)
        GameSaveData data = SaveManager.Load();
        if (data == null)
        {
            Debug.LogError("[로드] 저장 파일을 읽는 데 실패했습니다.");
            if (loadingScreen != null) loadingScreen.SetActive(false);
            Destroy(gameObject);
            yield break;
        }

        // 1. 마스터 씬을 싱글 모드로 먼저 로딩
        AsyncOperation masterOp = SceneManager.LoadSceneAsync("Master", LoadSceneMode.Single);
        while (!masterOp.isDone)
        {
            yield return null;
        }

        // 2. 나머지 씬들을 Additive로 로딩
        List<AsyncOperation> asyncOps = new List<AsyncOperation>();
        asyncOps.Add(SceneManager.LoadSceneAsync("Gameover", LoadSceneMode.Additive));
        asyncOps.Add(SceneManager.LoadSceneAsync("Character", LoadSceneMode.Additive));
        asyncOps.Add(SceneManager.LoadSceneAsync("Map", LoadSceneMode.Additive));
        asyncOps.Add(SceneManager.LoadSceneAsync("BossHorse", LoadSceneMode.Additive));

        // 3. 전부 로딩될 때까지 대기
        bool allScenesLoaded = false;
        while (!allScenesLoaded)
        {
            allScenesLoaded = true;
            foreach (AsyncOperation op in asyncOps)
            {
                if (!op.isDone)
                {
                    allScenesLoaded = false;
                    break;
                }
            }
            yield return null;
        }

        Debug.Log("[로드] 모든 인게임 씬 로딩 완료! 세이브 데이터 적용 시작.");

        // 4. 씬이 다 켜졌으니 이제 플레이어 상태와 위치를 저장된 값으로 덮어씀
        ApplySaveData(data);

        if (loadingScreen != null) loadingScreen.SetActive(false);

        Destroy(gameObject);
    }

    private void ApplySaveData(GameSaveData data)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[로드] Player를 찾을 수 없습니다. 태그 확인 필요.");
            return;
        }

        // HP 복원
        var movement = player.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.currentHealth = data.playerHP;
        }
        else
        {
            Debug.LogWarning("[로드] PlayerMovement 컴포넌트를 찾을 수 없어 HP를 복원하지 못했습니다.");
        }

        // 위치 복원: 화톳불 이름으로 먼저 시도, 실패하면 저장된 좌표로 폴백
        Vector3? bonfirePos = null;
        if (!string.IsNullOrEmpty(data.lastBonfireName) && BonfireManager.Instance != null)
        {
            bonfirePos = BonfireManager.Instance.GetBonfirePosition(data.lastBonfireName);
        }

        if (bonfirePos.HasValue)
        {
            player.transform.position = bonfirePos.Value;
            Debug.Log($"[로드] 화톳불 '{data.lastBonfireName}' 위치로 복원 완료.");
        }
        else
        {
            player.transform.position = new Vector3(data.posX, data.posY, data.posZ);
            Debug.Log("[로드] 화톳불 정보 없음/불일치 - 저장된 좌표로 복원.");
        }

        // ---- 레벨 / 경험치 / 스킬 복원 ----
        if (BattleManager.instance != null)
        {
            // BattleManager.cs 확인 결과 레벨 필드 이름은 "level"이 아니라 "expLevel" 이었음.
            BattleManager.instance.expLevel = data.playerLevel;
            BattleManager.instance.exp = data.playerExp;

            if (data.skillLevels != null)
            {
                for (int i = 0; i < data.skillLevels.Count && i < BattleManager.instance.skillLevels.Length; i++)
                {
                    BattleManager.instance.skillLevels[i] = data.skillLevels[i];
                }
            }
            BattleManager.instance.skillPoint = data.skillPoint;

            // 스킬 UI가 이미 떠 있는 상태였다면 갱신
            if (UIManager.instance != null)
            {
                UIManager.instance.RefreshSkillImages();

                // 경험치 바(HUD)도 저장된 값 기준으로 다시 그려줌
                int[] expTable = BattleManager.instance.expTable;
                if (expTable != null && expTable.Length > 0)
                {
                    int tableIndex = Mathf.Clamp(BattleManager.instance.expLevel, 0, expTable.Length - 1);
                    UIManager.instance.UpdateExp(BattleManager.instance.exp, expTable[tableIndex]);
                }
            }

            Debug.Log("[로드] 레벨/경험치/스킬 복원 완료.");
        }
        else
        {
            Debug.LogWarning("[로드] BattleManager.instance가 없어 레벨/경험치/스킬을 복원하지 못했습니다.");
        }
    }
}