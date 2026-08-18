using System.Collections.Generic;
using UnityEngine;

public class BonfireManager : MonoBehaviour
{
    public static BonfireManager Instance { get; private set; }

    private Dictionary<string, Bonfire> bonfires = new Dictionary<string, Bonfire>();

    // 플레이어가 지금 상호작용 중인(범위 안에 있는) 화톳불
    private Bonfire currentBonfire;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[화톳불 매니저] 인스턴스가 이미 존재합니다. 중복 오브젝트 삭제.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // 각 Bonfire가 자신의 Awake에서 호출해서 등록
    public void Register(string bonfireName, Bonfire bonfire)
    {
        if (bonfires.ContainsKey(bonfireName))
        {
            Debug.LogWarning($"[화톳불 매니저] 이름 '{bonfireName}' 중복! ({bonfires[bonfireName].name} ↔ {bonfire.name})");
            return;
        }
        bonfires.Add(bonfireName, bonfire);
    }

    public void Unregister(string bonfireName)
    {
        bonfires.Remove(bonfireName);
    }

    public Bonfire GetBonfire(string bonfireName)
    {
        bonfires.TryGetValue(bonfireName, out var bonfire);
        return bonfire;
    }

    // 로드 시 저장된 이름으로 위치 찾을 때 사용
    public Vector3? GetBonfirePosition(string bonfireName)
    {
        if (bonfires.TryGetValue(bonfireName, out var bonfire))
            return bonfire.transform.position;

        Debug.LogWarning($"[화톳불 매니저] 이름 '{bonfireName}'에 해당하는 화톳불을 찾을 수 없음 (씬에 없거나 아직 미등록).");
        return null;
    }

    // "Bonfire_2" 형태의 이름에서 뒤쪽 숫자(진행도 인덱스)만 파싱. 실패 시 0.
    public static int ParseBonfireIndex(string bonfireName)
    {
        if (string.IsNullOrEmpty(bonfireName)) return 0;

        int lastUnderscore = bonfireName.LastIndexOf('_');
        string numberPart = lastUnderscore >= 0 ? bonfireName.Substring(lastUnderscore + 1) : bonfireName;

        return int.TryParse(numberPart, out int index) ? index : 0;
    }

    // Bonfire가 OnTriggerEnter2D에서 호출해서 "지금 이 화톳불과 상호작용 중"이라고 알려줌
    public void SetCurrentBonfire(Bonfire bonfire)
    {
        currentBonfire = bonfire;
    }

    // 범위를 벗어날 때 호출. 벗어난 화톳불이 현재 등록된 화톳불일 때만 해제
    public void ClearCurrentBonfire(Bonfire bonfire)
    {
        if (currentBonfire == bonfire)
            currentBonfire = null;
    }

    // SaveButton의 OnClick()에 이 함수 하나만 연결하면 됨.
    // 실제로 어느 화톳불을 저장할지는 currentBonfire가 알아서 결정.
    public void SaveAtCurrentBonfire()
    {
        if (currentBonfire == null)
        {
            Debug.LogWarning("[화톳불 매니저] 현재 상호작용 중인 화톳불이 없습니다.");
            return;
        }
        currentBonfire.SaveAtBonfire();
    }
}