$srcPath = [System.IO.Path]::GetFullPath(($PSScriptRoot + '\..\..\src'))


# delete existing packages
Remove-Item $PSScriptRoot\*.nupkg

nuget pack $srcPath\SenseNet.Compatibility\SenseNet.Compatibility.nuspec -properties Configuration=Release -OutputDirectory $PSScriptRoot