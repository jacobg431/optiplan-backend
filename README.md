# Optiplan
Welcome to Optiplan — a .NET-based application designed to optimize the scheduling of industrial maintenance work orders.
This repository contains the full source code, unit tests, and supporting tools to build and run the project locally.


## 📋 Prerequisites

Before you get started, make sure you have the following installed on your system:

.NET SDK 8.0.407
Download from: https://dotnet.microsoft.com/en-us/download/dotnet/8.0

SQLite3 (version 3.49.1 or higher recommended)
Download from: https://sqlite.org/download.html


## 🚀 Setting up the Project

To set up the project environment, initialize the database, restore dependencies, and run unit tests, use the setup.sh script.

Run setup script:
```
./setup.sh
```

This script will:
- Verify required versions of .NET SDK and SQLite3 are installed.
- Restore all project dependencies.
- Build the solution.
- Initialize the SQLite database from OptiplanSqlite3.sql.
- Generate and trust a development HTTPS certificate.
- Run unit tests to ensure everything is working correctly.

If any step fails, the script will stop with a clear message.


## ▶️ Running the Application

Once setup is complete, you can run the application using the run.sh script.

Run application script:
```
./run.sh
```

This script will:
- Restore dependencies.
- Build the solution.
- Run unit tests.
- Start the Optiplan.WebApi project using the https launch profile.
- When successful, the application will be running locally and accessible via HTTPS.


## 🛠️ Project Structure

Optiplan/
├── Optiplan.WebApi/        # Main Web API project
├── Optiplan.UnitTests/     # Unit tests for the project
├── OptiplanSqlite3.sql     # SQL script to initialize the SQLite database
├── Optiplan.db             # SQLite database (created during setup)
├── setup.sh                # Project setup script
├── run.sh                  # Application run script
└── README.md               # This file


## 📑 Notes

Make sure both setup.sh and run.sh have executable permissions:
```
chmod +x setup.sh run.sh
```

The HTTPS certificate is generated and trusted for development use only.
