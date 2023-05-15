using DeepArtConsoleClient.DeepArtAPI;
using DeepArtConsoleClient.Errors;
using Microsoft.Extensions.Configuration;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using System.ComponentModel.DataAnnotations;

namespace DeepArtConsoleClient.Settings
{
    internal class AppSettingsBinder : BinderBase<AppSettingsTemplate>
    {
        public readonly Argument<IEnumerable<FileSystemInfo>?> Input = new Argument<IEnumerable<FileSystemInfo>?>(
            name: "path",
            description:
                "File or directory to convert. " +
                "In silent mode, at least one path is required. " +
                "In interactive mode, default value is \".\" (current directory). " +
                "Might also set value via option -i or --input, or environment variables INPUT__0, INPUT_1 and so on."
        );

        public readonly Option<IEnumerable<FileSystemInfo>?> InputHiddenOption = new Option<IEnumerable<FileSystemInfo>?>(
            aliases: new[] { "--input", "-i" }
        )
        { IsHidden = true, AllowMultipleArgumentsPerToken = true };

        [Obsolete]
        public readonly Option<string?> InputDirectory = new Option<string?>(
            "--inputDirectory",
            description: "Deprecated. Please specify files and/or directories directly."
        )
        { ArgumentHelpName = "path" };

        public readonly Option<bool?> Recursive = new Option<bool?>(
            aliases: new[] { "-r", "--recursive" },
            description:
                "For directory inputs: convert files in subfirectories. " +
                "In silent mode, default value is \"false\". " +
                "In interactive mode, default value is \"true\". " +
                "Might also set value via environment variable RECURSIVE."
        );

        public readonly Option<IEnumerable<string>?> FileMask = new Option<IEnumerable<string>?>(
            aliases: new[] { "-m", "--file-mask", "--fileMask", "--files" },
            description:
                "For directory inputs: file mask(s). " +
                "If no masks are specified, then all files in directory (including hidden!) will be converted. " +
                "Default value is *.png, *.jpg, *.jpeg - all the file types that are supported by Deep Art as of July 2022. " +
                "Might also set value via environment variables FILEMASK_0, FILEMASK_1 and so on."
        )
        { ArgumentHelpName = "mask", AllowMultipleArgumentsPerToken = true };

        public readonly Option<string?> Effect = new Option<string?>(
           aliases: new[] { "--effect", "-e" },
           description:
               "Name or ID of Deep Art effect to apply to files, case insensitive. " +
               "If not specified, user is asked to choose one of currently available effects. " +
               "In silent mode, value is required. " +
               "When using effect name, it is also recommended to set --media-type value, since effect name is not unique in Deep Art. " +
               "Might also set value via environment variable EFFECT."
        );

        public readonly Option<StyleInfoMediaType?> MediaType = new Option<StyleInfoMediaType?>(
           aliases: new[] { "--media-type" },
           description:
               "Type of effects to use. " +
               "Default value is \"IMAGE\". " +
               "Might also set value via environment variable MEDIATYPE."
        );

        public readonly Option<string?> OutputDirectory = new Option<string?>(
            aliases: new[] { "-o", "--outputDirectory" },
            description:
                "Directory where coverted files are saved to. " +
                "If --recursive is set to true, then input directory structure is preserved. " +
                "If not specified, input files are overwritten. " +
                "Might also set value via environment variable OUTPUTDIRECTORY."
        )
        { ArgumentHelpName = "path" };

        public readonly Option<bool?> ConfirmOverwrite = new Option<bool?>(
            aliases: new[] { "-y", "--confirm-overwrite" },
            description:
                "Ask user confirmation if file is about to be overwritten. " +
                "In silent mode, value is ignored, files are overwritten without confirmation. " +
                "Default value is \"true\". " +
                "Might also set value via environment variable CONFIRMOVERWRITE."
        );

        public readonly Option<SaveReports?> SaveReports = new Option<SaveReports?>(
           aliases: new[] { "--save-reports", "--saveReports" },
           description:
               "Usage of marker files (*.converting, *.converted, *.failed). " +
               "Value \"All\" allows to continue operations after restart. " +
               "Default value is \"None\". " +
               "Might also set value via environment variable SAVEREPORTS."
        );

