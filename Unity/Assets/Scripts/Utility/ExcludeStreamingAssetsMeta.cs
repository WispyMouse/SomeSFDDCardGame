using UnityEditor;

// Solution from Noxalus at https://discussions.unity.com/t/why-is-there-meta-files-in-the-streamingassets-directory/738268/16

public class ExcludeStreamingAssetsMeta : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (var importedAsset in importedAssets)
        {
            if (importedAsset.Contains("Assets/StreamingAssets"))
            {
                // Remove generated meta file
                string metaFilePath = $"{importedAsset}.meta";
                AssetDatabase.MoveAssetToTrash(metaFilePath);
            }
        }
    }
}