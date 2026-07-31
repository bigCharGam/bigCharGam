using UnityEngine;

public class EnemyPassToggle : MonoBehaviour
{
    [Header("충돌 제어 대상 (몬스터/보스 Transform)")]
    [SerializeField] private Transform targetTransform;

    [Header("레이어 이름 설정")]
    [SerializeField] private string passLayerName = "PassBoss"; // 실제 프로젝트의 통과 레이어 이름
    [SerializeField] private string defaultLayerName = "BossH";  // 실제 프로젝트의 기본 보스 레이어 이름

    private bool isPassable = false;

    private void Awake()
    {
        if (targetTransform == null)
            targetTransform = transform;
    }

    private void Update()
    {
        // T 키를 누르면 충돌 통과 상태 토글 (ON/OFF)
        if (Input.GetKeyDown(KeyCode.T))
        {
            TogglePassMode();
        }
    }

    public void TogglePassMode()
    {
        isPassable = !isPassable;
        ApplyPassMode(isPassable);
    }

    // ★ 외부(BossHorseRush 스크립트)에서 직접 ON/OFF를 지정할 때 쓰기 딱 좋은 함수입니다!
    public void SetPassMode(bool enablePass)
    {
        isPassable = enablePass;
        ApplyPassMode(isPassable);
    }

    private void ApplyPassMode(bool enable)
    {
        int passLayer = LayerMask.NameToLayer(passLayerName);
        int defaultLayer = LayerMask.NameToLayer(defaultLayerName);

        if (passLayer == -1 || defaultLayer == -1)
        {
            Debug.LogError($"[EnemyPassToggle] 레이어 이름을 확인해주세요: {passLayerName} / {defaultLayerName}");
            return;
        }

        // 레이어를 변경하여 Physics2D Layer Collision Matrix 규칙 적용
        int targetLayer = enable ? passLayer : defaultLayer;
        SetLayerRecursively(targetTransform.gameObject, targetLayer);

        string modeStatus = enable ? "ON (통과 가능)" : "OFF (다시 충돌함)";
        Debug.Log($"[통과 모드] {modeStatus} - 현재 레이어: {LayerMask.LayerToName(targetLayer)}");
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child != null)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
    }
}