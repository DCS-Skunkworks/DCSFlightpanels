using NLog;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace DCSFlightpanels
{
    internal class ProfileAutoBackup
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public string AutoBackupFolderPath  { get; init; }
        
        public ProfileAutoBackup(string autoBackupFolderPath = null)
        {
            AutoBackupFolderPath = autoBackupFolderPath;
            //if No specific folder is given, application location + default folder will be used
            if (string.IsNullOrEmpty(AutoBackupFolderPath))
            {
                var assembly = Assembly.GetExecutingAssembly();
                var assemblylocation = Path.GetDirectoryName(assembly.Location);
                AutoBackupFolderPath = Path.Combine(assemblylocation, Constants.ProfilesAutoBackupDefaultFolderName);
            }
        }

        private void CheckBackupFolder()
        {
            try
            {
                if (!Directory.Exists(AutoBackupFolderPath))
                {
                    Directory.CreateDirectory(AutoBackupFolderPath);
                    Logger.Info($"Creating Profiles AutoBackup folder [{AutoBackupFolderPath}]");
                }
            }
            catch {
                LogErrorAndThrow($"Failed to create profiles AutoBackup folder [{AutoBackupFolderPath}]");
            }
        }

        private string LogErrorAndThrow(string errorMessage)
        {
            Logger.Error(errorMessage);
            throw new Exception(errorMessage);
        }

        private string GetMD5(string filePath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        public void BackupProfile(string profileFilePath)
        {
            CheckBackupFolder();

            string backupFileName = GetProfileBackupFileName(profileFilePath);
            
            //Check if Backup already exists
            bool backupAlreadyExists = false;
            foreach (string filePath in Directory.GetFiles(AutoBackupFolderPath).ToList())
            {
                if (backupFileName == Path.GetFileName(filePath))
                    backupAlreadyExists = true;
            }

            //Creating Backup
            if (!backupAlreadyExists) {
                string archiveFilePath = Path.Combine(AutoBackupFolderPath, backupFileName);
                try
                {
                    using FileStream fs = new FileStream(archiveFilePath, FileMode.Create);
                    using ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Create);
                    archive.CreateEntryFromFile(profileFilePath, Path.GetFileName(profileFilePath));
                    Logger.Info($"AutoBackup for profile [{profileFilePath}] done in file [{archiveFilePath}]");
                }
                catch(Exception ex)
                {
                    LogErrorAndThrow($"Failed to create profile AutoBackup archive [{archiveFilePath}] [{ex.Message}]");
                }
            }
        }

        private string GetProfileBackupFileName(string profileFilePath)
        {
            string md5 = string.Empty;
            string fileName = string.Empty;
            string version = string.Empty;
            try
            {
                md5 = GetMD5(profileFilePath);
                fileName = Path.GetFileName(profileFilePath);
                Assembly assembly = Assembly.GetExecutingAssembly();
                System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
                version = fvi.FileVersion;
            }
            catch (Exception ex)
            {
                LogErrorAndThrow($"Failed to get profile AutoBackup file name [{ex.Message}]");
            }
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(md5) || string.IsNullOrEmpty(version)) {
                LogErrorAndThrow($"Incomplete AutoBackup file name [{fileName}][{md5}][{version}]");
            }
            return $"{version}_{fileName}_{md5}.zip";
        }
    }

}
