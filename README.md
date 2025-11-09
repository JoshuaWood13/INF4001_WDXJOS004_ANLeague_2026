# African Nations League

**UCT Username:** WDXJOS004

## Live Demo

**Deployment:** [https://african-nations-league-wdxjos004-fda5defucdgnhdax.southafricanorth-01.azurewebsites.net/](https://african-nations-league-wdxjos004-fda5defucdgnhdax.southafricanorth-01.azurewebsites.net/)

## Login Credentials

### Administrator Account
```
Email: admin@email.com
Password: Password123!
```

## Bonus Features Implemented
- Analytics for team performance for representatives to view
- Past performances of teams (after a reset is done)
- Past finalists and winners
- Remove a team to let another team join


## Technologies Used

- **Framework:** ASP.NET Core MVC 8.0
- **Runtime:** .NET 8.0
- **Database:** Firebase
- **Authentication:** Firebase Authentication
- **AI/Commentary:** Google Gemini AI (Gemini 2.5 Flash)
- **Styling:** Bootstrap CSS (Brite theme from Bootswatch)
- **Deployment:** Azure App Service

## How To Run Application

### Prerequisites

1. **Install .NET 8.0 SDK or later**
   - Download from [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
   - Verify installation by running `dotnet --version` in your terminal/command prompt

2. **Clone the repository**
   - Clone the project code from GitHub or extract the project `.zip` file to a folder of your choosing

### Configuration Setup

Before running the application, you need to configure the `appsettings.Development.json` file with your Firebase and AI credentials.

1. **Create `appsettings.Development.json`** in the root directory (this file is git-ignored for security)

2. **Copy the structure from `appsettings.json`** and update with your credentials:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Firebase": {
    "ProjectId": "your-firebase-project-id",
    "CredentialsPath": "Config/firebase-adminsdk.json"
  },
  "FirebaseWeb": {
    "apiKey": "your-firebase-web-api-key",
    "authDomain": "your-project-id.firebaseapp.com",
    "projectId": "your-firebase-project-id",
    "storageBucket": "your-project-id.appspot.com",
    "messagingSenderId": "your-messaging-sender-id",
    "appId": "your-firebase-app-id",
    "measurementId": "your-google-analytics-id"
  },
  "AI": {
    "Provider": "Gemini",
    "ApiKey": "your-google-gemini-api-key"
  }
}
```

3. **Set up Firebase Admin SDK credentials:**
   - Download your Firebase service account key JSON file from the Firebase Console
   - Place it in the `Config/` directory as `firebase-adminsdk.json`
   - Update the `CredentialsPath` in `appsettings.Development.json` if needed


> [!NOTE]
>Both `appsettings.Development.json` and `Config/firebase-adminsdk.json` are git-ignored for security reasons.

### Running with Visual Studio

1. **Install Visual Studio 2022 or later** with the following workloads:
   - .NET 8.0 SDK
   - ASP.NET and web development

2. **Open the project:**
   - Navigate to the project folder
   - Open the `.sln` file using Visual Studio

3. **Restore NuGet packages:**
   - Right-click the solution in Solution Explorer
   - Click **Restore NuGet Packages**
   - Verify the project includes the following NuGet packages:
     - `FirebaseAdmin` (v3.4.0)
     - `Google.Cloud.Firestore` (v3.11.0)
     - `Google_GenerativeAI` (v3.4.0)

4. **Build and run:**
   - Build the solution: **Build -> Build Solution** (or press `Ctrl+Shift+B`)
   - Run the application: **Debug -> Start Debugging** (or press `F5`)
   - The application will launch in your default browser

### Running with VS Code

1. **Install VS Code extensions:**
   - Install the **C#** extension by Microsoft
   - Install the **C# Dev Kit** extension (optional, recommended)

2. **Open the project:**
   - Open VS Code
   - Click **File -> Open Folder**
   - Navigate to and select the project folder

3. **Restore and build:**
   - Open the integrated terminal: **Terminal -> New Terminal** (or press `` Ctrl+` ``)
   - Restore NuGet packages:
     ```bash
     dotnet restore
     ```
   - Build the solution:
     ```bash
     dotnet build
     ```

4. **Run the application:**
   - Run the application:
     ```bash
     dotnet run
     ```
   - The application will start and display the URL (typically `https://localhost:7241`)
   - Open the URL in your browser

## Project Structure

```
INF4001_WDXJOS004_ANLeague_2026/
│
├── Controllers/
│   ├── AccountController.cs
│   ├── AdminController.cs
│   ├── HomeController.cs
│   ├── RepresentativeController.cs
│   └── TournamentController.cs
│
├── Models/
│   ├── Entities/
│   ├── Enums/
│   └── ViewModels/
│
├── Views/
│   ├── Account/
│   │   ├── Login.cshtml
│   │   └── SignUp.cshtml
│   ├── Home/
│   │   ├── Index.cshtml
│   │   ├── Leaderboard.cshtml
│   │   ├── MatchHighlights.cshtml
│   │   └── MatchReplay.cshtml
│   ├── Representative/
│   │   ├── CreateTeam.cshtml
│   │   └── MyTeam.cshtml
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   ├── _ValidationScriptsPartial.cshtml
│   │   └── Error.cshtml
│   └── Tournament/
│       ├── MatchDetails.cshtml
│       └── Tournament.cshtml
│
├── Services/
│   ├── AICommentary/
│   │   ├── AICommentaryService.cs
│   │   └── IAICommentaryService.cs
│   ├── Auth/
│   │   ├── AuthService.cs
│   │   └── IAuthService.cs
│   ├── Country/
│   │   ├── CountryService.cs
│   │   └── ICountryService.cs
│   ├── Email/
│   │   ├── EmailService.cs
│   │   └── IEmailService.cs
│   ├── Firebase/
│   │   ├── FirebaseService.cs
│   │   └── IFirebaseService.cs
│   ├── Match/
│   │   ├── IMatchService.cs
│   │   └── MatchService.cs
│   ├── PlayerGenerator/
│   │   ├── IPlayerGeneratorService.cs
│   │   └── PlayerGeneratorService.cs
│   └── Tournament/
│       ├── ITournamentService.cs
│       └── TournamentService.cs
│
├── Data/
│   ├── AiPrompts.cs
│   ├── CountryList.cs
│   └── PlayerNames.cs
│
├── Middleware/
│
├── wwwroot/
│   ├── css/
│   ├── js/
│   ├── lib/
│   └── images/
│
├── Program.cs
├── appsettings.json
└── INF4001_WDXJOS004_ANLeague_2026.csproj
```