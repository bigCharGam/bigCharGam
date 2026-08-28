using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bonfire : MonoBehaviour
{
    [Header("씬에 추가할 때마다 고유하게 지정 (예: Bonfire_0, Bonfire_1)")]
    public string bonfireName = "Bonfire_0";

    private bool playerInRange = false;

    void Awake()
    {
        BonfireManager.Instance.Register(bonfireName, this);
    }

    void OnDestroy()
    {
        // 씬 전환/오브젝트 파괴 시 매니저에서도 정리
        if (BonfireManager.Instance != null)
            BonfireManager.Instance.Unregister(bonfireName);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            SaveAtBonfire();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            BonfireManager.Instance.SetCurrentBonfire(this);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            BonfireManager.Instance.ClearCurrentBonfire(this);
        }
    }

    // public으로 빼서 버튼 OnClick에 바로 연결 가능
    public void SaveAtBonfire()
    {
        var player = GameObject.FindWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("[모닥불 저장] Player를 찾을 수 없습니다. 태그 확인 필요.");
            return;
        }

        var movement = player.GetComponent<PlayerMovement>();
        if (movement == null)
        {
            Debug.LogWarning("[모닥불 저장] PlayerMovement 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        // 화톳불에서 쉬면 체력/기력/포션 개수를 전부 완전 회복
        movement.FullyRestore();
        if (BattleManager.instance != null)
        {
            BattleManager.instance.potionCount = BattleManager.instance.maxPotionCount;
        }

        GameSaveData data = new GameSaveData
        {
            playerHP = movement.currentHealth,
            posX = player.transform.position.x,
            posY = player.transform.position.y,
            posZ = player.transform.position.z,
            currentSceneName = SceneManager.GetActiveScene().name,
            lastBonfireName = bonfireName
        };

        // ---- 레벨 / 경험치 / 스킬 저장 ----
        if (BattleManager.instance != null)
        {
            // BattleManager.cs 확인 결과 레벨 필드 이름은 "level"이 아니라 "expLevel" 이었음.
            data.playerLevel = BattleManager.instance.expLevel;
            data.playerExp = BattleManager.instance.exp;

            data.skillLevels = new List<int>(BattleManager.instance.skillLevels);
            data.skillPoint = BattleManager.instance.skillPoint;
        }
        else
        {
            Debug.LogWarning("[모닥불 저장] BattleManager.instance가 없어 레벨/경험치/스킬을 저장하지 못했습니다.");
        }

        SaveManager.Save(data);
        Debug.Log($"[모닥불 저장] '{bonfireName}' 에서 저장 완료 - {Application.persistentDataPath}");

        // 화톳불 앉으면 적 제거
        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.DeleteAllEnemies();
        }
    }
}