using DeepArtConsoleClient.DeepArtAPI;

namespace DeepArtConsoleClient.Settings
{
    internal class AppSettingsTemplate
    {
        public string[]? Input { get; set; }

        [Obsolete]
        public string? InputDirectory { get; set; }

        public bool? Recursive { get; set; }

        public string[]? FileMask { get; set; }

        [Obsolete]
        public string[]? Files { get; set; }

        public string? Effect { get; set; }

        public StyleInfoMediaType? MediaType { get; set; }

        public string? OutputDirectory { get; set; }

        public bool? ConfirmOverwrite { get; set; }

        public SaveReports? SaveReports { get; set; }

        public bool? SilentMode { get; set; }

        public int? ParallelismDegree { get; set; }

        [Obsolete]
        public int? MaxConcurrentTaskCount { get; set; }

        public TimeSpan? Timeout { get; set; }

        public Uri? ApiUrl { get; set; }

        public TimeSpan? PollInterval { get; set; }

        public TimeSpan? HttpTimeout { get; set; }

        [Obsolete]
        public TimeSpan? RequestHttpTimeout { get; set; }
    }
}
