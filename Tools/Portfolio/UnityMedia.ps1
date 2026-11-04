param(
    [ValidateSet('Capture', 'Verify')][string]$Action = 'Capture',
    [string]$UnityPath = 'C:\Program Files\Unity\Hub\Editor\6000.3.19f1\Editor\Unity.exe'
)
$ErrorActionPreference = 'Stop'
$taskProject = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
[string[]]$taskNames = if ($Action -eq 'Capture') { @('PortfolioCaptureTests', 'PortfolioGalleryTests') } else { @('PortfolioVideoValidationTests') }
$taskFixtures = @($taskNames | ForEach-Object { [System.IO.Path]::GetFullPath((Join-Path $taskProject "Assets/TetherRush/Tests/PlayMode/$_.cs")) })
foreach ($taskFixture in $taskFixtures) {
    if (!$taskFixture.StartsWith($taskProject + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw 'Temporary fixture must remain inside this checkout.'
    }
    if ((Test-Path -LiteralPath $taskFixture) -or (Test-Path -LiteralPath ($taskFixture + '.meta'))) {
        throw "Refusing to overwrite existing fixture: $taskFixture"
    }
}
$taskManifestPath = Join-Path $taskProject 'Packages/manifest.json'
$taskLockPath = Join-Path $taskProject 'Packages/packages-lock.json'
$taskAsmPath = Join-Path $taskProject 'Assets/TetherRush/Tests/PlayMode/TetherRush.PlayModeTests.asmdef'
$taskManifestBytes = [System.IO.File]::ReadAllBytes($taskManifestPath)
$taskLockBytes = [System.IO.File]::ReadAllBytes($taskLockPath)
$taskAsmBytes = [System.IO.File]::ReadAllBytes($taskAsmPath)
$taskExitCode = 1
try {
    # The opt-in editor tools need the built-in Video module and NativeArray.
    # All product/package files are restored byte-for-byte in finally.
    $taskManifest = [System.Text.Encoding]::UTF8.GetString($taskManifestBytes) | ConvertFrom-Json
    $taskManifest.dependencies | Add-Member -NotePropertyName 'com.unity.modules.video' -NotePropertyValue '1.0.0' -Force
    [System.IO.File]::WriteAllText($taskManifestPath, ($taskManifest | ConvertTo-Json -Depth 100), [System.Text.UTF8Encoding]::new($false))
    $taskAsm = [System.Text.Encoding]::UTF8.GetString($taskAsmBytes) | ConvertFrom-Json
    if ($taskAsm.references -notcontains 'Unity.Collections') { $taskAsm.references = @($taskAsm.references) + 'Unity.Collections' }
    [System.IO.File]::WriteAllText($taskAsmPath, ($taskAsm | ConvertTo-Json -Depth 100), [System.Text.UTF8Encoding]::new($false))
    for ($taskIndex = 0; $taskIndex -lt $taskNames.Count; $taskIndex++) {
        $taskTemplate = Join-Path $PSScriptRoot ($taskNames[$taskIndex] + '.cs.template')
        [System.IO.File]::WriteAllBytes($taskFixtures[$taskIndex], [System.IO.File]::ReadAllBytes($taskTemplate))
    }
    $taskFilter = if ($Action -eq 'Capture') {
        'TetherRush.Tests.PortfolioCaptureTests.ExportRealGameplayPortfolio;TetherRush.Tests.PortfolioGalleryTests.CaptureMenuAndLevelMap'
    } else { 'TetherRush.Tests.PortfolioVideoValidationTests.DecodePublishedTrailer' }
    & (Join-Path $taskProject 'Tools/Unity.ps1') -Action PlayMode -UnityPath $UnityPath -TestFilter $taskFilter
    $taskExitCode = $LASTEXITCODE
} finally {
    [System.IO.File]::WriteAllBytes($taskManifestPath, $taskManifestBytes)
    [System.IO.File]::WriteAllBytes($taskLockPath, $taskLockBytes)
    [System.IO.File]::WriteAllBytes($taskAsmPath, $taskAsmBytes)
    # These are only the newly-created fixtures, checked above; no recursive removal.
    foreach ($taskFixture in $taskFixtures) {
        if (Test-Path -LiteralPath $taskFixture) { Remove-Item -LiteralPath $taskFixture }
        if (Test-Path -LiteralPath ($taskFixture + '.meta')) { Remove-Item -LiteralPath ($taskFixture + '.meta') }
    }
}
exit $taskExitCode
