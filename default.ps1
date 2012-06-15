Framework "4.0"

Properties {
    $build_dir = Split-Path $psake.build_script_file
    $build_artifacts_dir = "$build_dir\build\"
    $solution_file = "$build_dir\solutionizer.sln"
	$nupack_dir = "$build_dir\NuPack"
}

task Default -depends Clean, Compile

task CreateAssemblyInfo {
    $gittag = & git describe --tags --long
    $gittag

    if (!($gittag -match '^v(?<major>\d+)\.(?<minor>\d+)(\.(?<patch>\d+))?-(?<revision>\d+)-(?<commit>[a-z0-9]+)$')){
        throw "$gittag is not recognized"
    }
    $majorVersion = $matches['major']
    $minorVersion = $matches['minor']
    $patchVersion = $matches['patch']
    $revisionCount = $matches['revision']
    $commitVersion = $matches['commit']

    Write-Host "Current version: $majorVersion.$minorVersion.$patchVersion.$revisionCount-$commitVersion"

    $Script:version = "$majorVersion.$minorVersion.$patchVersion.$revisionCount"
    $fileversion = "$majorVersion.$minorVersion.$patchVersion.$revisionCount"
    $asmInfo = "using System.Reflection;

[assembly: AssemblyVersion(""$majorVersion.$minorVersion.0"")]
[assembly: AssemblyInformationalVersion(""$majorVersion.$minorVersion.$patchVersion.$revisionCount-$commitVersion"")]
[assembly: AssemblyFileVersion(""$majorVersion.$minorVersion.$patchVersion.$revisionCount"")]"

    $file = Join-Path $build_dir "CommonAssemblyInfo.cs"
    Write-Host "Generating assembly info file $file"
    Write-Output $asmInfo > $file
}

task Compile -depends CreateAssemblyInfo {
    Write-Host "Building $solution_file" -ForegroundColor Green
    Exec { msbuild "$solution_file" /v:minimal /p:OutDir=$build_artifacts_dir }
}

Task Clean {
    Write-Host "Creating BuildArtifacts directory" -ForegroundColor Green
    if (Test-Path $build_artifacts_dir) {   
        rd $build_artifacts_dir -rec -force | out-null
    }
    
    mkdir $build_artifacts_dir | out-null
    
    Write-Host "Cleaning $solution_file" -ForegroundColor Green
    Exec { msbuild $solution_file /t:Clean /p:Configuration=Release /v:minimal } 
}

Task Package -depends CreateAssemblyInfo, Compile {
	Remove-Item $nupack_dir -Force -Recurse -ErrorAction SilentlyContinue
	New-Item $nupack_dir -Type directory | Out-Null

	$files = @(
		"Solutionizer.exe",
		"Solutionizer.exe.config",
		"Caliburn.Micro.dll",
		"Caliburn.Micro.Logging.dll",
		"Caliburn.Micro.Logging.NLog.dll",
		"MahApps.Metro.dll",
		"Newtonsoft.Json.dll",
		"NLog.dll",
		"nunit.framework.dll",
		"Ookii.Dialogs.Wpf.dll",
		"Solutionizer.Tests.dll",
		"System.Windows.Interactivity.dll",
		"WPFSpark.dll")

	$xml = [xml]"<package xmlns='http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd'>
  <metadata>
    <version>$version</version>
    <authors>thoemmi</authors>
    <owners>thoemmi</owners>
    <projectUrl>https://github.com/thoemmi/Solutionizer</projectUrl>
    <id>Solutionizer</id>
    <title>Solutionizer</title>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <description>Solutionizer enables you to create ad-hoc solutions for Visual Studio</description>
    <releaseNotes>Initial release.</releaseNotes>
    <copyright>Copyright 2012</copyright>
    <tags>solution project</tags>
  </metadata></package>"

	$filesElem = $xml.CreateElement("files")	
  	$files | ForEach-Object {
		$elem = $xml.CreateElement("file")
		$elem.SetAttribute("src", (Join-Path $build_artifacts_dir $_))
		$elem.SetAttribute("target", "lib\net40\$_")
		$filesElem.AppendChild($elem) | Out-Null
	}
	$xml.package.AppendChild($filesElem) | Out-Null

	$nuspec_path = (Join-Path $nupack_dir "solutionizer.nuspec")
	$xml.Save($nuspec_path) | Out-Null
	& "$build_dir\.nuget\nuget.exe" pack $nuspec_path -OutputDirectory $nupack_dir -NoPackageAnalysis
    $package_path = Join-Path $nupack_dir "solutionizer.$version.nupkg"
    if (!(Test-Path $package_path)) {
        throw "The new package was not created with the expected name `"$package_path`""
    }
    
    # we need to load NSync manually until there's a better solution
    [Reflection.Assembly]::LoadFile("$build_dir\tools\NSync\NSync.Core.dll") | Out-Null
   
    $releaseEntry = [NSync.Core.ReleaseEntry]::GenerateFromFile($package_path)
    
    # though we call Add-Content, the RELEASES file will contain only this release entry, because at the start of this task we delete the NuPack folder
    Add-Content "$nupack_dir\RELEASES" -value "$($releaseEntry.EntryAsString)"
}