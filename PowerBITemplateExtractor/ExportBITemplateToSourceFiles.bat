:: set folder to path of batch file
pushd %~dp0
.\PowerBITemplateExtractor.exe  Export --Config PowerBISourceControlConfig.json
popd