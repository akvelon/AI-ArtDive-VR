using DeepArtConsoleClient.DeepArtAPI;
using DeepArtConsoleClient.Files;
using DeepArtConsoleClient.Settings;
using Polly;
using System.Net;

namespace DeepArtConsoleClient.Convert
{
    internal class FileConverter
    {
        private readonly DeepArtService _deepArt;
        private readonly AppSettings _settings;

        public FileConverter(DeepArtService deepArt, AppSettings settings)
        {
            _deepArt = deepArt;
            _settings = settings;
        }

        /// <exception cref="TimeoutException"></exception>
        public async Task ConvertFiles(
            FileDescriptor[] files,
            StyleInfo effect,
            IProgress<ConversionReport>? progress,
            CancellationToken cancellation
        )
        {
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = _settings.ParallelismDegree ?? files.Length,
                CancellationToken = cancellation
            };

            var batchConvert = Parallel.ForEachAsync(files, parallelOptions, async (FileDescriptor file, CancellationToken cancellation) =>
            {
                Progress<string>? fileProgress = null;
                if (progress != null)
                {
                    fileProgress = new Progress<string>();
                    fileProgress.ProgressChanged += (s, e) => progress.Report(new ConversionReport(file, e));
                }

                await ConvertFile(file, effect, fileProgress, cancellation)
                    .ContinueWith(task =>
                    {
                        progress?.Report(new ConversionReport(file, null, true, task.IsCanceled, task.Exception));

                        if (task.Exception != null) throw task.Exception;
                    });
            });

            if (_settings.Timeout != null)
            {
                batchConvert = batchConvert.WaitAsync(_settings.Timeout.Value);
            }

            await batchConvert;
        }

        public async Task ConvertFile(
            FileDescriptor file,
            StyleInfo effect,
            IProgress<string>? progress,
            CancellationToken cancellation
        )
        {
            var marker = new FileMarker(file, _settings);
            marker.Restore();

            var alphaMap = await ReadAlphaMap(file, marker, progress);

            if (marker.SourceMediaId == null)
            {
                await UploadMedia(file, marker, progress, cancellation).ConfigureAwait(false);
            }

            if (marker.OperationId == null
                || !effect.Id.Equals(marker.EffectId)
                || marker.IsCompleted && !marker.IsSuccess
            )
            {
                await StartOperation(effect, marker, progress, cancellation).ConfigureAwait(false);
            }

            if (!marker.IsCompleted)
            {
                await WaitForOperationResult(file, marker, progress, cancellation).ConfigureAwait(false);
            }

            if (marker.IsSuccess)
            {
                await ApplyAlphaMap(file, alphaMap, marker, progress, cancellation).ConfigureAwait(false);
            }
        }

        protected async Task<AlphaMap> ReadAlphaMap(
           FileDescriptor file,
           FileMarker marker,
           IProgress<string>? progress
       )
        {
            marker.SetInProgress();
            marker.Persist();

            progress?.Report($"Reading alpha map");

            AlphaMap alphaMap;
            try
            {
                alphaMap = await Task.Run(
                    () => AlphaMap.ReadFromFile(file.SourceFilePath)
                ).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                marker.SetFailure(ex.Message);
                throw new ApplicationException($"Failed to read alpha map from {file}", ex);
            }
            finally
            {
                marker.Persist();
            }

            progress?.Report($"Read alpha map");

            return alphaMap;
        }

