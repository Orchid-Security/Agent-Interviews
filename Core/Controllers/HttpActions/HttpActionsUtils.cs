namespace Core.Controllers.HttpActions;

public static class HttpActionsUtils
{
    public static IEnumerable<string> ListFiles(string path, int maxDepth)
    {
        return ListFilesInternal(path, maxDepth, 0);
    }

    private static IEnumerable<string> ListFilesInternal(string path, int maxDepth, int currentDepth)
    {
        if (currentDepth > maxDepth)
        {
            yield break;
        }

        foreach (var file in Directory.GetFiles(path))
        {
            yield return file;
        }

        foreach (var directory in Directory.GetDirectories(path))
        {
            foreach (var file in ListFilesInternal(directory, maxDepth, currentDepth + 1))
            {
                yield return file;
            }
        }
    }
}