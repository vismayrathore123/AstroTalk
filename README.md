# AstroDeepak - MAUI Kundli Management

AstroDeepak is a cross-platform .NET MAUI application designed for managing astrological birth chart (Kundli) records. It allows users to add, search, edit, and select specific planetary positions (Navgrah) for individuals.

## 🏗️ Architecture

This project follows **Clean Architecture** principles to ensure separation of concerns, maintainability, and testability.

- **Domain Layer**: Contains core business entities (`Person`) and repository interfaces (`IPersonRepository`). *Has no external dependencies.*
- **Application Layer**: Contains Data Transfer Objects (`DTOs`), service interfaces (`IPersonService`), and business logic implementations (`PersonService`). *Depends only on Domain.*
- **Infrastructure Layer**: Handles data persistence using SQLite. Contains database entities (`PersonEntity`), the database context (`SqliteDbContext`), and repository implementations (`PersonRepository`). *Depends on Domain.*
- **Presentation Layer (MAUI)**: The user interface (XAML and Code-behind). *Depends on Application and Infrastructure via Dependency Injection.*

## ✨ Features

- **📋 List Dashboard**: View all saved Kundli records sorted by newest first.
- **🔍 Search**: Real-time filtering of records by Name, Father's Name, or Gotra.
- **➕ Add New Kundli**: Create a new astrological profile.
- **✏️ Edit Existing**: Tap "Open" on any record to modify its details.
- **🪐 Navgrah Selection**: Select exactly **one** planet (Surya, Chandra, Mangal, Budha, Guru, Shukra, Shani, Rahu, Ketu) for the individual.
- **🗄️ Local Database**: Uses SQLite for local storage with automatic table creation.

## 🛠️ Prerequisites

To build and run this project, you need:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download) or later.
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (v17.3+) with the **.NET MAUI** workload installed.
- Alternatively, [Visual Studio Code](https://code.visualstudio.com/) with the [MAUI Extensions](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-maui).

## 🚀 Getting Started

Follow these instructions to get a copy of the project up and running on your local machine.

### 1. Clone the repository
```bash
git clone https://github.com/your-username/AstroDeepak.git
cd AstroDeepak