using ApplicationCore.Common;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Requests;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Web.Interfaces;

namespace Web.Adapters
{
    public class DriveServiceAdapter : IDriveServiceAdapter
    {
        private const string DriveFolderMimeType = "application/vnd.google-apps.folder";
        private const string PermissionType = "user";
        private const string PermissionRole = "writer";

        private readonly ILogger<DriveServiceAdapter> _logger;
        private readonly DriveService _driveService;

        public DriveServiceAdapter(ILogger<DriveServiceAdapter> logger, DriveService driveService)
        {
            _logger = logger;
            _driveService = driveService;
        }

        public async Task<string> CreateFolderAsync(string name)
        {
            Contract.Require(!string.IsNullOrWhiteSpace(name), "Folder name is required for creation.");

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = name,
                MimeType = DriveFolderMimeType
            };

            try
            {
                var createRequest = _driveService.Files.Create(fileMetadata);
                createRequest.Fields = "id";
                Google.Apis.Drive.v3.Data.File file = await createRequest.ExecuteAsync();
                return file.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return string.Empty;
        }

        public async Task<string> GetFolderIdByNameAsync(string name)
        {
            Contract.Require(!string.IsNullOrWhiteSpace(name), "Folder name is required when retrieving id.");

            try
            {
                var listRequest = _driveService.Files.List();
                listRequest.Q = $"mimeType='{DriveFolderMimeType}' and name='{name}'";
                var fileList = await listRequest.ExecuteAsync();
                if (fileList.Files.Count > 0)
                {
                    return fileList.Files[0].Id;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return string.Empty;
        }

        public async Task<string> UploadFileToFolder(string filePath, string folderId, string mimeType)
        {
            Contract.Require(!string.IsNullOrWhiteSpace(filePath), "Cannot upload empty file path.");
            Contract.Require(!string.IsNullOrWhiteSpace(folderId), "Folder id is required for file upload.");
            Contract.Require(!string.IsNullOrWhiteSpace(mimeType), "Mime type is required for file upload.");

            try
            {
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = Path.GetFileName(filePath),
                    Parents = new List<string> { folderId }
                };

                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    var uploadRequest = _driveService.Files.Create(fileMetadata, stream, mimeType);
                    uploadRequest.Fields = "id";
                    await uploadRequest.UploadAsync();
                    Google.Apis.Drive.v3.Data.File file = uploadRequest.ResponseBody;
                    return file.Id;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return string.Empty;
        }

        public async Task<bool> AlreadyShared(string fileId)
        {
            Contract.Require(!string.IsNullOrWhiteSpace(fileId), "File id is required when checking if file is shared.");

            try
            {
                var list = await _driveService.Permissions.List(fileId).ExecuteAsync();
                foreach (var permission in list.Permissions)
                {
                    if (permission.Type == PermissionType && permission.Role == PermissionRole)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return false;
        }

        public async Task<string> ShareFile(string fileId, string email)
        {
            Contract.Require(!string.IsNullOrWhiteSpace(fileId), "File id is required when sharing a file.");
            Contract.Require(!string.IsNullOrWhiteSpace(email), "Email is required when sharing a file.");

            try
            {
                var userPermission = new Permission()
                {
                    Type = PermissionType,
                    Role = PermissionRole,
                    EmailAddress = email
                };

                var createRequest = _driveService.Permissions.Create(userPermission, fileId);
                createRequest.Fields = "id";
                Permission permission = await createRequest.ExecuteAsync();
                return permission.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return string.Empty;
        }

        public async Task DeleteAllFiles()
        {
            try
            {
                var files = _driveService.Files.List().Execute();
                if (files.Files.Count > 0)
                {
                    var batch = new BatchRequest(_driveService);
                    BatchRequest.OnResponse<string> callback = delegate (string id, RequestError error, int index, HttpResponseMessage message)
                    {
                        if (error != null)
                        {
                            _logger.LogError(error.Message);
                        }
                    };

                    foreach (var file in files.Files)
                    {
                        var deleteRequest = _driveService.Files.Delete(file.Id);
                        batch.Queue(deleteRequest, callback);
                    }

                    await batch.ExecuteAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}
