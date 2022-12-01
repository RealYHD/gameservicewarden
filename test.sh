#!/bin/bash
for PROJ in tests/**/*.csproj
do
    echo "Testing $PROJ"
    dotnet test --logger trx -v n --blame --collect:"XPlat Code Coverage" "$PROJ" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
done