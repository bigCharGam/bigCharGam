using TMPro;
using UnityEngine;

public class MultiSceneCameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0f, 2f, -10f);
    [Range(1f, 10f)]
    public float smoothSpeed = 5f;

    [Header("Camera Boundary (맵 제한)")]
    [Tooltip("카메라가 갈 수 있는 왼쪽 끝 X 좌표")]
    public float minX = -10f;
    [Tooltip("카메라가 갈 수 있는 오른쪽 끝 X 좌표")]
    public float maxX = 50f;

    // 인스펙터에서 노출하지 않고 내부에서만 관리 (다중 씬 연결 불가 방지)
    private Transform playerTarget;

    void LateUpdate()
    {
        // 1. 플레이어 씬이나 프리팹이 아직 로드되지 않았다면 실시간으로 찾기 시도
        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
            {
                // 플레이어를 찾았다면 타겟으로 설정
                playerTarget = playerObj.transform;

                // 카메라가 플레이어를 찾자마자 부자연스럽게 날아오지 않도록 즉시 이동
                Vector3 startPos = playerTarget.position + offset;
                startPos.x = Mathf.Clamp(startPos.x, minX, maxX); // 시작할 때도 맵 밖으로 안 나가게 제한
                transform.position = startPos;
            }
            else
            {
                // 아직 플레이어가 없으면 에러를 내지 않고 이번 프레임 대기
                return;
            }
        }

        // 2. 정상적인 카메라 추적 로직 ( Clamp 로직이 들어갈 올바른 위치)
        Vector3 desiredPosition = playerTarget.position + offset;

        // 목표 위치의 X값을 minX와 maxX 사이로 꼼짝 못하게 가둡니다.
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);

        // 제한된 위치로 부드럽게 이동합니다.
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }

    // 3. (옵션) 매니저 스크립트가 플레이어 생성 직후 직접 타겟을 꽂아줄 때 사용하는 함수
    public void SetTarget(Transform newTarget)
    {
        playerTarget = newTarget;
    }
}