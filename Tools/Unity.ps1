param(
    [ValidateSet('Prepare', 'EditMode', 'PlayMode', 'Android', 'Windows')]
    [string]$Action = 'Prepare',
    [string]$UnityPath = 'C:\Program Files\Unity\Hub\Editor\6000.3.19f1\Editor\Unity.exe'
)
$ErrorActionPreference = 'Stop'
$taskProject = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
# Android's toolchain rejects non-ASCII project paths. A temporary junction keeps
# the same checkout/cache and gives the tools an ASCII path without moving files.
if ($Action -eq 'Android' -and $taskProject -match '[^\x00-\x7F]') {
    $taskHasher = [System.Security.Cryptography.SHA256]::Create()
    try { $taskHashBytes = $taskHasher.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($taskProject)) }
    finally { $taskHasher.Dispose() }
    $taskHash = [BitConverter]::ToString($taskHashBytes).Replace('-', '').Substring(0, 12)
    $taskLink = Join-Path ([System.IO.Path]::GetTempPath()) "TetherRush-Android-$taskHash"
    if ($taskLink -match '[^\x00-\x7F]') { throw 'Android needs an ASCII-only project path; TEMP also contains non-ASCII characters.' }
    if (Test-Path -LiteralPath $taskLink) {
        $taskExisting = Get-Item -LiteralPath $taskLink -Force
        if ($taskExisting.LinkType -ne 'Junction' -or [System.IO.Path]::GetFullPath([string](@($taskExisting.Target)[0])) -ne $taskProject) {
            throw "Refusing to reuse an unrelated path: $taskLink"
        }
    } else {
        New-Item -ItemType Junction -Path $taskLink -Target $taskProject | Out-Null
    }
    $taskProject = $taskLink
    Write-Output "Android build uses temporary project junction: $taskProject"
}
$taskLogs = Join-Path $taskProject 'output/validation'
New-Item -ItemType Directory -Force -Path $taskLogs | Out-Null
if (!(Test-Path -LiteralPath $UnityPath)) { throw "Unity executable missing: $UnityPath" }
$taskArgs = @('-batchmode', '-projectPath', ('"' + $taskProject + '"'), '-logFile', ('"' + (Join-Path $taskLogs "$Action.log") + '"'))
if ($Action -eq 'EditMode' -or $Action -eq 'PlayMode') {
    # The test runner exits itself. -quit can terminate it before results are written.
    $taskArgs += @('-runTests', '-testPlatform', $Action, '-testResults', ('"' + (Join-Path $taskLogs "$Action.xml") + '"'))
} else {
    $taskMethod = switch ($Action) {
        'Prepare' { 'PrepareProject' }
        'Android' { 'BuildAndroid' }
        'Windows' { 'BuildWindows' }
    }
    $taskArgs += @('-quit', '-executeMethod', "TetherRush.Editor.ProjectTools.$taskMethod")
}
$taskProcess = Start-Process -FilePath $UnityPath -ArgumentList $taskArgs -WorkingDirectory $taskProject -WindowStyle Hidden -PassThru
# Wait for the Editor, not long-lived compiler-server descendants.
$taskProcess.WaitForExit()
Write-Output "Unity $Action exit code: $($taskProcess.ExitCode). Log: $taskLogs"
exit $taskProcess.ExitCode
