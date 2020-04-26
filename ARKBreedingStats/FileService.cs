﻿using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

namespace ARKBreedingStats
{

    public static class FileService
    {
        private const string jsonFolder = "json";

        public const string ValuesFolder = "values";
        public const string ValuesJson = "values.json";
        public const string ValuesServerMultipliers = "serverMultipliers.json";
        public const string TamingFoodData = "tamingFoodData.json";
        public const string ModsManifest = "_manifest.json";
        public const string KibblesJson = "kibbles.json";
        public const string AliasesJson = "aliases.json";
        public const string ArkDataJson = "ark_data.json";
        public const string IgnoreSpeciesClasses = "ignoreSpeciesClasses.json";
        public const string CustomReplacingsNamePattern = "customReplacings.json";

        public static readonly string ExeFilePath = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
        public static readonly string ExeLocation = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

        /// <summary>
        /// Returns a <see cref="FileStream"/> of a file located in the json data folder
        /// </summary>
        /// <param name="fileName">name of file to read; use FileService constants</param>
        /// <returns></returns>
        public static FileStream GetJsonFileStream(string fileName)
        {
            return File.OpenRead(GetJsonPath(fileName));
        }

        /// <summary>
        /// Returns a <see cref="StreamReader"/> of a file located in the json data folder
        /// </summary>
        /// <param name="fileName">name of file to read; use FileService constants</param>
        /// <returns></returns>
        public static StreamReader GetJsonFileReader(string fileName)
        {
            return File.OpenText(GetJsonPath(fileName));
        }

        /// <summary>
        /// Gets the full path for the given filename or the path to the application data folder
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetPath(string fileName = null)
        {
            return Path.Combine(Updater.IsProgramInstalled ? getLocalApplicationDataPath() : ExeLocation, fileName ?? string.Empty);
        }

        /// <summary>
        /// Gets the full path for the given filename or the path to the json folder
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetJsonPath(string fileName = null, string fileName2 = null)
        {
            return Path.Combine(Updater.IsProgramInstalled ? getLocalApplicationDataPath() : ExeLocation, jsonFolder, fileName ?? string.Empty, fileName2 ?? string.Empty);
        }

        private static string getLocalApplicationDataPath()
        {
            return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), // C:\Users\xxx\AppData\Local\
                    Path.GetFileNameWithoutExtension(ExeFilePath) ?? "ARK Smart Breeding"); // ARK Smart Breeding;
        }

        /// <summary>
        /// Saves an object to a json-file.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="filePath">filePath</param>
        public static bool SaveJSONFile(string filePath, object data, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                using (StreamWriter sw = File.CreateText(filePath))
                {
                    var ser = new Newtonsoft.Json.JsonSerializer();
                    ser.Serialize(sw, data);
                }
                return true;
            }
            catch (SerializationException ex)
            {
                errorMessage = $"File\n{Path.GetFullPath(filePath)}\ncouldn't be saved.\nErrormessage:\n\n" + ex.Message;
            }
            return false;
        }

        /// <summary>
        /// Loads a serialized object from a json-file.
        /// </summary>
        /// <param name="filePath">filePath</param>
        /// <param name="data"></param>
        public static bool LoadJSONFile<T>(string filePath, out T data, out string errorMessage) where T : class
        {
            errorMessage = null;
            data = null;
            if (!File.Exists(filePath))
                return false;

            // load json-file of data
            try
            {
                using (StreamReader sr = File.OpenText(filePath))
                {
                    var ser = new Newtonsoft.Json.JsonSerializer();
                    data = (T)ser.Deserialize(sr, typeof(T));
                    if (data != null)
                        return true;

                    errorMessage = $"File\n{Path.GetFullPath(filePath)}\n contains no readable data.";
                    return false;
                }
            }
            catch (Newtonsoft.Json.JsonReaderException ex)
            {
                errorMessage = $"File\n{Path.GetFullPath(filePath)}\ncouldn't be opened or read.\nErrormessage:\n\n" + ex.Message;
            }
            catch (Newtonsoft.Json.JsonSerializationException ex)
            {
                errorMessage = $"File\n{Path.GetFullPath(filePath)}\ncouldn't be opened or read.\nErrormessage:\n\n" + ex.Message;
            }
            return false;
        }

        /// <summary>
        /// Tries to create a directory if not existing. Returns true if the path exists.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool TryCreateDirectory(string path, out string error)
        {
            error = null;
            if (Directory.Exists(path)) return true;

            try
            {
                Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            return false;
        }

        /// <summary>
        /// Tries to delete a file, doesn't throw an exception.
        /// </summary>
        /// <param name="filePath"></param>
        public static bool TryDeleteFile(string filePath)
        {
            if (!File.Exists(filePath)) return false;
            try
            {
                File.Delete(filePath);
                return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Tries to move a file, doesn't throw an exception.
        /// </summary>
        /// <param name="filePath"></param>
        public static bool TryMoveFile(string filePathFrom, string filePathTo)
        {
            if (!File.Exists(filePathFrom)) return false;
            try
            {
                File.Move(filePathFrom, filePathTo);
                return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Tests if a folder is protected and needs admin privileges to copy files over.
        /// This is used for the updater.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns>Returns true if elevated privileges are needed.</returns>
        public static bool TestIfFolderIsProtected(string folderPath)
        {
            try
            {
                string testFilePath = Path.Combine(folderPath, "testFile.txt");
                File.WriteAllText(testFilePath, string.Empty);
                TryDeleteFile(testFilePath);
            }
            catch (UnauthorizedAccessException)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if a file is a valid json file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        internal static bool IsValidJsonFile(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            string fileContent = File.ReadAllText(filePath);
            // currently very basic test, could be improved
            return fileContent.StartsWith("{") && fileContent.EndsWith("}");

            //try
            //{
            //    Newtonsoft.Json.Linq.JObject.Parse(fileContent);
            //    return true;
            //}
            //catch { return false; }
        }
    }
}
