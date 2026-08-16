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

        GameSaveData data = new GameSaveData
        {
            playerHP = movement.currentHealth,
            posX = player.transform.position.x,
            posY = player.transform.position.y,
            posZ = player.transform.position.z,
            currentSceneName = SceneManager.GetActiveScene().name,
            lastBonfireName = bonfireName
        };

        SaveManager.Save(data);
        Debug.Log($"[모닥불 저장] '{bonfireName}' 에서 저장 완료 - {Application.persistentDataPath}");
    }
}