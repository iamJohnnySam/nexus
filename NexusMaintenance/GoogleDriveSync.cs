using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NexusMaintenance;

public class GoogleDriveSync
{
    SqliteLogger logger;
    private static DriveService GetDriveService_local()
    {
        string[] Scopes = { DriveService.Scope.DriveFile };
        string ApplicationName = "Nexus Project App";

        UserCredential credential;
        using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
        {
            string credPath = "token.json";
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true)).Result;
        }

        return new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });
    }


    public static DriveService GetDriveService()
    {
        string credentialsPath = "service-account.json";

        GoogleCredential credential;
        using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream)
                .CreateScoped(DriveService.Scope.DriveFile);
        }

        // Initialize Drive API service
        var service = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "NexusBackupService",
        });

        return service;
    }

    public GoogleDriveSync()
    {
        logger = new();
    }

    // Folder: 1Sp6bAlSCZm3QWYDk3OxZ3GmnrkJcjrSM

    public async Task UploadDatabaseToDriveAsync_OAuth(string dbPath)
    {
        DriveService driveService = GetDriveService_local();


        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string tempFile = $"NexusDB_{timestamp}.sqlite";
        string tempCopy = Path.Combine(Path.GetTempPath(), tempFile);

        File.Copy(dbPath, tempCopy, true);
        logger.Info($"Database copied to {tempCopy}");

        // Replace with your actual Google Drive "Backups" folder ID
        string backupsFolderId = "1Sp6bAlSCZm3QWYDk3OxZ3GmnrkJcjrSM";

        var fileMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = Path.GetFileName(tempCopy),
            Parents = new List<string> { backupsFolderId } // ✅ Uploads directly into Backups folder
        };

        logger.Info($"Starting Upload of {tempFile}");
        using (var stream = new FileStream(tempCopy, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            var request = driveService.Files.Create(fileMetadata, stream, "application/octet-stream");
            request.Fields = "id, parents";
            await request.UploadAsync();
        }

        await Task.Delay(100);
        try
        {
            File.Delete(tempCopy);
        }
        catch (IOException)
        {
            logger.Error($"Warning: Could not delete temp file {tempCopy}.");
        }

        logger.Info($"✅ Uploaded {Path.GetFileName(tempCopy)} to Google Drive folder 'Nexus/Backups'.");
    }

    public static async Task UploadDatabaseToDriveAsync(string filePath)
    {
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"NexusDB_{DateTime.Now:yyyyMMdd_HHmmss}.sqlite");
        File.Copy(filePath, tempFilePath, overwrite: true);

        var service = GetDriveService();

        var folderName = "nexus/backups";
        string folderId = await GetOrCreateFolderAsync(service, folderName);

        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = Path.GetFileName(tempFilePath),
            Parents = new List<string> { folderId }
        };

        using (var stream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
        {
            var request = service.Files.Create(fileMetadata, stream, "application/x-sqlite3");
            request.Fields = "id";
            await request.UploadAsync();
        }

        File.Delete(tempFilePath);
        Console.WriteLine($"Uploaded: {Path.GetFileName(tempFilePath)}");
    }


    private static async Task<string> GetOrCreateFolderAsync(DriveService service, string folderName)
    {
        // Split nested folders (e.g., Nexus/backups)
        string[] parts = folderName.Split('/');
        string? parentId = null;

        foreach (string name in parts)
        {
            var listRequest = service.Files.List();
            listRequest.Q = $"mimeType='application/vnd.google-apps.folder' and name='{name}'" +
                            (parentId == null ? "" : $" and '{parentId}' in parents");
            listRequest.Fields = "files(id, name)";
            var list = await listRequest.ExecuteAsync();

            var folder = list.Files.FirstOrDefault();
            if (folder == null)
            {
                // Create folder
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = name,
                    MimeType = "application/vnd.google-apps.folder",
                    Parents = parentId == null ? null : new List<string> { parentId }
                };

                var createRequest = service.Files.Create(fileMetadata);
                createRequest.Fields = "id";
                var created = await createRequest.ExecuteAsync();
                parentId = created.Id;
            }
            else
            {
                parentId = folder.Id;
            }
        }

        return parentId!;
    }





    public async Task DownloadLatestVersionAsync(string saveToPath)
    {
        DriveService driveService;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            driveService = GetDriveService();
        }
        else
        {
            driveService = GetDriveService_local();
        }

        var listRequest = driveService.Files.List();
        listRequest.Q = "name contains 'NexusDB_' and mimeType='application/octet-stream'";
        listRequest.Spaces = "drive";
        listRequest.Fields = "files(id, name, createdTime)";
        var files = (await listRequest.ExecuteAsync()).Files;

        var latestFile = files.OrderByDescending(f => f.CreatedTimeDateTimeOffset).FirstOrDefault();
        if (latestFile == null) return;

        var request = driveService.Files.Get(latestFile.Id);
        using var stream = new FileStream(saveToPath, FileMode.Create, FileAccess.Write);
        await request.DownloadAsync(stream);
    }


}
