using DeepArtConsoleClient.DeepArtAPI;
using System.ComponentModel.DataAnnotations;

namespace DeepArtConsoleClient.Settings
{
    internal class AppSettings
    {
        [Required(ErrorMessage = "Please specify at least one input file or directory")]
        [MinLength(1, ErrorMessage = "Please specify at least one input file or directory")]
        public string[] Input { get; set; } = new string[0];

        public bool Recursive { get; set; }

        public string[]? FileMask { get; set; }

        public string? Effect { get; set; }

        public StyleInfoMediaType? MediaType { get; set; }

        public string? OutputDirectory { get; set; }

        public bool ConfirmOverwrite { get; set; }

        public SaveReports SaveReports { get; set; }

        public bool SilentMode { get; set; }

        [Range(1, int.MaxValue)]
        public int? ParallelismDegree { get; set; }

        public TimeSpan? Timeout { get; set; }

        [Required(ErrorMessage = "Please specify Deep Art API url")]
        public Uri? ApiUrl { get; set; }

        public TimeSpan PollInterval { get; set; }

        public TimeSpan? HttpTimeout { get; set; }


        /// <exception cref="ValidationException"></exception>
        public void Validate()
        {
            Validator.ValidateObject(this, new ValidationContext(this), validateAllProperties: true);

            // in silent mode Effect must be specified
            if (SilentMode && string.IsNullOrEmpty(Effect))
            {
                throw new ValidationException("Please specify Effect name or ID");
            }
        }
    }

    internal enum SaveReports { None, FailuresOnly, All }
}
