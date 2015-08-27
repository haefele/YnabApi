$root = (split-path -parent $MyInvocation.MyCommand.Definition) + '\..';

$version = [System.Reflection.Assembly]::LoadFile("$root\src\YnabApi\bin\Release\YnabApi.dll").GetName().Version;
$versionAsString = "{0}.{1}.{2}" -f ($version.Major, $version.Minor, $version.Build);

$nuspecPath = $root + "\nuget\YnabApi.nuspec";
[xml]$content = Get-Content $nuspecPath; 
$content.package.metadata.version = $versionAsString;
$content.Save($nuspecPath);

& nuget pack $nuspecPath;