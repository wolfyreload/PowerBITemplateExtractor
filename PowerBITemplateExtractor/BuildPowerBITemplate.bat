:: set folder to path of batch file
pushd %~dp0
.\PowerBITemplateExtractor.exe Import --Config PowerBISourceControlConfig.json
popd