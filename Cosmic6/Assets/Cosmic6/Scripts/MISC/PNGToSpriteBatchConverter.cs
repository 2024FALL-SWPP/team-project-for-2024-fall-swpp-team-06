using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PNGToSpriteBatchConverter : MonoBehaviour
{
    /// <summary>
    /// Path to the folder containing PNG files.
    /// This should be relative to the Assets folder.
    /// Example: "Assets/Icons"
    /// </summary>
    public string folderPath = "Assets/Cosmic6/Prefabs/Plants/Icons";

    /// <summary>
    /// Target path where the created sprites will be saved.
    /// This should be relative to the Assets folder.
    /// </summary>
    public string targetPath = "Assets/Cosmic6/Prefabs/Plants/Icons/Sprites";

    /// <summary>
    /// Converts all PNG files in the folder to Sprites and saves them.
    /// </summary>
    public void ConvertAllPNGsToSprites()
    {
        // Get the absolute path of the folder
        string absoluteFolderPath = Path.Combine(Application.dataPath, folderPath.Replace("Assets/", ""));

        // Check if the folder exists
        if (!Directory.Exists(absoluteFolderPath))
        {
            Debug.LogError("Folder not found: " + absoluteFolderPath);
            return;
        }

        // Ensure the target folder exists
        string absoluteTargetPath = Path.Combine(Application.dataPath, targetPath.Replace("Assets/", ""));
        if (!Directory.Exists(absoluteTargetPath))
        {
            Directory.CreateDirectory(absoluteTargetPath);
            Debug.Log("Created target folder: " + absoluteTargetPath);
        }

        // Get all PNG files in the folder
        string[] pngFiles = Directory.GetFiles(absoluteFolderPath, "*.png");
        if (pngFiles.Length == 0)
        {
            Debug.LogWarning("No PNG files found in folder: " + absoluteFolderPath);
            return;
        }

        foreach (string pngFile in pngFiles)
        {
            CreateAndSaveSpriteFromPNG(pngFile);
        }

        Debug.Log("All PNG files have been processed.");
    }

    /// <summary>
    /// Creates a Sprite from a PNG file and saves it in the target path.
    /// </summary>
    /// <param name="pngFilePath">Absolute path to the PNG file.</param>
    private void CreateAndSaveSpriteFromPNG(string pngFilePath)
    {
        // Load PNG file into a Texture2D
        byte[] pngData = File.ReadAllBytes(pngFilePath);
        Texture2D texture = new Texture2D(2, 2); // Initial size will be replaced
        if (!texture.LoadImage(pngData))
        {
            Debug.LogError("Failed to load PNG: " + pngFilePath);
            return;
        }

        // Get the file name without extension
        string fileName = Path.GetFileNameWithoutExtension(pngFilePath);

        // Create a Sprite
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f) // Pivot at center
        );

#if UNITY_EDITOR
        // Save the sprite as an asset
        string relativePath = Path.Combine(targetPath, fileName + ".asset");
        AssetDatabase.CreateAsset(sprite, relativePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Created and saved sprite: " + relativePath);
#else
        Debug.LogError("Sprite saving is only supported in the Unity Editor.");
#endif
    }
}
