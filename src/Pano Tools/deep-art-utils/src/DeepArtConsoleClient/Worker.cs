using DeepArtConsoleClient.Convert;
using DeepArtConsoleClient.DeepArtAPI;
using DeepArtConsoleClient.Errors;
using DeepArtConsoleClient.Files;
using DeepArtConsoleClient.Settings;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace DeepArtConsoleClient
{
    internal class Worker
    {
        class SyncConsoleWriter
        {
            private static readonly object _sync = new object();

            public int WriteLine(string text)
            {
                lock (_sync)
                {
                    Console.WriteLine(text);
                    return Console.CursorTop - 1;
                }
            }

            public void RewriteLine(int line, string text)
            {
                lock (_sync)
                {
                    var prevLine = Console.CursorTop;
                    Console.SetCursorPosition(0, line);

                    if (text.Length > Console.WindowWidth) text = text.Substring(0, Console.WindowWidth);
                    if (text.Length < Console.WindowWidth) text = text + new string(' ', Console.WindowWidth - text.Length);

                    Console.Write(text);
                    Console.SetCursorPosition(0, prevLine);
                }
            }
        }


        private readonly AppSettings _settings;

        public Worker(AppSettings settings)
        {
            _settings = settings;
        }


        public async Task DoWork(CancellationToken cancellation)
        {
            var files = GetInputFiles();

            using var deepArt = new DeepArtService(_settings);

            var availableEffects = await GetAvailableEffects(deepArt, cancellation);

            var effect = GetPresetEffect(availableEffects) ?? SelectEffect(availableEffects);

            if (ConfirmFilesOverwrite(files))
            {
                await ConvertFiles(files, effect, deepArt, cancellation);
            }
        }


        // throws DeepArtConvertException
        protected FileDescriptor[] GetInputFiles()
        {
            var service = new FileService(_settings);

            FileDescriptor[]? files;
            try
            {
                files = service.GetInputFiles()?.ToArray();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new DeepArtConvertException(
                    AppErrorCode.InputFilesNotFound,
                    $"Failed to scan input files",
                    ex
                );
            }

            if (files == null || !files.Any())
            {
                throw new DeepArtConvertException(
                    AppErrorCode.InputFilesNotFound,
                    "No files found to convert"
                );
            }

            if (!_settings.SilentMode)
            {
                PrintFilesToBeConverted(files);
                Console.WriteLine();
            }

            return files!;
        }

        // throws DeepArtConvertException
        protected async Task<StyleInfo[]> GetAvailableEffects(DeepArtService deepArt, CancellationToken cancellation)
        {
            if (!_settings.SilentMode)
            {
                Console.WriteLine($"Getting available effects...");
                Console.WriteLine();
            }

            StyleInfo[]? effects;
            try
            {
                effects = (await deepArt.GetEffects(cancellation))?.ToArray();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new DeepArtConvertException(
                    AppErrorCode.GetEffectsFailed,
                    $"Failed to get available effects",
                    ex
                );
            }

            if (effects != null && _settings.MediaType != null)
            {
                effects = effects
                    .Where(x => x.MediaType == _settings.MediaType.Value)
                    .ToArray();
            }

            if (effects == null || !effects.Any())
            {
                throw new DeepArtConvertException(
                    AppErrorCode.EffectListIsEmpty,
                    "No Deep Art effects are available at the moment. Please try later."
                );
            }

            return effects!;
        }

        // throws DeepArtConvertException
        protected StyleInfo? GetPresetEffect(StyleInfo[] availableEffects)
        {
            if (string.IsNullOrEmpty(_settings.Effect))
            {
                if (_settings.SilentMode)
                {
                    throw new DeepArtConvertException(
                        AppErrorCode.EffectNotFound,
                        $"No effect is specified in silent mode"
                    );
                }

                return null;
            }

            var foundEffects = availableEffects.Where(
                     x => string.Equals(_settings.Effect, x.Name, StringComparison.InvariantCultureIgnoreCase)
                      || string.Equals(_settings.Effect, x.Id?.ToString(), StringComparison.InvariantCultureIgnoreCase)
                 ).ToArray();

            if (foundEffects.Length == 0)
            {
                throw new DeepArtConvertException(
                    AppErrorCode.EffectNotFound,
                    $"Effect with name or id '{_settings.Effect}' is not available"
                );
            }

            if (foundEffects.Length > 1)
            {
                throw new DeepArtConvertException(
                    AppErrorCode.EffectNotFound,
                    $"Effect with name or id '{_settings.Effect}' is not unique"
                );
            }

            return foundEffects[0];
        }

        protected StyleInfo SelectEffect(StyleInfo[] availableEffects)
        {
            if (_settings.SilentMode)
            {
                // should never get here
                throw new DeepArtConvertException(
                    AppErrorCode.EffectNotFound,
                    $"No effect is specified in silent mode"
                );
            }

            var printedEffects = PrintEffectsToChooseFrom(availableEffects);

            Console.Write($"Please select an effect (1 - {printedEffects.Length}): ");
            while (true)
            {
                var input = Console.ReadLine();

                if (!int.TryParse(input, out var number) || number <= 0 || printedEffects.Length < number)
                {
                    Console.Write($"Invalid number. Please select an effect (1 - {printedEffects.Length}): ");
                    continue;
                }

                var selectedEffect = printedEffects[number - 1];

                Console.WriteLine($"Selected effect: {selectedEffect.Name}");
                Console.WriteLine();

                return selectedEffect;
            }
        }

        protected bool ConfirmFilesOverwrite(FileDescriptor[] files)
        {
            if (_settings.SilentMode || !_settings.ConfirmOverwrite)
            {
                return true;
            }

            var filesToOverwrite = files.Where(f => f.OverwritesTarget()).ToArray();

            if (!filesToOverwrite.Any())
            {
                return true;
            }

            PrintFilesToBeOverwritten(files);

            Console.Write($"Continue? Y/N ");
            while (true)
            {
                var input = Console.ReadLine();

                var isYes = string.Equals(input, "y", StringComparison.InvariantCultureIgnoreCase)
                     || string.Equals(input, "yes", StringComparison.InvariantCultureIgnoreCase);

                var isNo = string.Equals(input, "n", StringComparison.InvariantCultureIgnoreCase)
                     || string.Equals(input, "no", StringComparison.InvariantCultureIgnoreCase);

                if (!isYes && !isNo)
                {
                    Console.Write($"Continue? Y/N ");
                    continue;
                }

                return isYes;
            }
        }

        // throws DeepArtConvertException
        protected async Task ConvertFiles(
            FileDescriptor[] files,
            StyleInfo effect,
            DeepArtService deepArt,
            CancellationToken cancellation
        )
        {
            if (!_settings.SilentMode)
            {
                Console.WriteLine("Converting may take a while, please wait...");
                Console.WriteLine();
            }

            Progress<ConversionReport>? progress = null;
            CompositeDisposable? cleanUp = null;

            if (!_settings.SilentMode)
            {
                progress = new Progress<ConversionReport>();
                cleanUp = new CompositeDisposable(files.Length);

                var reportWriter = new SyncConsoleWriter();

                var reports = Observable
                    .FromEventPattern<object, ConversionReport>(progress!, nameof(progress.ProgressChanged))
                    .Select(ev => ev.EventArgs);

                var pulse = Observable.Interval(TimeSpan.FromSeconds(1));

                foreach(var file in files)
                {
                    var fileName = file.FileName;
                    var fileLine = reportWriter.WriteLine($"{fileName} - pending");

                    IDisposable? filePulse = null;
                    filePulse = reports.Where(x => x.File == file)
                        .CombineLatest(pulse)
                        .Subscribe(((ConversionReport Report, long Time) e) =>
                        {
                            if (e.Report.IsCompleted)
                            {
                                var outcome = e.Report.IsCanceled ? "CANCELLED"
                                    : e.Report.Error == null ? "Converted"
                                    : $"FAILED {(e.Report.Error.GetBaseException() ?? e.Report.Error)?.Message}";

                                reportWriter.RewriteLine(fileLine, $"{fileName} - {outcome}");

                                filePulse!.Dispose();
                            }
                            else
                            {
                                var state = e.Report.State;
                                var dots = e.Time % 2 == 0 ? "..." : "..";

                                reportWriter!.RewriteLine(fileLine, $"{fileName} - {state}{dots}");
                            }
                        });

                    cleanUp.Add(filePulse);
                }

                Console.WriteLine();
            }

            var converter = new FileConverter(deepArt, _settings);

            try
            {
                await converter.ConvertFiles(files, effect, progress, cancellation);
            }
            catch (TimeoutException ex)
            {
                throw new DeepArtConvertException(
                    AppErrorCode.ConvertationTimeout,
                    $"Conversion timeout exceeded",
                    ex
                );
            }
            catch (Exception ex) when ((ex is not TaskCanceledException) && (ex is not OperationCanceledException))
            {
                throw new DeepArtConvertException(
                    AppErrorCode.ConvertingError,
                    "Failed to convert one or more files",
                    ex
                );
            }
            finally
            {
                cleanUp?.Dispose();
            }

            if (!_settings.SilentMode)
            {
                Console.WriteLine("Conversion completed");
            }
        }


        private void PrintFilesToBeConverted(FileDescriptor[] files)
        {
            var maxCount = 10;

            Console.WriteLine($"These files will be converted: ");

            foreach (var file in files.Take(maxCount))
            {
                Console.WriteLine($"  {file}");
            }
            if (files.Length > maxCount)
            {
                Console.WriteLine($"  and {files.Length - maxCount} other(s)");
            }
        }

        private void PrintFilesToBeOverwritten(FileDescriptor[] files)
        {
            var maxCount = 10;

            Console.WriteLine($"These files will be OVERWRITTEN:");

            foreach (var file in files.Take(maxCount))
            {
                Console.WriteLine($"  {file}");
            }
            if (files.Length > maxCount)
            {
                Console.WriteLine($"  and {files.Length - maxCount} other(s)");
            }
        }

        private StyleInfo[] PrintEffectsToChooseFrom(StyleInfo[] effects)
        {
            var effectsOrdered = new StyleInfo[effects.Length];
            var index = 0;

            var effectsByType = effects.GroupBy(x => x.MediaType?.ToString()).OrderBy(g => g.Key);
            foreach (var g in effectsByType)
            {
                var mediaType = g.Key;
                Console.WriteLine(
                    string.IsNullOrEmpty(mediaType)
                        ? "Other available effects:"
                        : $"Available {mediaType} effects:"
                );

                foreach (var effect in g)
                {
                    effectsOrdered[index] = effect;
                    Console.WriteLine($"  {index + 1}. {effect.Name}");

                    index++;
                }
                Console.WriteLine();
            }

            return effectsOrdered;
        }
    }
}
