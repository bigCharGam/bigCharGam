using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    [SerializeField] private GameObject mainCameraPrefab;

    void Awake()
    {
        // 씬에 메인 카메라가 없을 때만 실행
        if (Camera.main == null)
        {
            if (mainCameraPrefab != null)
            {
                // 프리팹 원본이 가지고 있는 자체 Transform(위치, 회전) 정보 그대로 생성!
                Transform prefabTransform = mainCameraPrefab.transform;

                Instantiate(
                    mainCameraPrefab,
                    prefabTransform.position,
                    prefabTransform.rotation
                );
            }
            else
            {
                Debug.LogWarning("GameBootstrapper: 카메라 프리팹이 등록되지 않았습니다.");
            }
        }
    }
}