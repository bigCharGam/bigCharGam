using UnityEngine;

public class AutoParallax : MonoBehaviour
{
    [Header("패럴랙스 설정")]
    [Tooltip("1 = 카메라와 똑같이 이동\n0 = 고정된 땅")]
    [Range(0f, 1f)] public float parallaxFactorX = 0.8f;
    [Range(0f, 1f)] public float parallaxFactorY = 0f;
    public bool isInfinite = true;

    private Transform cam;
    private Vector3 startPos;
    private Vector3 camStartPos;
    private float boundSizeX;

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

        transform.position = new Vector3(
            startPos.x + travelX * parallaxFactorX,
            startPos.y + travelY * parallaxFactorY,
            transform.position.z
        );

        // 2. Factor가 1 미만일 때만 무한 루프를 돌립니다. (1이면 벗어날 일이 없으므로 생략)
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