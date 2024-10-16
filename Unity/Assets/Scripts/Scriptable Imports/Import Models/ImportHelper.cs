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

        public static async Task ImportImportableFilesIntoDatabaseAsync<T>(string rootFolder, string fileExtension, ImportFileOperation<T> importFunc, SynchronizationContext mainThreadContext) where T : IImportable
        {
            List<T> importedFiles = await ImportImportableFilesAsync<T>(rootFolder, fileExtension, mainThreadContext);
            foreach (T importedFile in importedFiles)
            {
                importFunc(importedFile);
            }
        }

        public static async Task<List<T>> ImportImportableFilesAsync<T>(string rootFolder, string fileExtension, SynchronizationContext mainThreadContext) where T : IImportable
        {
            string[] importScriptNames = Directory.GetFiles(rootFolder, $"*.{fileExtension}", SearchOption.AllDirectories);
            List<T> importedFiles = new List<T>();
            foreach (string fileName in importScriptNames)
            {
                T currentImport = await ImportImportableFileAsync<T>(fileName, mainThreadContext).ConfigureAwait(false);
                importedFiles.Add(currentImport);
            }
            return importedFiles;
        }

        public static async Task<T> ImportImportableFileAsync<T>(string filePath, SynchronizationContext mainThreadContext) where T : IImportable
        {
            try
            {
                T result = await GetFileAsync<T>(filePath).ConfigureAwait(false);
                result.FilePath = filePath;
                await result.ProcessAdditionalFilesAsync(mainThreadContext).ConfigureAwait(false);
                return result;
            }
            catch
            {
                throw;
            }
        }

        public static async Task<T> GetFileAsync<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            using StreamReader reader = new StreamReader(filePath);
            string fileText = await reader.ReadToEndAsync().ConfigureAwait(false);

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
            string fileText = await reader.ReadToEndAsync().ConfigureAwait(false);

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

        public static async Task<byte[]> ReadAllBytesAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            using StreamReader reader = new StreamReader(filePath);
            string fileText = await reader.ReadToEndAsync().ConfigureAwait(false);

            return File.ReadAllBytes(filePath);
        }

        public static async Task<Sprite> GetSpriteAsync(string filePath, int dimensionWidth, int dimensionHeight, SynchronizationContext mainThreadContext)
        {
            SynchronizationContext previousContext = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(mainThreadContext);
                byte[] imageBytes = await ImportHelper.ReadAllBytesAsync(filePath);

                Sprite createdSprite = null;
                Texture2D texture = new Texture2D(144, 80);
                texture.LoadImage(imageBytes);
                createdSprite = Sprite.Create(texture, new Rect(0, 0, dimensionWidth, dimensionHeight), Vector2.zero);

                return createdSprite;
            }
            catch
            {
                throw;
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        #region Synchronous Versions


        public static T GetFile<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            using StreamReader reader = new StreamReader(filePath);
            string fileText = reader.ReadToEnd();

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

        public static T ImportImportableFile<T>(string filePath) where T : IImportable
        {
            try
            {
                T result = GetFile<T>(filePath);
                result.FilePath = filePath;
                result.ProcessAdditionalFiles();
                return result;
            }
            catch
            {
                throw;
            }
        }

        public static Sprite GetSprite(string filePath, int dimensionWidth, int dimensionHeight)
        {
            byte[] imageBytes = ImportHelper.ReadAllBytes(filePath);

            Sprite createdSprite = null;
            Texture2D texture = new Texture2D(144, 80);
            texture.LoadImage(imageBytes);
            createdSprite = Sprite.Create(texture, new Rect(0, 0, dimensionWidth, dimensionHeight), Vector2.zero);

            return createdSprite;
        }

        public static byte[] ReadAllBytes(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            using StreamReader reader = new StreamReader(filePath);
            string fileText = reader.ReadToEnd();

            return File.ReadAllBytes(filePath);
        }
        #endregion
    }
}