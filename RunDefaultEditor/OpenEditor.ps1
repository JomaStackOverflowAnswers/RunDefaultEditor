[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $TextFileName,

    [string]
    $Editor = [string]::Empty,

    [switch]
    $TryVSCode
)

$IsVerbose = $PSBoundParameters.Keys.Contains("Verbose")

function Show-Message {
    if ($IsVerbose) {
        Write-Output "Opening text editor ""$Editor"" for file ""$TextFileName"" "
    }
}

$windowsEditors = @("notepad", "code")
$linuxEditors = @("nano", "vim", "vi", "code")

if ($TryVSCode.IsPresent) {
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


