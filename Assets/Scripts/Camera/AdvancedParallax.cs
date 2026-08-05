using UnityEngine;

public class AutoParallax : MonoBehaviour
{
    [Header("패럴랙스 설정")]
    [Tooltip("1 = 카메라와 똑같이 이동\n0 = 고정된 땅")]
    [Range(0f, 1f)] public float parallaxFactorX = 0.8f;
    [Range(0f, 1f)] public float parallaxFactorY = 0f;
    public bool isInfinite = true;

    [Header("미세 떨림 방지 (Jitter Threshold)")]
    [Tooltip("카메라가 이 값보다 적게 움직였을 땐 '움직이지 않은 것'으로 취급합니다. " +
             "Cinemachine PixelPerfect나 물리 기반 캐릭터의 서브픽셀 떨림이 배경에 그대로 반영되는 걸 막아줍니다.")]
    public float movementThreshold = 0.01f;

    private Transform cam;
    private Vector3 startPos;
    private Vector3 camStartPos;
    private float boundSizeX;

    // 마지막으로 '유의미하다고 판단해 반영한' 카메라 이동량 (스냅 방지용 기준점)
    private float lastAppliedTravelX;
    private float lastAppliedTravelY;

    void Start()
    {
        cam = Camera.main.transform;
        // 배경과 카메라의 '초기 시작 위치'를 각각 저장해둡니다.
        startPos = transform.position;
        camStartPos = cam.position;

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            boundSizeX = renderer.bounds.size.x;
        }
        else
        {
            isInfinite = false;
        }
    }

    void LateUpdate()
    {
        if (cam == null) return;

        // 1. 카메라가 시작 지점에서 '이동한 순수 거리'만 계산합니다. (좌표 버그 원천 차단)
        float travelX = cam.position.x - camStartPos.x;
        float travelY = cam.position.y - camStartPos.y;

        // 2. 직전에 반영한 값과 비교해서, 변화량이 threshold보다 작으면 무시합니다.
        //    (카메라의 서브픽셀 떨림이 배경에 그대로 곱해져 노이즈로 보이는 것을 방지)
        if (Mathf.Abs(travelX - lastAppliedTravelX) >= movementThreshold)
        {
            lastAppliedTravelX = travelX;
        }
        if (Mathf.Abs(travelY - lastAppliedTravelY) >= movementThreshold)
        {
            lastAppliedTravelY = travelY;
        }

        transform.position = new Vector3(
            startPos.x + lastAppliedTravelX * parallaxFactorX,
            startPos.y + lastAppliedTravelY * parallaxFactorY,
            transform.position.z
        );

        // 3. Factor가 1 미만일 때만 무한 루프를 돌립니다. (1이면 벗어날 일이 없으므로 생략)
        if (isInfinite && boundSizeX > 0 && parallaxFactorX < 1f)
        {
            float distance = cam.position.x - transform.position.x;

            if (distance > boundSizeX)
            {
                startPos.x += boundSizeX;
            }
            else if (distance < -boundSizeX)
            {
                startPos.x -= boundSizeX;
            }
        }
    }
}