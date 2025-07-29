# FileVoyage

**FileVoyage** is a web application initially developed as part of the **Software Design (PSWE-04)** course at Cenfotec University’s Master's in Software Engineering.  
It allows users to **upload files, generate temporary download links, and obtain a QR code for easy sharing**.

---

## ✨ Features
- ✅ Upload files to **Azure Blob Storage**  
- ✅ Generate **temporary download links**  
- ✅ Configurable **download limits**  
- ✅ Automatic **QR code generation**  
- ✅ Accessible **WCAG-compliant UI** with high contrast and keyboard navigation  
- ✅ **CI/CD pipeline** with GitHub Actions and Azure App Service  

---

## 📂 Architecture

### Main Components
| Component | Description |
|-----------|-------------|
| **CosmosDbService** | Handles file metadata in CosmosDB. Uses **PartitionKey = first 2 characters of shortCode** for even distribution and efficient lookups. |
| **AzureStorageService** | Manages file upload, download, and deletion in Azure Blob Storage. |
| **HomeController** | Handles file upload, shortCode generation, metadata creation, and QR code generation. |
| **DownloadController** | Handles file downloads, expiration checks, and QR rendering. |
| **Program.cs** | Configures CosmosDB, Blob Storage, MVC services, and HTTP security headers. |

---

## 🛠 Tech Stack
- .NET 8 MVC with Razor Views  
- Azure CosmosDB (NoSQL)  
- Azure Blob Storage  
- Azure App Service  
- GitHub Actions (CI/CD)  

---

## 🚀 How to Run Locally

### 1️⃣ Clone the repository
```bash
git clone https://github.com/yourusername/FileVoyage.git
cd FileVoyage
```

### 2️⃣ Configure settings
Create an **`appsettings.json`** file with your Azure credentials:

```json
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
```

### 3️⃣ Run the application
```bash
dotnet run
```

Open the app at: `https://localhost:7198`

---

## 🧩 Key Technical Decisions
- **CosmosDB PartitionKey = first 2 characters of shortCode**  
  Ensures even data distribution across partitions and efficient lookups.  
- **Azure Blob Storage** for cost-effective and secure file storage.  
- **GitHub Actions CI/CD** for automated build, test, and deployment to **Azure App Service**.  
- **.NET 8 MVC with Razor Views** to keep the application simple, modular, and easy to maintain.

---

## 📈 Roadmap
- 🔐 Authentication and user account management  
- 📊 Admin dashboard with metrics and reporting  
- ⏳ Advanced expiration options and notifications  
- 💳 Paid plans and monetization features  

> **Note:** This repository remains public as an **academic and portfolio example**.  
> A **private commercial version** is under active development with enhanced features and improvements.

---

## 👨‍💻 Author
**Randall Vargas Li**  
🌐 [filevoyage.com](https://filevoyage.com)  
📧 contact@filevoyage.com
