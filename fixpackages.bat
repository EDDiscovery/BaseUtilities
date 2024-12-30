rgrep "..\packages" /r .csproj -rep "$(SolutionDir)\packages"
rgrep "..\\$(SolutionDir)\\" /r .csproj -rep "$(SolutionDir)\\"


