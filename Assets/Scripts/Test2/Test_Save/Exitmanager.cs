using UnityEngine;

/// <summary>
/// Exit 버튼에 연결해서 게임을 종료시키는 스크립트.
/// Exit Button 오브젝트(또는 별도의 매니저 오브젝트)에 붙이고,
/// Button 컴포넌트의 OnClick() 이벤트에 ExitGame() 함수를 연결하면 됩니다.
/// </summary>
public class ExitManager : MonoBehaviour
{
    // Exit 버튼 OnClick()에 연결할 함수
    public void ExitGame()
    {
        Debug.Log("게임을 종료합니다.");

#if UNITY_EDITOR
        // 에디터에서는 Application.Quit()이 동작하지 않으므로 플레이 모드를 꺼줌
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}