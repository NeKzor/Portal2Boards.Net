if [ "$1" = "TEST" ]; then
  dotnet build src/Portal2Boards.Net.Test/ -c ${2-CHA}
else
  dotnet build src/Portal2Boards.Net/ -c Release
fi