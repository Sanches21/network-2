using System.IO;
using UnityEngine;

public class SystemPath
{
    private string saveFolder = Application.streamingAssetsPath;
    private const string fileExtension = ".json";

    public string GetFullSavePath(string fileName)
    {
        string fullPath = Path.Combine(saveFolder, fileName + fileExtension);
        Debug.Log(fullPath);
        return fullPath;
    }
}
