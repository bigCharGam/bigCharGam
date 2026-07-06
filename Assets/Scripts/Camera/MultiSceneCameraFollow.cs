using UnityEngine;

public class MultiSceneCameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0f, 2f, -10f);
    [Range(1f, 10f)]
    public float smoothSpeed = 5f;

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

                // (선택) 카메라가 플레이어를 찾자마자 순간이동하게 하려면 주석 해제
                transform.position = playerTarget.position + offset;
            }
            else
            {
                // 아직 플레이어가 없으면 에러를 내지 않고 이번 프레임 넘기기 (대기)
                return;
            }
        }

        // 2. 정상적인 카메라 추적 로직
        Vector3 desiredPosition = playerTarget.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }

    // 3. (옵션) 매니저 스크립트가 플레이어 생성 직후 직접 타겟을 꽂아줄 때 사용하는 함수
    public void SetTarget(Transform newTarget)
    {
        playerTarget = newTarget;
    }
}