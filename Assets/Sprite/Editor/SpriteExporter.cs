using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class SpriteExporter : Editor
{
    [MenuItem("Assets/Export Sliced Sprites")]// 상단 Assets에 Export Sliced Sliced Sprites 메뉴 생성, 유니티 내부에서 sliced된 sprite를 png 파일로 추출할 수 있게 해주는 코드
    private static void ExportSprites()
    {
        foreach (var obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            Texture2D texture = obj as Texture2D;
            if (texture == null) continue;

            string dir = Path.GetDirectoryName(path) + "/ExportedSprites";
            Directory.CreateDirectory(dir);

            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            foreach (var sprite in sprites)
            {
                Texture2D newTex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
                Color[] pixels = texture.GetPixels((int)sprite.rect.x, (int)sprite.rect.y, (int)sprite.rect.width, (int)sprite.rect.height);
                newTex.SetPixels(pixels);
                newTex.Apply();

                File.WriteAllBytes(dir + "/" + sprite.name + ".png", newTex.EncodeToPNG());
            }
        }
        AssetDatabase.Refresh();
    }
}