using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// 인스펙터에서 씬 파일을 직접 드래그해서 지정할 수 있게 해주는 클래스
// SceneManager.LoadScene 계열 함수에 그대로 넘기면 씬 이름 문자열로 자동 변환됨
[System.Serializable]
public class SceneField
{
    [SerializeField]
    private Object sceneAsset;

    [SerializeField]
    private string sceneName = "";

    public string SceneName => sceneName;

    // SceneField를 string이 필요한 곳에 그냥 넣으면 자동으로 씬 이름으로 변환됨
    public static implicit operator string(SceneField sceneField) => sceneField != null ? sceneField.sceneName : "";
}

#if UNITY_EDITOR
// 인스펙터에 "씬 오브젝트 드래그 슬롯" 형태로 그려주는 부분 (에디터에서만 동작, 빌드에는 포함 안 됨)
[CustomPropertyDrawer(typeof(SceneField))]
public class SceneFieldPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, GUIContent.none, property);

        SerializedProperty sceneAsset = property.FindPropertyRelative("sceneAsset");
        SerializedProperty sceneName = property.FindPropertyRelative("sceneName");

        position = EditorGUI.PrefixLabel(position, label);

        if (sceneAsset != null)
        {
            EditorGUI.BeginChangeCheck();
            Object selected = EditorGUI.ObjectField(position, sceneAsset.objectReferenceValue, typeof(SceneAsset), false);
            if (EditorGUI.EndChangeCheck())
            {
                sceneAsset.objectReferenceValue = selected;
                sceneName.stringValue = selected != null ? selected.name : "";
            }
        }

        EditorGUI.EndProperty();
    }
}
#endif