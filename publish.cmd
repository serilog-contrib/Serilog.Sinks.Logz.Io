@ECHO OFF

set version=7.1.0
set output=./packages

dotnet restore .

dotnet build . /p:Version=%version% --configuration=Release --no-restore

dotnet pack ./src/Serilog.Sinks.Logz.Io/Serilog.Sinks.Logz.Io.csproj -o %output% /p:Version=%version% --configuration=Release --no-restore --no-build

