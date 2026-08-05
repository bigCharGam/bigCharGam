using UnityEngine;

public class AdvancedParallax : MonoBehaviour
{
    [Header("패럴렉스 설정")]
    [Tooltip("-1: 카메라 이동 속도의 2배로 이동, 0: 제자리 고정, 1: 카메라에 완벽히 고정(겹침)")]
    [Range(-1f, 1f)]
    [SerializeField] private float parallaxFactorX = 0.8f;

    [Tooltip("-1: 카메라 이동 속도의 2배로 이동, 0: 제자리 고정, 1: 카메라에 완벽히 고정(겹침)")]
    [Range(-1f, 1f)]
    [SerializeField] private float parallaxFactorY = 0f;

    [SerializeField] private bool isInfinite = true;

    private Transform cameraTransform;
    private Vector3 lastCameraPosition;
    private float textureSizeX;
    private bool isInitialized = false;

    private void Start()
    {
        // Sprite의 가로 길이 계산 (무한 스크롤용)
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            textureSizeX = spriteRenderer.bounds.size.x;
        }

        // MainCamera 자동 연결 시도
        TryInitializeCamera();
    }

    private void LateUpdate()
    {
        // 카메라가 없을 경우 런타임 재검색
        if (cameraTransform == null)
        {
            if (!TryInitializeCamera()) return;
        }

        // 지난 프레임 대비 카메라 이동량 계산
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        // Factor가 1일 때 deltaMovement 만큼 100% 이동하여 카메라와 동기화(완전히 겹침)
        // Factor가 0일 때 이동량 0 (월드 좌표 고정)
        // Factor가 -1일 때 deltaMovement * -1 만큼 이동 (카메라 반대 방향)
        transform.position += new Vector3(
            deltaMovement.x * parallaxFactorX,
            deltaMovement.y * parallaxFactorY,
            0f
        );

        lastCameraPosition = cameraTransform.position;

        // 무한 가로 스크롤 처리
        if (isInfinite && textureSizeX > 0)
        {
            float distanceFromCamera = cameraTransform.position.x - transform.position.x;

            if (Mathf.Abs(distanceFromCamera) >= textureSizeX)
            {
                float offsetPositionX = distanceFromCamera % textureSizeX;
                transform.position = new Vector3(
                    cameraTransform.position.x - offsetPositionX,
                    transform.position.y,
                    transform.position.z
                );
            }
        }
    }

    /// <summary>
    /// MainCamera 태그를 가진 카메라를 찾아 참조를 초기화합니다.
    /// </summary>
    private bool TryInitializeCamera()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
            lastCameraPosition = cameraTransform.position;
            isInitialized = true;
            Debug.Log($"[{gameObject.name}] AdvancedParallax: MainCamera 연결 완료");
            return true;
        }

        Debug.LogWarning($"[{gameObject.name}] AdvancedParallax: MainCamera 태그를 가진 카메라를 찾을 수 없습니다.");
        return false;
    }
}