#!/bin/bash

set -e

echo "---------------------------------"
echo "🔍 Checking for .NET SDK 8.0.407"
echo "---------------------------------"
if ! dotnet --list-sdks | grep -q "8.0.407"; then
  echo "❌ .NET SDK 8.0.407 not found. Please install it from:"
  echo "   https://dotnet.microsoft.com/en-us/download/dotnet/8.0"
  exit 1
fi
echo "✅ .NET SDK 8.0.407 is installed."

echo
echo "-------------------------"
echo "🔍 Checking SQLite3"
echo "-------------------------"
if ! command -v sqlite3 >/dev/null 2>&1; then
  echo "❌ SQLite3 is not installed or not in PATH. Install it from:"
  echo "   https://sqlite.org/download.html"
  exit 1
fi

sqlite_version=$(sqlite3 -version | awk '{print $1}')
echo "✅ SQLite3 is installed (version $sqlite_version)"

latest_version="3.49.1"
if [[ "$sqlite_version" < "$latest_version" ]]; then
  echo "⚠️ SQLite3 version is older than $latest_version. Consider updating."
fi

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
echo "🗃️ Initializing database..."
echo "-----------------------------"
db_file="Optiplan.db"
sql_script="OptiplanSqlite3.sql"
if [ -f "$db_file" ]; then
  rm -f "$db_file"
fi
sqlite3 "$db_file" < "$sql_script"
echo "✅ Database initialized as $db_file"

echo
echo "-----------------------------"
echo "🔒 Generating development HTTPS certificate..."
echo "-----------------------------"
dotnet dev-certs https --clean
dotnet dev-certs https --trust

echo
echo "-----------------------------"
echo "🧪 Running unit tests..."
echo "-----------------------------"
dotnet test Optiplan.UnitTests --no-build

echo
echo "🎉 All steps completed successfully!"
