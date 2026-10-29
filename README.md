Unified Service Scheduler
A .NET 8 Web API for searching dealerships, checking vehicle-service appointment availability, and booking appointments while validating service-bay and technician constraints.
Overview
The application provides three main operations:
Search dealerships using filters and pagination.
Check appointment availability for service bays and qualified technicians at a dealership for a requested service and time.
Book appointments after validating the request and confirming that the selected bay and technician can accommodate the full service duration.
The final booking validation must happen in the database as part of a transaction. An availability response is only a snapshot: another request may book the same resource before the client submits its booking request.
Technology Stack
C# and .NET 8
ASP.NET Core Web API
SQL Server and T-SQL stored procedures
NPoco for database access
Swagger/OpenAPI for API exploration
xUnit and Moq for unit tests, where configured
APT Test console application for concurrent booking tests
Solution Structure
The exact folder and project names may differ slightly depending on the checked-in solution.
```text
Unified Service Scheduler/
├── UnifiedServiceScheduler.sln
├── UnifiedServiceScheduler/
│   ├── Controllers/
│   ├── Service/
│   ├── DataAccess/
│   ├── Model/
│   ├── CustomException/
│   ├── appsettings.json
│   └── UnifiedServiceScheduler.csproj
├── UnifiesServiceSchedulerTests/
│   └── ... unit tests ...
├── APT Test/
│   └── ... console application for concurrent API requests ...
├── Database/
│   └── Dbscript/
│       └── ... SQL scripts for tables, stored procedures, and seed data ...
└── README.md
```
Keep the SQL scripts in the repository so another developer can recreate the database.
Prerequisites
Install or prepare the following:
.NET 8 SDK
SQL Server (local SQL Server, SQL Server Developer Edition, or an approved test instance)
SQL Server Management Studio (SSMS), or another SQL client that can run T-SQL scripts
Visual Studio 2022 or the .NET CLI
Git
Check the installed .NET SDKs:
```bash
dotnet --list-sdks
```
Database Setup
> **Important:** Database setup is a manual prerequisite. The application does not automatically create the tables, stored procedures, or seed records. Run the SQL scripts before starting the API.
Start SQL Server and connect to the intended SQL Server instance using SSMS.
Open and run the SQL scripts in `Database/Dbscript` in the order specified by the scripts or their comments.
Refresh the database object list and confirm that the tables and stored procedures were created.
Confirm that the seed data contains valid IDs and availability windows for the dates used by your API requests and concurrency tests.

Database Setup Checklist
[ ] The database name matches the connection string.
[ ] Tables and foreign keys were created successfully.
[ ] Stored procedures were created successfully.
[ ] Seed data was inserted successfully.
Configuration
Configure the connection string and logging in the API project's `appsettings.json` or through your local development configuration before running the API.
Connection String
After running the database scripts, replace the connection-string placeholder with a connection string for your SQL Server instance. The expected database name is:
```text
KeyloopSchedulerDemo1234567
```
Make sure the server/instance name and authentication details match your local environment. Do not commit real credentials or secrets to source control.
Logging
Replace `[LogPath]` in the logging configuration with a valid physical path on your machine. Create the log directory if required, and ensure that the account running the API has permission to write to it.
Build and Run
Run the following commands from the solution root unless stated otherwise.
1. Restore NuGet Packages
```bash
dotnet restore UnifiedServiceScheduler.sln
```
2. Build the Solution
```bash
dotnet build UnifiedServiceScheduler.sln
```
Resolve any build errors before proceeding.
3. Run the API
```bash
dotnet run --project UnifiedServiceScheduler/UnifiedServiceScheduler.csproj
```
Use the actual `.csproj` path if the project folder name differs. Check the console output for the HTTP/HTTPS URLs.
If HTTPS development certificates are required, trust or configure the .NET development certificate for your environment.
Open Swagger UI using the HTTPS URL printed by the application. The URL commonly follows this pattern:
```text
https://localhost:<HTTPS_PORT>/swagger
```
Replace `<HTTPS_PORT>` with the port shown in your console output. Swagger can be used to try the endpoints and inspect their request and response schemas.
API Endpoints
The current controller route is `api/UnifiedServiceScheduler`.
Operation	Method and route	Purpose
Search dealerships	`POST /api/UnifiedServiceScheduler/getDealerDetails`	Searches dealerships using filters and pagination.
Check availability	`GET /api/UnifiedServiceScheduler/getAvailableSlots?dealerId=...`	Returns eligible bays and technicians. Availability depends on the supplied service and time parameters.
Book appointment	`POST /api/UnifiedServiceScheduler/bookAppointment`	Attempts to create an appointment and returns the generated appointment ID when successful.
Use the request models in Swagger or the source code to confirm the required parameters and request bodies.
Testing
Unit Tests
Run the unit-test project from Visual Studio Test Explorer, or use the .NET CLI with the actual test-project path:
```bash
dotnet test
```
This command runs the tests discovered in the solution. Confirm the output to determine which tests passed or failed.
API Testing
Start the API.
Open Swagger UI.
Submit requests to the dealership-search, availability, and booking endpoints.
Verify the HTTP responses and, where appropriate, confirm the resulting records in SQL Server.
Use valid IDs and time windows from the seeded database.
*** For Booking appointement testion please use Console app from APITest project
Concurrency Testing with APT Test
The solution includes a separate console project named APT Test. It sends multiple booking requests concurrently to the running API to test conflicting booking attempts.
Before Running the Test
Run the SQL setup and seed scripts.
Ensure the API is running and its connection string points to the test database.
Choose a valid user, vehicle, dealership, bay, technician, service, and start time from the seed data.
Confirm that the selected bay and technician are initially available for the entire service duration.
Update the API base URL and request payload in APT Test to match the running API and seeded data.
Run the APT Test project from Visual Studio or with the .NET CLI, using the actual project path.
Expected Outcome
When every request targets the same bay and technician for the same overlapping time interval, the expected result is:
Exactly one request successfully creates the appointment.
The other conflicting requests are rejected because the resource is no longer available.
The database contains no duplicate, overlapping, non-cancelled appointments for the same bay or technician.
Run this test against a dedicated test database. Each successful request can create a real appointment, so reset or prepare the test data before repeating the test.

AI Coloboration

After selecting the service-scheduling scenario, I used an initial UI prototype to understand the user journey and identify the operations the backend needed to support. I then used AI to help translate that flow into API responsibilities and request/response models.The resulting API design focused on three main operations: dealership
search, availability checking, and appointment booking. I reviewed the proposed endpoints and adapted them to the controller and model structure used in the application.


