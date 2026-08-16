using UnityEngine;
using UnityEngine.SceneManagement;

public class Bonfire : MonoBehaviour
{
    public string bonfireID = "Bonfire_01";
    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            SaveAtBonfire();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
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

        GameSaveData data = new GameSaveData
        {
            playerHP = player.GetComponent<PlayerAttack>().currentHealth,
            posX = transform.position.x,
            posY = transform.position.y,
            posZ = transform.position.z,
            currentSceneName = SceneManager.GetActiveScene().name,
            lastBonfireID = bonfireID
        };

        SaveManager.Save(data);
        Debug.Log($"[모닥불 저장] {bonfireID} 에서 저장 완료 - {Application.persistentDataPath}");
    }
}