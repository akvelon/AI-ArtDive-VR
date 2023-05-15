using DeepArtConsoleClient.Settings;

namespace DeepArtConsoleClient.Files
{
    class FileService
    {
        private readonly AppSettings _settings;

        public FileService(AppSettings settings)
        {
            _settings = settings;
        }


        public IEnumerable<FileDescriptor> GetInputFiles()
        {
            if (_settings.Input == null) return Array.Empty<FileDescriptor>();

            var result = new List<FileDescriptor>();

            foreach (var path in _settings.Input)
            {
                try
                {
                    if (IsDirectory(path))
                    {
                        AddInputFiles(path, path, ref result);
                    }
                    else
                    {
                        result.Add(new FileDescriptor(path, _settings.OutputDirectory));
                    }
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Failed to read input path: {path}", ex);
                }
            }

            return result;
        }

        private void AddInputFiles(string directory, string sourceDirectory, ref List<FileDescriptor> target)
        {
            string[] files;

            if (_settings.FileMask == null || !_settings.FileMask.Any())
            {
                files = Directory.GetFiles(directory);
            }
            else
            {
                files = _settings.FileMask
                    .SelectMany(mask => Directory.GetFiles(directory, mask))
                    .Distinct()
                    .ToArray();
            }

            foreach (var file in files)
            {
                target.Add(new FileDescriptor(file, sourceDirectory, _settings.OutputDirectory));
            }

            if (_settings.Recursive)
            {
                var subDirectories = Directory.GetDirectories(directory);
                foreach (var subDirectory in subDirectories)
                {
                    AddInputFiles(subDirectory, sourceDirectory, ref target);
                }
            }
        }

        protected bool IsDirectory(string path)
        {
            var attr = File.GetAttributes(path);
            return (attr & FileAttributes.Directory) == FileAttributes.Directory;
        }
    }
}
