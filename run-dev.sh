#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
cd "$ROOT"

echo "Building..."
dotnet build AzilEdu.sln

echo "Starting API on https://localhost:7205 ..."
dotnet run --project AzilEdu.Api/AzilEdu.Api.csproj --launch-profile https &
API_PID=$!

sleep 3

echo "Starting App on https://localhost:7094 ..."
dotnet run --project AzilEdu.App/AzilEdu.App.csproj --launch-profile https &
APP_PID=$!

echo ""
echo "API PID: $API_PID  -> https://localhost:7205/swagger"
echo "App PID: $APP_PID  -> https://localhost:7094"
echo "Press Ctrl+C to stop both."

trap 'kill $API_PID $APP_PID 2>/dev/null; exit' INT TERM
wait