        public readonly Option<bool?> SilentMode = new Option<bool?>(
            aliases: new[] { "-s", "--silent-mode", "--silentMode" },
            description:
                "Silent mode flag. " +
                "If set to \"true\", no user interaction is used. " +
                "Input paths and --effect are required in silent mode!. " +
                "Default value is \"false\". " +
                "Might also set value via environment variable SILENTMODE."
        );

        public readonly Option<int?> ParallelismDegree = new Option<int?>(
            aliases: new[] { "-p", "--parallelism-degree", "--maxConcurrentTaskCount" },
            description:
                "Maximum number of simultaneos convertations. " +
                "By default, no limitations are used. " +
                "Recommended value is 3-10. " +
                "Might also set value via environment variable PARALLELISMDEGREE."
        )
        { ArgumentHelpName = "integer" };

        public readonly Option<TimeSpan?> Timeout = new Option<TimeSpan?>(
            aliases: new[] { "-t", "--timeout" },
            description:
                "Overall execution timeout, " +
                "i.e. maximum amount of time allowed to convert all input files. " +
                "By default, no limitations are used. " +
                "Might also set value via environment variable TIMEOUT."
        )
        { ArgumentHelpName = "hh:mm:ss" };

        public readonly Option<Uri?> ApiUrl = new Option<Uri?>(
            aliases: new[] { "-api-url", "--apiUrl" },
            description:
                "Deep Art API url. " +
                "Default value is https://deep-art.k8s.akvelon.net/api " +
                "Might also set value via environment variable APIURL."
        )
        { ArgumentHelpName = "url" };

        public readonly Option<TimeSpan?> PollInterval = new Option<TimeSpan?>(
            aliases: new[] { "--poll-interval" },
            description:
                "Period of time beetween API requests when waiting for operation completion. " +
                "Default value is 2s. " +
                "Might also set value via environment variable POLLINTERVAL."
        )
        { ArgumentHelpName = "hh:mm:ss" };

        public readonly Option<TimeSpan?> HttpTimeout = new Option<TimeSpan?>(
            aliases: new[] { "-http-timeout", "--requestHttpTimeout" },
            description:
                "HTTP request timeout. " +
                "Default value is 100s. " +
                "Might also set value via environment variable HTTPTIMEOUT."
        )
        { ArgumentHelpName = "hh:mm:ss" };


        public AppSettings GetValue(BindingContext bindingContext)
        {
            AppSettingsTemplate config;
            try
            {
                config = ReadConfig();
            }
            catch (Exception ex)
            {
                throw new DeepArtConvertException(
                    AppErrorCode.InvalidSettings,
                    "Failed to read configuration",
                    ex
                );
            }

            AppSettingsTemplate? args;
            try
            {
                args = GetBoundValue(bindingContext);
            }
            catch (Exception ex)
            {
                throw new DeepArtConvertException(
                    AppErrorCode.InvalidSettings,
                    "Failed to read command line args",
                    ex
                );
            }

            var result = BuidlAppSettings(config, args);
            try
            {
                result.Validate();
            }
            catch (ValidationException ex)
            {
                throw new DeepArtConvertException(
                    AppErrorCode.InvalidSettings,
                    "Invalid configuration",
                    ex
                );
            }

            return result;
        }


