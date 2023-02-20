using System.Diagnostics;
using System.Runtime.InteropServices;

//Launcher.LaunchEditor("File.txt", "code", false, true); //With Verbose
//Launcher.LaunchEditor("File.txt", "notepad", false, true);
//Launcher.LaunchEditor("File.txt", "ignored-command-name", TryVsCode: true, Verbose: true); //Ignore editor, prefers VSCode.
//Launcher.LaunchEditor("File.txt", "bad-command-name", TryVsCode: false, Verbose: true); //Bad command name. Try to launch the default.
//Launcher.LaunchEditor("File.txt", "C:/Program Files/Notepad++/notepad++.exe", Verbose: true);
//Launcher.LaunchEditor("File.txt", "C:/Program Files/Sublime Text/sublime_text.exe", Verbose: true);
//Launcher.LaunchEditor("File.txt", "vim", Verbose: true);
//Launcher.LaunchEditor("File.txt", "vi", Verbose: true); 
//Launcher.LaunchEditor("File.txt", "nano", Verbose: true); 
//Launcher.LaunchEditor("File.txt", "", Verbose: true); 

Console.WriteLine("███ Opening file with text Editor");
string editor = "";
while (!editor.Equals("exit!"))
{
    Console.WriteLine(@"Type the editor command/filename OR exit [exit!]:");
    editor = Console.ReadLine() ?? string.Empty;
    if (!editor.Equals("exit!"))
    {
        Launcher.LaunchEditor("File.txt", editor, verbose: true);
    }
}


public class Launcher
{
    private static void OpenProcess(string command, string parameters, string textFilename, bool verbose)
    {
        if (!string.IsNullOrWhiteSpace(command))
        {
            if (verbose)
            {
                Console.WriteLine($@"Opening ""{textFilename}"" with ""{command}"" editor.");
            }
            var process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = command;
            process.StartInfo.Arguments = parameters;
            process.Start();
        }
        else
        {
            Console.WriteLine(@$"No editor was found for file ""{textFilename}"".");
        }
    }

    /// <summary>
    /// Launchs the specified editor. Using Process class, to start new process of PowerShell(pwsh) with the script as parameter and the script required parameters. If editor specified is not found, it try to launch the default.
    /// </summary>
    /// <param name="textFilename">Path of text file.</param>
    /// <param name="editor">The editor command.</param>
    /// <param name="tryVsCode">If Visual Studio Code is available. The file is opened with VSCode. Editor value is ignored.</param>
    /// <param name="verbose">Prints parameters.</param>
    public static void LaunchEditor(string textFilename, string editor = "", bool tryVsCode = false, bool verbose = false)
    {
        var command = CommandExists(editor);
        List<string> windowsCommands = new List<string> { "notepad", "code" };
        List<string> linuxCommands = new List<string> { "nano", "vim", "vi" };

        if (tryVsCode)
        {
            command = CommandExists("code");
            if (!string.IsNullOrWhiteSpace(command))
            {
                OpenProcess(command, @$"""{textFilename}""", textFilename, verbose);
                return;
            }
        }

        command = CommandExists(editor);
        if (!string.IsNullOrWhiteSpace(command))
        {
            OpenProcess(command, @$"""{textFilename}""", textFilename, verbose);
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            foreach (var cmd in windowsCommands)
            {
                command = CommandExists(cmd);
                if (!string.IsNullOrWhiteSpace(command))
                {
                    OpenProcess(command, @$"""{textFilename}""", textFilename, verbose);
                    return;
                }
            }
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            foreach (var cmd in linuxCommands)
            {
                command = CommandExists(cmd);
                if (!string.IsNullOrWhiteSpace(command))
                {
                    OpenProcess(command, @$"""{textFilename}""", textFilename, verbose);
                    return;
                }
            }
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            OpenProcess("open", @$"-t ""{textFilename}""", textFilename, verbose);
        }
    }


    static string CommandExists(string filename)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var paths = new[] { Environment.CurrentDirectory }
                    .Concat(Environment.GetEnvironmentVariable("PATH")!.Split(';'))
                    .Where(p => !string.IsNullOrWhiteSpace(p));

            var extensions = new[] { String.Empty }
                    .Concat(Environment.GetEnvironmentVariable("PATHEXT")!.Split(';')
                               .Where(e => e.StartsWith(".")));

            var combinations = paths.SelectMany(x => extensions,
                    (path, extension) => Path.Combine(path, filename + extension));
            return combinations.FirstOrDefault(File.Exists) ?? string.Empty;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var paths = new[] { Environment.CurrentDirectory }
                    .Concat(Environment.GetEnvironmentVariable("PATH")!.Split(':'))
                    .Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => Path.Combine(p, filename));
            return paths.FirstOrDefault(File.Exists) ?? string.Empty;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var paths = new[] { Environment.CurrentDirectory }
                    .Concat(Environment.GetEnvironmentVariable("PATH")!.Split(':'))
                    .Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => Path.Combine(p, filename));
            return paths.FirstOrDefault(File.Exists) ?? string.Empty;
        }
        throw new NotSupportedException(RuntimeInformation.OSDescription);
    }

}
