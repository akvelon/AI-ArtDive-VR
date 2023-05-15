using DeepArtConsoleClient.Settings;

namespace DeepArtConsoleClient.Files
{
    class FileMarker
    {
        public const string EXT_IN_PROGRESS = ".converting";
        public const string EXT_SUCCESS = ".converted";
        public const string EXT_FAILURE = ".failed";

        private const string SUCCESS = "SUCCESS";
        private const string FAILURE = "FAILURE: ";

        private readonly AppSettings _settings;

        public FileMarker(FileDescriptor file, AppSettings settings)
        {
            FileDescriptor = file;
            _settings = settings;
        }

        public FileDescriptor FileDescriptor { get; }

        public Guid? SourceMediaId { get; set; }
        public Guid? EffectId { get; set; }
        public Guid? OperationId { get; set; }
        public string? Outcome { get; private set; }

        public bool IsCompleted => !string.IsNullOrEmpty(Outcome);
        public bool IsSuccess => Outcome == SUCCESS;
        public bool IsFailure => Outcome != null && Outcome != SUCCESS;


        public void SetInProgress()
        {
            Outcome = null;
        }

        public void SetSuccess()
        {
            Outcome = SUCCESS;
        }

        public void SetFailure(string failureDesc)
        {
            Outcome = FAILURE + failureDesc;
        }


        public void Restore()
        {
            if (_settings.SaveReports == SaveReports.None)
            {
                return;
            }

            var existingPath = GetPossibleFilePaths().Where(File.Exists).FirstOrDefault();
            if (existingPath == null) return;

            var lines = File.ReadAllLines(existingPath);
            Deserialize(lines);
        }

        public void Persist()
        {
            if (_settings.SaveReports == SaveReports.None)
            {
                return;
            }

            foreach (var path in GetPossibleFilePaths().Where(File.Exists))
            {
                try
                {
                    File.Delete(path);
                }
                catch
                {
                    // ignore
                }
            }

            if (_settings.SaveReports == SaveReports.FailuresOnly && !IsFailure)
            {
                return;
            }

            var currentPath = GetCurrentFilePath();

            var outputDirectory = new FileInfo(currentPath).Directory;
            if (outputDirectory == null)
            {
                return;
            }
            outputDirectory.Create();

            var lines = Serialize();
            File.WriteAllLines(currentPath, lines);
        }


        protected IEnumerable<string> GetPossibleFilePaths()
        {
            yield return FileDescriptor.TargetFilePath + EXT_IN_PROGRESS;
            yield return FileDescriptor.TargetFilePath + EXT_SUCCESS;
            yield return FileDescriptor.TargetFilePath + EXT_FAILURE;
        }

        protected string GetCurrentFilePath()
        {
            var ext = IsCompleted ? IsSuccess ? EXT_SUCCESS : EXT_FAILURE : EXT_IN_PROGRESS;
            return FileDescriptor.TargetFilePath + ext;
        }

        protected void Deserialize(string[] lines)
        {
            SourceMediaId = lines?.Length > 0 && Guid.TryParse(lines[0], out var sourceMediaId) ? sourceMediaId : null;
            EffectId = lines?.Length > 1 && Guid.TryParse(lines[1], out var effectId) ? effectId : null;
            OperationId = lines?.Length > 2 && Guid.TryParse(lines[2], out var operationId) ? operationId : null;
            Outcome = lines?.Length > 3 ? string.Join("\n", lines.Skip(3)) : null;
        }

        protected string[] Serialize()
        {
            return new string[] {
                SourceMediaId?.ToString() ?? "",
                EffectId?.ToString() ?? "",
                OperationId?.ToString() ?? "",
                Outcome ?? ""
            };
        }
    }
}