        protected override AppSettingsTemplate GetBoundValue(BindingContext bindingContext)
        {
            var input = bindingContext.ParseResult.GetValueForArgument(Input)?
                .Select(f => f.FullName)
                .ToArray();

            if (input == null || !input.Any())
            {
                input = bindingContext.ParseResult.GetValueForOption(InputHiddenOption)?
                    .Select(f => f.FullName)
                    .ToArray();
            }

            // if TimeSpan value is set as an integer, then interpret it as seconds (15 -> 00:00:15)
            // (by default number is interpreted as hours: 15 -> 15:00:00)
            TimeSpan? GetTimeSpanValue(Option<TimeSpan?> option)
            {
                var timeSpan = bindingContext.ParseResult.GetValueForOption(option);
                if (timeSpan != null)
                {
                    var rawValue = bindingContext.ParseResult.CommandResult.Children
                        .OfType<OptionResult>()
                        .FirstOrDefault(x => x.Option == option)?
                        .Tokens
                        .FirstOrDefault()?
                        .Value;

                    if (int.TryParse(rawValue, out var number))
                    {
                        return TimeSpan.FromSeconds(number);
                    }
                }
                return timeSpan;
            }

            return new AppSettingsTemplate
            {
                Input = input,
                InputDirectory = bindingContext.ParseResult.GetValueForOption(InputDirectory),
                Recursive = bindingContext.ParseResult.GetValueForOption(Recursive),
                FileMask = bindingContext.ParseResult.GetValueForOption(FileMask)?.ToArray(),
                Effect = bindingContext.ParseResult.GetValueForOption(Effect),
                MediaType = bindingContext.ParseResult.GetValueForOption(MediaType),
                OutputDirectory = bindingContext.ParseResult.GetValueForOption(OutputDirectory),
                ConfirmOverwrite = bindingContext.ParseResult.GetValueForOption(ConfirmOverwrite),
                SaveReports = bindingContext.ParseResult.GetValueForOption(SaveReports),
                SilentMode = bindingContext.ParseResult.GetValueForOption(SilentMode),
                ParallelismDegree = bindingContext.ParseResult.GetValueForOption(ParallelismDegree),
                Timeout = GetTimeSpanValue(Timeout),
                ApiUrl = bindingContext.ParseResult.GetValueForOption(ApiUrl),
                PollInterval = GetTimeSpanValue(PollInterval),
                HttpTimeout = GetTimeSpanValue(HttpTimeout),
            };
        }

        protected AppSettingsTemplate ReadConfig()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                //.AddJsonFile("appsettings.debug.json", optional: true)
                .AddEnvironmentVariables()
                .Build()
                .Get<AppSettingsTemplate>();
        }

        protected AppSettings BuidlAppSettings(AppSettingsTemplate config, AppSettingsTemplate args)
        {
            var settings = new AppSettings();

            var isSilentMode = args?.SilentMode ?? config?.SilentMode ?? false;

            var input = new List<string>();
            if (args?.Input != null) input.AddRange(args.Input);
            if (args?.InputDirectory != null) input.Add(args.InputDirectory);
            if (!input.Any())
            {
                if (config?.Input != null) input.AddRange(config.Input);
                if (config?.InputDirectory != null) input.Add(config.InputDirectory);
            }
            if (!input.Any())
            {
                if (!isSilentMode) input.Add(".");
            }
            settings.Input = input.ToArray();

            settings.SilentMode = isSilentMode;

            settings.Recursive = args?.Recursive ?? config?.Recursive ?? !isSilentMode;

            var fileMask = new List<string>();
            if (args?.FileMask != null) fileMask.AddRange(args.FileMask);
            if (!fileMask.Any())
            {
                if (config?.FileMask != null) fileMask.AddRange(config.FileMask);
                if (config?.Files != null) fileMask.AddRange(config.Files);
            }
            if (!fileMask.Any())
            {
                fileMask.Add("*.png");
                fileMask.Add("*.jpg");
                fileMask.Add("*.jpeg");
            }
            settings.FileMask = fileMask.ToArray();

            settings.Effect = args?.Effect ?? config?.Effect;

            settings.MediaType = args?.MediaType ?? config?.MediaType ?? StyleInfoMediaType.IMAGE;

            settings.OutputDirectory = args?.OutputDirectory ?? config?.OutputDirectory;

            settings.ConfirmOverwrite = args?.ConfirmOverwrite ?? config?.ConfirmOverwrite ?? !isSilentMode;

            settings.SaveReports = args?.SaveReports ?? config?.SaveReports ?? Settings.SaveReports.None;

            settings.ParallelismDegree = args?.ParallelismDegree ?? config?.ParallelismDegree;

            settings.Timeout = args?.Timeout ?? config?.Timeout;

            settings.ApiUrl = args?.ApiUrl ?? config?.ApiUrl ?? new Uri("https://deep-art.k8s.akvelon.net/api");

            settings.PollInterval = args?.PollInterval ?? config?.PollInterval ?? TimeSpan.FromSeconds(2);

            settings.HttpTimeout = args?.HttpTimeout ?? config?.HttpTimeout ?? config?.RequestHttpTimeout;

            return settings;
        }
    }
}