        protected async Task UploadMedia(
            FileDescriptor file,
            FileMarker marker,
            IProgress<string>? progress,
            CancellationToken cancellation
        )
        {
            marker.SourceMediaId = null;
            marker.OperationId = null;
            marker.SetInProgress();
            marker.Persist();

            progress?.Report("Uploading media");

            try
            {
                var mimeType = MimeTypes.GetMimeType(file.FileName);

                using (var fileContents = File.OpenRead(file.SourceFilePath))
                {
                    var fileInfo = new FileParameter(fileContents, file.FileName, mimeType);
                    marker.SourceMediaId = await _deepArt.AddMedia(fileInfo, cancellation);
                }

                if (marker.SourceMediaId == null)
                {
                    throw new ApplicationException("Unable to receive source media Id");
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                marker.SetFailure(ex.Message);
                throw new ApplicationException("Failed to upload media", ex);
                throw;
            }
            finally
            {
                marker.Persist();
            }

            progress?.Report("Media uploaded");
        }

        protected async Task StartOperation(
            StyleInfo effect,
            FileMarker marker,
            IProgress<string>? progress,
            CancellationToken cancellation
        )
        {
            marker.EffectId = effect.Id;
            marker.OperationId = null;
            marker.SetInProgress();
            marker.Persist();

            progress?.Report("Requesting convert operation");

            try
            {
                if (marker.SourceMediaId == null)
                {
                    throw new InvalidOperationException("No source media Id information");
                }
                if (effect.Id == null)
                {
                    throw new InvalidOperationException("Effect has no Id specified");
                }

                var operation = new SubmitOperationParameters
                {
                    Data =
                        {
                            Media = { Id = marker.SourceMediaId.Value },
                            Effect = { Id = effect.Id.Value }
                        },
                };
                marker.OperationId = await _deepArt.StartOperation(operation, cancellation).ConfigureAwait(false);

                if (marker.SourceMediaId == null)
                {
                    throw new InvalidOperationException("Unable to receive operation Id");
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                marker.SetFailure(ex.Message);
                throw new ApplicationException("Failed to request convert operation", ex);
            }
            finally
            {
                marker.Persist();
            }

            progress?.Report("Convert operation requested");
        }

        protected async Task WaitForOperationResult(
            FileDescriptor file,
            FileMarker marker,
            IProgress<string>? progress,
            CancellationToken cancellation
        )
        {
            marker.SetInProgress();
            marker.Persist();

            progress?.Report("Checking convert operation");

            try
            {
                if (marker.OperationId == null)
                {
                    throw new InvalidOperationException("No operation id information");
                }

                var poll = BuildOperationResultPoll(progress);

                var convertedData = await poll.ExecuteAsync(
                    () => _deepArt.GetOperationResult(marker.OperationId.Value, cancellation)
                ).ConfigureAwait(false);

                if (convertedData == null || convertedData.Length == 0)
                {
                    throw new InvalidOperationException("Unable to receive result media");
                }

                marker.SetSuccess();
                SaveFileData(file, convertedData);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                marker.SetFailure(ex.Message);
                throw new ApplicationException("Failed to check convert operation", ex);
            }
            finally
            {
                marker.Persist();
            }

            progress?.Report("Convert operation completed");
        }

        private async Task ApplyAlphaMap(
            FileDescriptor file,
            AlphaMap alphaMap,
            FileMarker marker,
            IProgress<string>? progress,
            CancellationToken cancellation
        )
        {
            if (!alphaMap.HasOpacity())
            {
                progress?.Report("No aplha map no apply");
                return;
            }

            marker.SetInProgress();
            marker.Persist();

            progress?.Report("Applying aplha map");

            try
            {
                await alphaMap.ApplyToFile(file.TargetFilePath, cancellation).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                marker.SetFailure(ex.Message);
                throw new ApplicationException("Failed to apply alpha map", ex);
            }
            finally
            {
                marker.Persist();
            }

            progress?.Report($"Applied alpha map");
        }

        protected IAsyncPolicy<byte[]> BuildOperationResultPoll(IProgress<string>? progress = null)
        {
            var retryOn = new[] {
                HttpStatusCode.InternalServerError,
                HttpStatusCode.ServiceUnavailable,
                HttpStatusCode.RequestTimeout,
                HttpStatusCode.GatewayTimeout
            };

            var retryCount = 5;

            var pollInterval = _settings.PollInterval;

            var exceptionsPolicy = Policy
                .Handle<Exception>(ex =>
                {
                    var httpCode = ex is HttpRequestException http
                        ? http.StatusCode : ex is ApiException api
                        ? (HttpStatusCode)api.StatusCode
                        : null;

                    return httpCode != null && retryOn.Contains(httpCode.Value);
                })
                .WaitAndRetryAsync(retryCount, i =>
                {
                    progress?.Report($"Checking operation #{i + 1}");
                    return pollInterval + TimeSpan.FromSeconds((i + 1) * 2);
                })
                .AsAsyncPolicy<byte[]>();

            var emptyResultPolicy = Policy
                .HandleResult<byte[]>(result => result == null || result.Length == 0)
                .WaitAndRetryForeverAsync(i =>
                {
                    progress?.Report($"Checking operation #{i + 1}");
                    return pollInterval;
                });

            return Policy.WrapAsync(exceptionsPolicy, emptyResultPolicy);
        }

        protected void SaveFileData(FileDescriptor file, byte[] data)
        {
            var path = file.TargetFilePath;

            try
            {
                var outputDirectory = new FileInfo(path).Directory;
                if (outputDirectory == null)
                {
                    throw new ApplicationException("Unable to determine target directory for a file");
                }
                outputDirectory.Create();

                File.WriteAllBytes(path, data);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to save converted file data", ex);
            }
        }
    }

    internal struct ConversionReport
    {
        public readonly FileDescriptor File;
        public readonly string? State;
        public readonly bool IsCompleted;
        public readonly bool IsCanceled;
        public readonly Exception? Error;

        public ConversionReport(
            FileDescriptor file,
            string? state,
            bool isCompleted = false,
            bool isCanceled = false,
            Exception? error = null
        ) : this()
        {
            File = file;
            State = state;
            IsCompleted = isCompleted;
            IsCanceled = isCanceled;
            Error = error;
        }
    }


}
