using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusMaintenance;

public class GoogleDriveSync
{
    SqliteLogger logger;
    private static DriveService GetDriveService()
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

    public GoogleDriveSync()
    {
        logger = new();
    }

    // 1Sp6bAlSCZm3QWYDk3OxZ3GmnrkJcjrSM

    public async Task UploadDatabaseToDriveAsync(string dbPath)
    {
        var driveService = GetDriveService();

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string tempFile = $"NexusDB_{timestamp}.sqlite";
        string tempCopy = Path.Combine(Path.GetTempPath(), tempFile);

        File.Copy(dbPath, tempCopy, true);
        logger.InfoAsync($"Database copied to {tempCopy}");

        // Replace with your actual Google Drive "Backups" folder ID
        string backupsFolderId = "1Sp6bAlSCZm3QWYDk3OxZ3GmnrkJcjrSM";

        var fileMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = Path.GetFileName(tempCopy),
            Parents = new List<string> { backupsFolderId } // ✅ Uploads directly into Backups folder
        };

        logger.InfoAsync($"Starting Upload of {tempFile}");
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
            logger.ErrorAsync($"Warning: Could not delete temp file {tempCopy}.");
        }

        logger.InfoAsync($"✅ Uploaded {Path.GetFileName(tempCopy)} to Google Drive folder 'Nexus/Backups'.");
    }




    public async Task DownloadLatestVersionAsync(string saveToPath)
    {
        var driveService = GetDriveService();

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
