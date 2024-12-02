using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ThumbnailCreator : MonoBehaviour
{
    [SerializeField]
    public List<Transform> Entities = new List<Transform>();

    public const int MaxEntities = 30;

    public string TargetPath = "Icons";

#if UNITY_EDITOR
    public void generateEntityIcons()
    {
        Debug.Log("Generating icons and sprites for entities...");

        if (Entities.Count > MaxEntities)
        {
            Debug.LogError($"The list contains more than {MaxEntities} entities. Please reduce the list size.");
            return;
        }

        string absolutePath = Path.Combine(Application.dataPath, TargetPath.Replace("Assets/", ""));
        string assetPath = "Assets/" + TargetPath.TrimStart('/');

        if (!Directory.Exists(absolutePath))
        {
            Directory.CreateDirectory(absolutePath);
            Debug.Log("Created directory: " + absolutePath);
        }

        foreach (Transform e in Entities)
        {
            Debug.Log($"Generating icon for {e.name}...");

            // Get preview synchronously
            Texture2D icon = GetPreviewSynchronously(e.gameObject);

            if (icon == null)
            {
                Debug.LogWarning($"Could not load icon for {e.name}. Skipping...");
                continue;
            }

            Texture2D iconCopy = CopyTexture(icon);
            iconCopy.Apply();

            //// Save as PNG
            //string pngPath = Path.Combine(absolutePath, e.name + ".png");
            //File.WriteAllBytes(pngPath, iconCopy.EncodeToPNG());
            //Debug.Log($"Generated PNG icon for {e.name} at {pngPath}");


            // Save Texture2D as asset
            string texturePath = assetPath + "/" + e.name + "_texture.asset";
            Texture2D existingTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

            if (existingTexture == null)
            {
                AssetDatabase.CreateAsset(iconCopy, texturePath);
                Debug.Log($"Generated Texture asset for {e.name} at {texturePath}");
            }
            else
            {
                Debug.LogWarning($"Texture asset already exists at {texturePath}. Overwriting...");
                existingTexture = iconCopy;
                EditorUtility.SetDirty(existingTexture);
            }

            // Save as Sprite asset
            Texture2D loadedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            Sprite sprite = Sprite.Create(
                loadedTexture,
                new Rect(0, 0, loadedTexture.width, loadedTexture.height),
                new Vector2(0.5f, 0.5f)
            );
            string spritePath = assetPath + "/" + e.name + ".asset";

            Sprite existingSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (existingSprite == null)
            {
                AssetDatabase.CreateAsset(sprite, spritePath);
                Debug.Log($"Generated Sprite asset for {e.name} at {spritePath}");
            }
            else
            {
                Debug.LogWarning($"Sprite asset already exists at {spritePath}. Overwriting...");
                existingSprite = sprite;
                EditorUtility.SetDirty(existingSprite);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

        }

        Debug.Log($"Generation complete - Saved PNGs and Sprites to path: {absolutePath}");
    }

    private Texture2D CopyTexture(Texture2D source)
    {
        Texture2D copy = new Texture2D(source.width, source.height, source.format, false);
        copy.SetPixels(source.GetPixels());
        copy.Apply();
        return copy;
    }

    /// <summary>
    /// Waits for Unity to generate the preview synchronously.
    /// </summary>
    private Texture2D GetPreviewSynchronously(GameObject obj)
    {
        int retries = 10; // Number of retries
        int delayMs = 5000; // Delay between retries in milliseconds

        // Request the preview
        Texture2D preview = UnityEditor.AssetPreview.GetAssetPreview(obj);

        // Wait until the preview is generated
        while (preview == null && retries > 0)
        {
            retries--;
            System.Threading.Thread.Sleep(delayMs);
            preview = UnityEditor.AssetPreview.GetAssetPreview(obj);
        }

        // Ensure the preview is fully loaded
        if (preview == null)
        {
            Debug.LogWarning($"Preview for {obj.name} could not be generated.");
        }

        return preview;
    }

    private void OnValidate()
    {
        if (Entities.Count > MaxEntities)
        {
            Debug.LogWarning($"Entities list exceeds the maximum limit of {MaxEntities}. Trimming the list.");
            Entities = Entities.GetRange(0, MaxEntities);
        }
    }
#endif
}
