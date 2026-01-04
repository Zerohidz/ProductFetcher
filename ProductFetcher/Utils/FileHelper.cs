namespace ProductFetcher.Utils;

/// <summary>
/// File and directory management utilities
/// Equivalent to Python's os_utils.py
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// Creates a directory and clears it if it already exists
    /// </summary>
    public static void SetupClearDirectory(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, recursive: true);
        }

        Directory.CreateDirectory(directoryPath);
    }

    /// <summary>
    /// Ensures a directory exists, creating it if necessary
    /// </summary>
    public static void EnsureDirectoryExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }
}
