using UnityEngine;

public class InfiniteParallax : MonoBehaviour
{
    [Header("Parallax Settings")]
    public float multiplierX = 0.8f;
    public float multiplierY = 0f;

    [Header("Infinite Loop Settings")]
    [Tooltip("체크하면 배경이 끊기지 않고 가로로 무한 반복됩니다.")]
    public bool infiniteHorizontal = true;
    [Tooltip("배경 이미지 한 장의 가로 크기 (유니티 Grid 단위 기준)")]
    public float textureSizeX = 38.4f; // 사용하시는 배경 타일 한 덩어리의 가로 길이에 맞게 조절

    private Transform cam;
    private Vector3 lastCameraPosition;
    private bool isInitialized = false;
    private Vector3 targetPosition;

    void LateUpdate()
    {
        if (!isInitialized)
        {
            if (Camera.main != null)
            {
                cam = Camera.main.transform;
                lastCameraPosition = cam.position;
                targetPosition = transform.position;
                isInitialized = true;
            }
            return;
        }
        if (cam == null) { isInitialized = false; return; }

        // 1. 패럴랙스 기본 이동 계산
        Vector3 deltaMovement = cam.position - lastCameraPosition;
        targetPosition += new Vector3(deltaMovement.x * multiplierX, deltaMovement.y * multiplierY, 0);
        transform.position = targetPosition;
        lastCameraPosition = cam.position;

        // 2. [핵심] 무한 루프 순간이동 로직
        if (infiniteHorizontal)
        {
            // 카메라가 배경의 중심에서 배경 크기만큼 벗어났는지 체크
            if (Mathf.Abs(cam.position.x - transform.position.x) >= textureSizeX)
            {
                // 카메라가 우측으로 벗어났다면 배경을 우측으로 한 칸 순간이동
                float offsetPositionX = (cam.position.x - transform.position.x) % textureSizeX;
                targetPosition.x = cam.position.x - offsetPositionX;
                transform.position = new Vector3(targetPosition.x, transform.position.y, transform.position.z);
            }
        }
    }
}