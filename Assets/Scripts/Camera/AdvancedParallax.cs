using UnityEngine;

public class AutoParallax : MonoBehaviour
{
    [Header("패럴랙스 설정")]
    [Tooltip("1 = 카메라와 똑같이 이동\n0 = 고정된 땅")]
    [Range(0f, 1f)] public float parallaxFactorX = 0.8f;
    [Range(0f, 1f)] public float parallaxFactorY = 0f;

    [Header("미세 떨림 방지 (Jitter Threshold)")]
    [Tooltip("카메라가 이 값보다 적게 움직였을 땐 '움직이지 않은 것'으로 취급합니다. " +
             "Cinemachine PixelPerfect나 물리 기반 캐릭터의 서브픽셀 떨림이 배경에 그대로 반영되는 걸 막아줍니다.")]
    public float movementThreshold = 0.01f;

    private Transform cam;
    private Vector3 startPos;
    private Vector3 camStartPos;
    private bool initialized = false;

    // 마지막으로 '유의미하다고 판단해 반영한' 카메라 이동량 (스냅 방지용 기준점)
    private float lastAppliedTravelX;
    private float lastAppliedTravelY;

    void Start()
    {
        cam = Camera.main.transform;
        startPos = transform.position;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        // Cinemachine 등 카메라 컨트롤러가 LateUpdate에서 카메라를 옮기는 경우를 대비해
        // 기준점(camStartPos)은 첫 프레임이 아니라, 카메라가 실제로 자리 잡은 다음 프레임에 캡처합니다.
        if (!initialized)
        {
            camStartPos = cam.position;
            initialized = true;
            return;
        }

        // 1. 카메라가 시작 지점에서 '이동한 순수 거리'만 계산합니다.
        float travelX = cam.position.x - camStartPos.x;
        float travelY = cam.position.y - camStartPos.y;

        // 2. 직전에 반영한 값과 비교해서, 변화량이 threshold보다 작으면 무시합니다.
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
    }
}