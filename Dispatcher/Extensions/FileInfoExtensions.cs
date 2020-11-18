using System.IO;

namespace Dispatcher.Extensions
{
    public static class FileInfoExtensions
    {
        public static bool IsLocked(this FileInfo file) 
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return false;
                }
            }
            catch (IOException)
            {
                return true;
            }
        }

        public static void WaitUntilLocked(this FileInfo fileInfo)
        {
            while (fileInfo.IsLocked());
        }
    }
}
