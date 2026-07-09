using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class BattleRoom : MonoBehaviour
{
    [Header("Room Settings")]
    [Tooltip("전투 시작 시 길을 막을 왼쪽 벽")]
    [SerializeField] private GameObject leftWall;
    [Tooltip("전투 시작 시 길을 막을 오른쪽 벽")]
    [SerializeField] private GameObject rightWall;

    // 전투 중에만 화면에 보여줄 전경/오브젝트
    [Tooltip("전투 시작 시 켜지고 클리어 시 꺼지는 오브젝트")]
    [SerializeField] private GameObject battleForeground;

    // 페이드 효과가 걸리는 시간을 설정할 수 있습니다.
    [Header("Fade Settings")]
    [Tooltip("전경이 완전히 나타나거나 사라지는 데 걸리는 시간(초)")]
    [SerializeField] private float fadeDuration = 1.0f;

    // 이 방 안에서 카메라가 움직일 수 있는 한계값
    [Header("Camera Settings")]
    [Tooltip("방 안에서의 카메라 최소 X")]
    [SerializeField] private float roomCameraMinX;
    [Tooltip("방 안에서의 카메라 최대 X")]
    [SerializeField] private float roomCameraMaxX;

    [Header("Enemy List")]
    [Tooltip("이 방에서 처치해야 할 몬스터들을 드래그해서 넣으세요")]
    [SerializeField] private List<EnemyBase> enemiesInRoom = new List<EnemyBase>();

    private bool isRoomActive = false; // 전투가 진행 중인지
    private bool isCleared = false;    // 이미 클리어한 방인지

    // 카메라 스크립트를 담아둘 변수
    private MultiSceneCameraFollow camScript;

    private void Start()
    {
        // 처음에는 플레이어가 지나갈 수 있게 벽을 끄기
        if (leftWall != null) leftWall.SetActive(false);
        if (rightWall != null) rightWall.SetActive(false);

        // 맵이 시작될 때는 이 전경이 안 보이도록 확실하게 꺼둡니다.
        if (battleForeground != null) battleForeground.SetActive(false);

        // 메인 카메라에 달려있는 스크립트를 자동으로 찾아옵니다.
        if (Camera.main != null)
        {
            camScript = Camera.main.GetComponent<MultiSceneCameraFollow>();
        }
    }

    // 플레이어가 트리거(방)에 입장했을 때
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isCleared && !isRoomActive && other.CompareTag("Player"))
        {
            StartBattle();
        }
    }

    private void StartBattle()
    {
        isRoomActive = true;

        // 투명 벽을 켜서 플레이어를 가두기
        if (leftWall != null) leftWall.SetActive(true);
        if (rightWall != null) rightWall.SetActive(true);

        // 전경을 활성화한 뒤, 투명도를 0에서 1로 서서히 올리는 코루틴 실행
        if (battleForeground != null)
        {
            battleForeground.SetActive(true);
            StartCoroutine(FadeForeground(0f, 1f, false));
        }

        // 전투 시작! 등록해둔 전경 오브젝트(TilemapForeground)를 활성화합니다.
        if (battleForeground != null) battleForeground.SetActive(true);

        // 전투 시작 시 카메라 한계값을 방 크기로 좁혀버립니다.
        if (camScript != null)
        {
            camScript.SetBoundary(roomCameraMinX, roomCameraMaxX);
        }

        Debug.Log("전투 시작! 문이 잠겼습니다.");
    }

    // 투명도를 시간에 따라 서서히 변화시키는 코루틴 함수
    private IEnumerator FadeForeground(float startAlpha, float targetAlpha, bool disableAfter)
    {
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            // Mathf.Lerp: 두 숫자 사이의 비율에 맞는 중간값을 부드럽게 찾아주는 함수
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            SetForegroundAlpha(currentAlpha);
            yield return null; // 다음 프레임까지 대기
        }

        // 오차가 발생할 수 있으니 마지막에는 목표 투명도로 확실히 고정합니다.
        SetForegroundAlpha(targetAlpha);

        // 사라지는 연출이 끝났다면, 메모리 절약을 위해 오브젝트를 완전히 비활성화합니다.
        if (disableAfter)
        {
            battleForeground.SetActive(false);
        }
    }

    // TilemapForeground 및 자식들에 포함된 모든 렌더러의 투명도를 한 번에 칠해주는 함수
    private void SetForegroundAlpha(float alpha)
    {
        // 최상위 폴더에 넣기만 해도, 자식 폴더에 있는 모든 타일맵과 스프라이트를 자동으로 찾아옵니다.
        Tilemap[] tilemaps = battleForeground.GetComponentsInChildren<Tilemap>();
        SpriteRenderer[] sprites = battleForeground.GetComponentsInChildren<SpriteRenderer>();

        foreach (var tm in tilemaps)
        {
            Color c = tm.color;
            c.a = alpha; // a(Alpha) 값이 투명도입니다.
            tm.color = c;
        }
        foreach (var sr in sprites)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    private void Update()
    {
        if (isRoomActive && !isCleared)
        {
            CheckEnemies();
        }
    }

    private void CheckEnemies()
    {
        // 리스트에 있는 적들 중, 체력이 다해 Destroy된(null이 된) 녀석들을 리스트에서 제거
        enemiesInRoom.RemoveAll(enemy => enemy == null);

        // 남은 적이 0마리라면 방 클리어
        if (enemiesInRoom.Count == 0)
        {
            ClearRoom();
        }
    }

    private void ClearRoom()
    {
        isCleared = true;
        isRoomActive = false;

        // 벽을 다시 꺼서 길을 열어줍니다.
        if (leftWall != null) leftWall.SetActive(false);
        if (rightWall != null) rightWall.SetActive(false);

        // 전투가 끝났으므로 전경 오브젝트를 다시 꺼버립니다.
        if (battleForeground != null) battleForeground.SetActive(false);

        // 전투 종료 시 카메라를 다시 맵 전체 크기로 풀어줍니다.
        if (camScript != null)
        {
            camScript.ResetBoundary();
        }

        Debug.Log("방 클리어! 다음 구역으로 이동할 수 있습니다.");
    }
}