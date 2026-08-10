using UnityEngine;

public class MainCameraGenerator : MonoBehaviour
{
    [SerializeField] private GameObject mainCameraPrefab;

    void Awake()
    {
        // ���� ���� ī�޶� ���� ���� ����
        if (Camera.main == null)
        {
            if (mainCameraPrefab != null)
            {
                // ������ ������ ������ �ִ� ��ü Transform(��ġ, ȸ��) ���� �״�� ����!
                Transform prefabTransform = mainCameraPrefab.transform;

                Instantiate(
                    mainCameraPrefab,
                    prefabTransform.position,
                    prefabTransform.rotation
                );
            }
            else
            {
                Debug.LogWarning("MainCameraGenerator: ī�޶� �������� ��ϵ��� �ʾҽ��ϴ�.");
            }
        }
    }
}