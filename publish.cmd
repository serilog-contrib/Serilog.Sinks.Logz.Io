@ECHO OFF

set version1=8.0.0
set version2=4.0.0

set output=./packages

dotnet restore .

dotnet pack ./src/Serilog.Sinks.Logz.Io/Serilog.Sinks.Logz.Io.csproj -o %output% /p:Version=%version1% --configuration=Release --no-restore
dotnet pack ./src/Serilog.Sinks.Http.LogzIo/Serilog.Sinks.Http.LogzIo.csproj -o %output% /p:Version=%version2% --configuration=Release --no-restore

