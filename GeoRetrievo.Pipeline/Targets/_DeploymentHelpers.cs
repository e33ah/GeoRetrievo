namespace GeoRetrievo.Pipeline;

public partial class Build : FalloutBuild {
    private static void RunCommand(
        string fileName,
        IEnumerable<string> arguments,
        AbsolutePath workingDirectory) {
        var commandArguments = arguments.ToList();
        var startInfo = OperatingSystem.IsWindows()
            && string.Equals(fileName, "gcloud", StringComparison.OrdinalIgnoreCase)
                ? CreateWindowsGCloudStartInfo(commandArguments, workingDirectory)
                : new ProcessStartInfo(fileName) {
                    UseShellExecute = false,
                    WorkingDirectory = workingDirectory.ToString(),
                };

        if (!OperatingSystem.IsWindows()
            || !string.Equals(fileName, "gcloud", StringComparison.OrdinalIgnoreCase)) {
            foreach (string argument in commandArguments) {
                startInfo.ArgumentList.Add(argument);
            }
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Could not start '{fileName}'.");

        Log.Information("Started command {Command}", FormatCommand(fileName, commandArguments));
        process.WaitForExit();
        if (process.ExitCode != 0) {
            Log.Error(
                "Command {Command} failed with exit code {ExitCode}",
                FormatCommand(fileName, commandArguments),
                process.ExitCode);
            throw new InvalidOperationException(
                $"'{fileName}' exited with code {process.ExitCode}.");
        }

        Log.Information("Command {Command} completed successfully", FormatCommand(fileName, commandArguments));
    }

    private static ProcessStartInfo CreateWindowsGCloudStartInfo(
        IReadOnlyCollection<string> arguments,
        AbsolutePath workingDirectory) {
        if (arguments.Any(argument => argument.Any(character =>
                !char.IsAsciiLetterOrDigit(character)
                && character is not '.' and not '_' and not ':' and not '/' and not '-'))) {
            throw new InvalidOperationException(
                "Google Cloud CLI arguments contain characters that cannot safely be passed through cmd.exe.");
        }

        var startInfo = new ProcessStartInfo(
            Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe") {
            UseShellExecute = false,
            WorkingDirectory = workingDirectory.ToString(),
        };

        startInfo.ArgumentList.Add("/d");
        startInfo.ArgumentList.Add("/s");
        startInfo.ArgumentList.Add("/c");
        startInfo.ArgumentList.Add($"gcloud.cmd {string.Join(" ", arguments)}");
        return startInfo;
    }

    private static string FormatCommand(string fileName, IEnumerable<string> arguments) =>
        $"{fileName} {string.Join(" ", arguments)}";

    private string GetCurrentCommitTag() {
        var startInfo = new ProcessStartInfo("git") {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            WorkingDirectory = RootDirectory.ToString(),
        };
        startInfo.ArgumentList.Add("rev-parse");
        startInfo.ArgumentList.Add("--short=12");
        startInfo.ArgumentList.Add("HEAD");

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start 'git'.");

        string tag = process.StandardOutput.ReadToEnd().Trim();
        process.WaitForExit();
        if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(tag)) {
            throw new InvalidOperationException("Could not determine the current Git commit for the image tag.");
        }

        return tag;
    }
}
