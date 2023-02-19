using System.Diagnostics;
using System.Text;

Console.WriteLine("███ Opening file with text Editor");
Launcher.LaunchEditor("File.txt", "code", false, true); //With Verbose
Launcher.LaunchEditor("File.txt", "notepad", false, true);
Launcher.LaunchEditor("File.txt", "ignored-command-name", TryVsCode: true, Verbose: true); //Ignore editor, prefers VSCode.
Launcher.LaunchEditor("File.txt", "bad-command-name", TryVsCode: false, Verbose: true); //Bad command name. Try to launch the default.
Launcher.LaunchEditor("File.txt", "C:/Program Files/Notepad++/notepad++.exe", Verbose: true);
Launcher.LaunchEditor("File.txt", "C:/Program Files/Sublime Text/sublime_text.exe", Verbose: true);
Launcher.LaunchEditor("File.txt", "vim", Verbose: true); 
Console.ReadLine();

public class Launcher
{
    /// <summary>
    /// Launchs the specified editor. Using Process class, to start new process of PowerShell(pwsh) with the script as parameter and the script required parameters. If editor specified is not found, it try to launch the default.
    /// </summary>
    /// <param name="TextFileName">Path of text file.</param>
    /// <param name="Editor">The editor command.</param>
    /// <param name="TryVsCode">If Visual Studio Code is available. The file is opened with VSCode. Editor value is ignored.</param>
    /// <param name="Verbose">Prints parameters.</param>
    public static void LaunchEditor(string TextFileName, string Editor = "", bool TryVsCode = false, bool Verbose = false)
    {
        var sb = new StringBuilder(ScriptCodeForProcess);
        sb.Replace("##1##", $@"${nameof(TextFileName)}=""{TextFileName}""");
        sb.Replace("##2##", $@"${nameof(Editor)}=""{Editor}""");
        sb.Replace("##3##", $@"${nameof(TryVsCode)}=${TryVsCode}");
        sb.Replace("##4##", $@"$Is{nameof(Verbose)}=${Verbose}");
        var script = Convert.ToBase64String(Encoding.Unicode.GetBytes(sb.ToString()));
        var process = new Process();
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.FileName = @"pwsh";
        string scriptFile = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}{Guid.NewGuid().ToString().Replace("-", string.Empty)}.ps1";
        File.WriteAllText(scriptFile, ScriptCodeForProcess, Encoding.UTF8);
        process.StartInfo.Arguments = @$"-NoLogo -ExecutionPolicy ByPass -EncodedCommand {script}";
        process.Start();
        File.Delete(scriptFile);
    }


    private const string ScriptCodeForProcess = """
            ##1##
            ##2##
            ##3##
            ##4##

            function Show-Message {
                if ($IsVerbose) {
                    Write-Output "Opening text editor ""$Editor"" for file ""$TextFileName"" "
                }
            }

            $windowsEditors = @("notepad", "code")
            $linuxEditors = @("nano", "vim", "vi", "code")

            if ($TryVSCode) {
                $Editor1 = (Get-Command "code" -ErrorAction Ignore).Source
                if ($Editor1) {
                    $Editor = "code"
                    Show-Message
                    & "$Editor" "$TextFileName"
                    return
                }
            }

            if (![string]::IsNullOrWhiteSpace($Editor)) {
                $Editor = (Get-Command $Editor -ErrorAction Ignore).Source
                if ($Editor) {
                    Show-Message
                    & "$Editor" "$TextFileName"
                    return
                }
            }


            if ($IsWindows) {
                foreach ($Editor in $windowsEditors) {
                    $Editor = (Get-Command $Editor -ErrorAction Ignore).Source
                    if ($Editor) { 
                        Show-Message
                        & "$($Editor)" "$TextFileName"
                        return
                    }
                }
            }

            if ($IsLinux) {
                foreach ($Editor in $linuxEditors) {
                    $Editor = (Get-Command $Editor -ErrorAction Ignore).Source
                    if ($Editor) { 
                        Show-Message
                        & "$($Editor)" "$TextFileName"
                        return
                    }
                }
            }

            if ($IsMacOS) {
                $Editor = "Default text editor"
                Show-Message
                & "open" -t "$TextFileName"
                return
            }

            throw [System.Exception]::new("No text editor was found.")


            
            """;  
}
