namespace DeepArtConsoleClient.Files
{
    class FileDescriptor
    {
        public FileDescriptor(string sourceFilePath, string? outputDirectory = null)
        {
            sourceFilePath = Path.GetFullPath(sourceFilePath);

            FileName = Path.GetFileName(sourceFilePath);
            SourceFilePath = sourceFilePath;

            if (string.IsNullOrEmpty(outputDirectory))
            {
                TargetFilePath = sourceFilePath;
            }
            else
            {
                TargetFilePath = Path.GetFullPath(Path.Combine(outputDirectory, FileName));
            }

            TargetRelativeFilePath = FileName;

        }

        public FileDescriptor(string sourceFilePath, string sourceDirectory, string? outputDirectory = null)
        {
            sourceFilePath = Path.GetFullPath(sourceFilePath);
            sourceDirectory = Path.GetFullPath(sourceDirectory);

            if (!sourceFilePath.StartsWith(sourceDirectory))
            {
                throw new ArgumentException($"{nameof(sourceFilePath)} and {nameof(sourceDirectory)} should match", nameof(sourceDirectory));
            }

            FileName = Path.GetFileName(sourceFilePath);
            SourceFilePath = sourceFilePath;

            // preserve source directory structure:
            //  if sourceDirectory is   /source/dir
            //  and sourceFilePath is   /source/dir/subdir/file.png
            //  and outputDirectory is  /output/dir
            //  then TargetRelativeFilePath should be:  subdir/file.png
            //  and TargetFilePath should be:           /output/dir/subdir/file.png
            outputDirectory ??= sourceDirectory;
            sourceDirectory = sourceDirectory.TrimEnd(Path.DirectorySeparatorChar, '/', '\\');
            outputDirectory = outputDirectory.TrimEnd(Path.DirectorySeparatorChar, '/', '\\');
            var targetRelativePath = sourceFilePath.Substring(sourceDirectory.Length);

            TargetFilePath = Path.GetFullPath(outputDirectory + targetRelativePath);
            TargetRelativeFilePath = targetRelativePath.TrimStart(Path.DirectorySeparatorChar, '/', '\\');
        }

        public string FileName { get; }
        public string SourceFilePath { get; }
        public string TargetFilePath { get; }
        public string TargetRelativeFilePath { get; }

        public bool OverwritesTarget()
        {
            return SourceFilePath == TargetFilePath;
        }

        public override string ToString()
        {
            return TargetRelativeFilePath;
        }
    }
}
