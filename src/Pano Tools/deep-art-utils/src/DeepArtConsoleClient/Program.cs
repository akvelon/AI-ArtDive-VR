using DeepArtConsoleClient;
using DeepArtConsoleClient.Errors;
using DeepArtConsoleClient.Settings;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;



var command = new RootCommand("Converts files using Deep Art API") { TreatUnmatchedTokensAsErrors = true };

var binder = new AppSettingsBinder();
command.Add(binder.Input);
command.Add(binder.InputHiddenOption);
command.Add(binder.InputDirectory);
command.Add(binder.OutputDirectory);
command.Add(binder.SilentMode);
command.Add(binder.Effect);
command.Add(binder.Timeout);
command.Add(binder.Recursive);
command.Add(binder.FileMask);
command.Add(binder.ConfirmOverwrite);
command.Add(binder.ParallelismDegree);
command.Add(binder.MediaType);
command.Add(binder.SaveReports);
command.Add(binder.ApiUrl);
command.Add(binder.HttpTimeout);
command.Add(binder.PollInterval);

command.SetHandler(async (InvocationContext context) =>
{
    var appSettings = binder.GetValue(context.BindingContext);
    var cancellation = context.GetCancellationToken();

    await new Worker(appSettings).DoWork(cancellation);
});

Environment.ExitCode = new CommandLineBuilder(command)
    .UseVersionOption("version", "-v", "--version", "/v", "/version")
    .UseHelp("help", "-h", "--help", "/h", "/help", "/?", "?")
    .UseHelp(context => context.HelpBuilder.CustomizeLayout(
        _ => HelpBuilder.Default.GetLayout()
            .Append(PrintExitCodes)
            .Append(PrintExamples)
    ))
    .UseEnvironmentVariableDirective()
    .UseParseDirective()
    .UseSuggestDirective()
    .RegisterWithDotnetSuggest()
    .UseTypoCorrections()
    .UseParseErrorReporting((int)AppErrorCode.InvalidSettings)
    .CancelOnProcessTermination()
    .UseExceptionHandler(HandleError)
    .Build()
    .Invoke(args);


static void HandleError(Exception ex, InvocationContext context)
{
    PrintError(ex);

    context.ExitCode = (int)(ex is DeepArtConvertException deepArtEx
        ? deepArtEx.ErrorCode
        : AppErrorCode.Unknown
    );
}

static void PrintError(Exception ex)
{
    if (ex is TaskCanceledException || ex is OperationCanceledException)
    {
        Console.WriteLine("Conversion stopped");
        return;
    }

    if (ex is AggregateException aggregate)
    {
        foreach (var inner in aggregate.InnerExceptions)
        {
            PrintError(inner);
        }
        return;
    }

    Console.Error.WriteLine(ex.Message);

    var rootEx = ex.GetBaseException();
    if (rootEx != null && rootEx != ex)
    {
        Console.Error.WriteLine(rootEx.Message);
    }

    Console.Error.WriteLine();
}

static void PrintExitCodes(HelpContext context)
{
    context.Output.WriteLine("Returns:");
    context.Output.WriteLine("    1 - Unknown error");
    context.Output.WriteLine("  101 - Invalid command line args or appsettings.json");
    context.Output.WriteLine("  102 - Failed to convert one or more files. If --save-reports is set to \"All\" or \"FailuresOnly\", then error details can bee seen in *.failed files");
    context.Output.WriteLine("  110 - No files to convert. Ensure --input, --inputDirectory, --file-mask, --recursive are valid");
    context.Output.WriteLine("  111 - Failed to receive Deep Art effects. Ensure --api-url is valid. DeepArtConsoleClient expects API v1.0");
    context.Output.WriteLine("  112 - No available effects. Contact Deep Art team");
    context.Output.WriteLine("  113 - Specified effect not found or not unique. Ensure --effect and --media-type are valid");
    context.Output.WriteLine("  114 - Overall timeout. Try increasing --timeout value. If --save-reports was set to \"All\", then on next run the conversion will continue, otherwise a new conversion will be started");
    context.Output.WriteLine();
}

static void PrintExamples(HelpContext context)
{
    context.Output.WriteLine("Examples:");
    context.Output.WriteLine();
    context.Output.WriteLine("  DeepArtConsoleClient");
    context.Output.WriteLine("    All files in current directory are converted. App suggests to choose effect and asks confirmation before overwriting files.");
    context.Output.WriteLine();
    context.Output.WriteLine("  DeepArtConsoleClient file1.png file2.jpg -e Pen -s");
    context.Output.WriteLine("    Specified files are converted into Pen effect in silent mode.");
    context.Output.WriteLine();
    context.Output.WriteLine("  DeepArtConsoleClient /path/to/input/dir -o /path/to/output/dir -e WATERCOLOR -sr");
    context.Output.WriteLine("    All png|jpg|jpeg files (recursively) in /path/to/input/dir are converted into Watercolor effect in silent mode. Converted files are saved to /path/to/output/dir preserving source directory structure.");
    context.Output.WriteLine();
    context.Output.WriteLine("  DeepArtConsoleClient /path/to/input/dir -m *.* -e WATERCOLOR -s -t 00:20:00");
    context.Output.WriteLine("    All files (non-recursively) in /path/to/input/dir are converted into Watercolor effect in silent mode. If conversion is not completed in 20 minutes, it is stopped.");
    context.Output.WriteLine();
    context.Output.WriteLine("  DeepArtConsoleClient /path/to/input/dir -y --poll-interval 00:00:05");
    context.Output.WriteLine("     All png|jpg|jpeg files (recursively) in /path/to/input/dir are converted. App suggests to choose effect and overwrites files withour confirmation. Timespan between Deep Art API requests is 5 seconds.");
    context.Output.WriteLine();
    context.Output.WriteLine("  DeepArtConsoleClient /path/to/input/dir -y --poll-interval 5");
    context.Output.WriteLine("    Same as previous example.");
    context.Output.WriteLine();
}
