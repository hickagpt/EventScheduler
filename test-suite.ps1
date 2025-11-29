dotnet build EventScheduler.sln
dotnet test --collect:"XPlat Code Coverage"

# Generate coverage report from all coverage files in test project TestResults directories
reportgenerator -reports:"*/TestResults/*/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
reportgenerator -reports:"*/TestResults/*/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:TextSummary
