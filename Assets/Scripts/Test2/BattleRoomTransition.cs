using UnityEngine;

public class BattleRoomTransition : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("상호작용에 사용할 키")]
    [SerializeField] private KeyCode interactKey = KeyCode.F;

    [Header("Target References")]
    [Tooltip("비활성화할 오브젝트들의 부모 (Hierarchy에서 'Object' 할당)")]
    [SerializeField] private Transform objectGroupToDeactivate;

    [Tooltip("새로 활성화할 배틀룸 오브젝트 (Hierarchy에서 'BattleRoom01' 할당)")]
    [SerializeField] private GameObject targetBattleRoom;

    // 플레이어가 트리거 안에 있는지 확인하는 플래그
    private bool isPlayerInZone = false;

    private void Start()
    {
        // 시작할 때 혹시 BattleRoom이 켜져있다면 확실하게 꺼줍니다.
        if (targetBattleRoom != null)
        {
            targetBattleRoom.SetActive(false);
        }
    }

    private void Update()
    {
        // 플레이어가 구역 안에 있고, F키를 눌렀을 때
        if (isPlayerInZone && Input.GetKeyDown(interactKey))
        {
            TransitionToBattleRoom();
        }
    }

    // 플레이어가 EnterZone(트리거)에 들어왔을 때
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
            Debug.Log("EnterZone 진입: F 키를 누르면 전투 방으로 이동합니다.");
        }
    }

    // 플레이어가 EnterZone(트리거)에서 나갔을 때
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
        }
    }

    // 씬 전환(활성/비활성) 로직
    private void TransitionToBattleRoom()
    {
        // 1. 배틀룸을 먼저 활성화합니다.
        if (targetBattleRoom != null)
        {
            targetBattleRoom.SetActive(true);
        }

        // 2. Object 산하의 모든 자식들을 비활성화합니다.
        if (objectGroupToDeactivate != null)
        {
            /* 
               주의: 이 스크립트가 붙어있는 EnterZone도 Object의 자식이므로, 
               비활성화되는 순간 이 스크립트의 작동도 정지됩니다.
               따라서 배틀룸 활성화를 무조건 먼저 해주는 것이 안전합니다.
            */
            foreach (Transform child in objectGroupToDeactivate)
            {
                child.gameObject.SetActive(false);
            }

            // 만약 자식들을 하나씩 끄는게 아니라 'Object' 그룹 전체를 통째로 끄고 싶다면
            // 위의 foreach문 대신 아래 한 줄을 사용하셔도 됩니다.
            // objectGroupToDeactivate.gameObject.SetActive(false);
        }

        isPlayerInZone = false;
        Debug.Log("전투 방으로 이동 완료!");
    }
}