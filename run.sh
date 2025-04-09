#!/bin/bash

set -e

echo
echo "-----------------------------"
echo "📦 Restoring dependencies..."
echo "-----------------------------"
dotnet restore

echo
echo "-----------------------------"
echo "🔨 Building the solution..."
echo "-----------------------------"
dotnet build --no-restore

echo
echo "-----------------------------"
echo "⚙️ Running unit tests..."
echo "-----------------------------"
dotnet test Optiplan.UnitTests --no-build

echo
echo "-----------------------------"
echo "🧪 Running the application..."
echo "-----------------------------"
dotnet run -p Optiplan.WebApi --launch-profile https

echo
echo "🎉 All steps completed successfully!"