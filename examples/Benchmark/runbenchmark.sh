dotnet run -p ../Example.Api/Example.Api.csproj --urls=http://localhost:5002 &
API_PID=$!
dotnet run -p ../Example.Controllers.Api/Example.Controllers.Api.csproj --urls=http://localhost:5003 &
CAPI_PID=$!

dotnet run -c Release

kill $API_PID
kill $CAPI_PID
