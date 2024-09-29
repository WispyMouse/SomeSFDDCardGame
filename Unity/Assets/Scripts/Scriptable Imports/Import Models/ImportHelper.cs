namespace SFDDCards
{
    using SFDDCards.ImportModels;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;

    public static class ImportHelper
    {
        public delegate void ImportFileOperation<T>(T toImport);

        public static IEnumerator YieldForTask(Task toYieldFor)
        {
            if (toYieldFor.Status == TaskStatus.Created)
            {
                toYieldFor.Start();
            }

            while (!toYieldFor.IsCompleted)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        public static async Task ImportImportableFilesIntoDatabaseAsync<T>(string rootFolder, string fileExtension, ImportFileOperation<T> importFunc) where T : IImportable
        {
            List<T> importedFiles = await ImportImportableFilesAsync<T>(rootFolder, fileExtension);
            foreach (T importedFile in importedFiles)
            {
                importFunc(importedFile);
            }
        }

        public static async Task<List<T>> ImportImportableFilesAsync<T>(string rootFolder, string fileExtension) where T : IImportable
        {
            string[] importScriptNames = Directory.GetFiles(rootFolder, $"*.{fileExtension}", SearchOption.AllDirectories);
            List<T> importedFiles = new List<T>();
            foreach (string fileName in importScriptNames)
            {
                T currentImport = await ImportImportableFileAsync<T>(fileName);
                importedFiles.Add(currentImport);
            }
            return importedFiles;
        }

        public static List<T> ImportImportableFiles<T>(string rootFolder, string fileExtension) where T : IImportable
        {
            Task<List<T>> waitOnTask = Task<List<T>>.Run(() => ImportImportableFilesAsync<T>(rootFolder, fileExtension));
            waitOnTask.Wait();
            return waitOnTask.Result;
        }

        public static async Task<T> ImportImportableFileAsync<T>(string filePath) where T : IImportable
        {
            try
            {
                T result = await GetFileAsync<T>(filePath);
                result.FilePath = filePath;
                await result.ProcessAdditionalFilesAsync();
                return result;
            }
            catch
            {
                throw;
            }
        }

        public static T ImportImportableFile<T>(string filePath) where T : IImportable
        {
            Task<T> waitOnTask = Task<T>.Run(() => ImportImportableFileAsync<T>(filePath));
            waitOnTask.Wait();
            return waitOnTask.Result;
        }

        public static async Task<T> GetFileAsync<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            using StreamReader reader = new StreamReader(filePath);
            string fileText = await reader.ReadToEndAsync();

            try
            {
                T result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(fileText);
                return result;
            }
            catch
            {
                throw;
            }
        }

        public static async Task<object> GetFileAsync(string filePath, Type toType)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            using StreamReader reader = new StreamReader(filePath);
            string fileText = await reader.ReadToEndAsync();

            try
            {
                object result = Newtonsoft.Json.JsonConvert.DeserializeObject(fileText, toType);
                return result;
            }
            catch
            {
                throw;
            }
        }

        public static T GetFile<T>(string filePath)
        {
            Task<T> waitOnTask = Task<T>.Run(() => GetFileAsync<T>(filePath));
            waitOnTask.Wait();
            return waitOnTask.Result;
        }

        public static object GetFile(string filePath, Type toType)
        {
            Task<object> waitOnTask = Task<object>.Run(() => GetFileAsync(filePath, toType));
            waitOnTask.Wait();
            return waitOnTask.Result;
        }

        public static async Task<byte[]> ReadAllBytesAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            using StreamReader reader = new StreamReader(filePath);
            string fileText = await reader.ReadToEndAsync();

            return File.ReadAllBytes(filePath);
        }

        public static async Task<Sprite> GetSpriteAsync(string filePath, int dimensionWidth, int dimensionHeight)
        {
            byte[] imageBytes = await ImportHelper.ReadAllBytesAsync(filePath);
            Texture2D texture = new Texture2D(144, 80);
            texture.LoadImage(imageBytes);
            return Sprite.Create(texture, new Rect(0, 0, dimensionWidth, dimensionHeight), Vector2.zero);
        }
    }
}