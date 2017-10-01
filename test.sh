if [ "$1" == "DEBUG" ]
  then
  echo "TEST: CLI (DEBUG)"
  echo ""
  dotnet run --configuration Debug
  echo ""
else
  echo "TEST: CLI"
  echo ""
  dotnet run --configuration Release
  echo ""
fi