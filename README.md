# filevoyage.com

FileVoyage is a web application initially developed as part of the Software Design (PSWE-04) course at Cenfotec University’s Master's in Software Engineering.
It allows users to upload files, generate temporary download links, and obtain a QR code for easy sharing.

✨ Features
✅ Upload files to Azure Blob Storage

✅ Generate temporary download links

✅ Configurable download limits

✅ Automatic QR code generation

✅ Accessible UI (WCAG) with high contrast, keyboard navigation, and alt texts

✅ Integration with CosmosDB to store file metadata

✅ CI/CD pipeline using GitHub Actions and Azure App Service for automated deployments

📂 Architecture
Main Components
Component	Description
CosmosDbService	Handles create/read/update operations for file metadata in CosmosDB. Uses a PartitionKey based on the first 2 characters of the shortCode to distribute data evenly.
AzureStorageService	Manages file upload, download, and deletion in Azure Blob Storage.
HomeController	Handles file uploads, shortCode generation, partition assignment, and QR creation.
DownloadController	Handles file downloads, expiration checks, and QR generation.
Program.cs	Configures CosmosDB, Blob Storage, MVC services, and HTTP security headers.

🛠️ Tech Stack
.NET 8 MVC with Razor Views

Azure CosmosDB (NoSQL)

Azure Blob Storage

Azure App Service

GitHub Actions (CI/CD)

📸 Screenshots
Upload Page	Download Page

🚀 How to Run Locally
1️⃣ Clone the repository
git clone https://github.com/yourusername/FileVoyage.git
cd FileVoyage

2️⃣ Configure settings
Create an appsettings.json file with your Azure configuration:

{
  "AzureStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=...",
    "ContainerName": "filevoyage-files"
  },
  "CosmosDb": {
    "ConnectionString": "AccountEndpoint=...",
    "DatabaseName": "FileVoyageDB",
    "ContainerName": "Files"
  }
}


3️⃣ Run the application

dotnet run
Access at https://localhost:7198.

🧩 Key Technical Decisions
CosmosDB with PartitionKey = first 2 characters of shortCode for even data distribution and efficient lookups.

Azure Blob Storage for cost-effective and secure file storage.

GitHub Actions CI/CD pipeline for automated build, test, and deployment to Azure App Service.

.NET 8 MVC with Razor Views for a simple, modular, and maintainable architecture.

📈 Roadmap
The application is evolving into a real commercial product with additional features:

🔐 Authentication and user account management

📊 Admin dashboard with metrics and reports

⏳ Advanced expiration settings and notifications

💳 Paid plans and monetization options

Note: This repository remains public as an academic and portfolio example.
The commercial version is under active development in a private repository with enhanced features and improvements.

👨‍💻 Author
Ing Randall Vargas Li
🌐 filevoyage.com


