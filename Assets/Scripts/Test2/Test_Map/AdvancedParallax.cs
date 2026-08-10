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

    [Header("무한 스크롤 - 짝꿍 배경")]
    [Tooltip("나와 번갈아 배치될 짝꿍 배경. 같은 parallaxFactor를 가진 오브젝트여야 합니다.")]
    [SerializeField] private Transform pairedBackground;

    private Transform cameraTransform;
    private Vector3 lastCameraPosition;
    private float textureSizeX;
    private bool isInitialized = false;

    private void Start()
    {
        // 일반 스프라이트 오브젝트인 경우
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            textureSizeX = spriteRenderer.bounds.size.x;
        }
        else
        {
            // Tilemap 오브젝트인 경우 (TilemapBack 등)
            Renderer tilemapRenderer = GetComponent<Renderer>();
            if (tilemapRenderer != null)
            {
                textureSizeX = tilemapRenderer.bounds.size.x;
            }
        }

        if (textureSizeX <= 0f)
        {
            Debug.LogWarning($"[{gameObject.name}] AdvancedParallax: textureSizeX를 계산하지 못했습니다. SpriteRenderer나 TilemapRenderer가 있는지, 실제로 타일이 칠해져 있는지 확인하세요.");
        }

        TryInitializeCamera();
    }

    private void LateUpdate()
    {
        if (cameraTransform == null)
        {
            if (!TryInitializeCamera()) return;
        }

        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        transform.position += new Vector3(
            deltaMovement.x * parallaxFactorX,
            deltaMovement.y * parallaxFactorY,
            0f
        );

        lastCameraPosition = cameraTransform.position;

        // 무한 가로 스크롤 처리: "짝꿍 배경" 기준으로 이어붙이기
        if (isInfinite && textureSizeX > 0 && pairedBackground != null)
        {
            float distanceFromCamera = cameraTransform.position.x - transform.position.x;

            // 카메라가 이 배경을 완전히 지나쳐서(한 텍스처 폭 이상) 화면 밖으로 벗어났을 때만 재배치
            if (Mathf.Abs(distanceFromCamera) >= textureSizeX)
            {
                // 카메라 진행 방향 판단
                bool movingRight = deltaMovement.x > 0f;

                if (movingRight)
                {
                    // 내가 뒤처졌으면(카메라보다 왼쪽), 짝꿍의 오른쪽 끝에 붙인다
                    if (transform.position.x < pairedBackground.position.x)
                    {
                        transform.position = new Vector3(
                            pairedBackground.position.x + textureSizeX,
                            transform.position.y,
                            transform.position.z
                        );
                    }
                }
                else
                {
                    // 카메라가 왼쪽으로 이동 중이면, 짝꿍의 왼쪽 끝에 붙인다
                    if (transform.position.x > pairedBackground.position.x)
                    {
                        transform.position = new Vector3(
                            pairedBackground.position.x - textureSizeX,
                            transform.position.y,
                            transform.position.z
                        );
                    }
                }
            }
        }
    }

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