using TMPro;
using UnityEngine;

public class MultiSceneCameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0f, 2f, -10f);

    [Header("Camera Window (관성/감속 없는 리지드 추적)")]
    [Tooltip("이 폭 안에서는 카메라가 절대 움직이지 않습니다. " +
             "플레이어가 이 범위를 벗어나려는 만큼만 카메라가 '즉시, 정확히' 그 거리만큼 이동합니다. " +
             "Lerp처럼 감속하며 안착하지 않기 때문에 멈췄을 때 카메라도 같이 딱 멈춥니다.")]
    public float horizontalWindow = 2.5f;
    public float verticalWindow = 1.5f;

    [Header("Extra Smoothing (선택, 0이면 완전 리지드)")]
    [Tooltip("윈도우를 벗어난 이동 자체에 아주 약간의 부드러움을 주고 싶을 때만 0보다 크게. " +
             "기본은 0(완전 리지드)을 권장 - 값을 넣을수록 다시 예전의 관성 느낌이 조금씩 살아남.")]
    [Range(0f, 1f)] public float extraSmoothing = 0f;

    [Header("Camera Boundary (맵 제한)")]
    [Tooltip("카메라가 갈 수 있는 왼쪽 끝 X 좌표")]
    public float minX = -10f;
    [Tooltip("카메라가 갈 수 있는 오른쪽 끝 X 좌표")]
    public float maxX = 50f;

    // 인스펙터에서 노출하지 않고 내부에서만 관리 (다중 씬 연결 불가 방지)
    private Transform playerTarget;

    private float defaultMinX;
    private float defaultMaxX;

    void Start()
    {
        // 게임 시작 시 인스펙터에 적어둔 원래 맵 크기를 기억하기
        defaultMinX = minX;
        defaultMaxX = maxX;
    }

    void LateUpdate()
    {
        // 1. 플레이어 씬이나 프리팹이 아직 로드되지 않았다면 실시간으로 찾기 시도
        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
            {
                playerTarget = playerObj.transform;

                // 카메라가 플레이어를 찾자마자 부자연스럽게 날아오지 않도록 즉시 이동
                Vector3 startPos = playerTarget.position + offset;
                startPos.x = Mathf.Clamp(startPos.x, minX, maxX);
                transform.position = startPos;
            }
            else
            {
                return;
            }
        }

        Vector3 desiredPosition = playerTarget.position + offset;
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);

        Vector3 nextPos = transform.position;

        // 2. X: 윈도우를 벗어난 만큼만 정확히 이동 (감속 없음 = 관성 없음)
        float xDiff = desiredPosition.x - transform.position.x;
        if (Mathf.Abs(xDiff) > horizontalWindow)
        {
            float excessX = xDiff - Mathf.Sign(xDiff) * horizontalWindow;
            nextPos.x += excessX;
        }

        // 3. Y: 동일한 윈도우 방식 (기존 데드존을 리지드 윈도우로 통합)
        float yDiff = desiredPosition.y - transform.position.y;
        if (Mathf.Abs(yDiff) > verticalWindow)
        {
            float excessY = yDiff - Mathf.Sign(yDiff) * verticalWindow;
            nextPos.y += excessY;
        }

        // 4. extraSmoothing이 0보다 크면 그만큼만 부드럽게, 0이면 완전 리지드로 즉시 적용
        if (extraSmoothing > 0f)
        {
            transform.position = Vector3.Lerp(transform.position, nextPos, (1f - extraSmoothing) * 20f * Time.deltaTime);
        }
        else
        {
            transform.position = nextPos;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        playerTarget = newTarget;
    }

    public void SetBoundary(float newMin, float newMax)
    {
        minX = newMin;
        maxX = newMax;
    }

    public void ResetBoundary()
    {
        minX = defaultMinX;
        maxX = defaultMaxX;
    }
}