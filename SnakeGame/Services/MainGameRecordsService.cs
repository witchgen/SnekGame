using Microsoft.Maui.Storage;
using SnakeGame.Abstractions;
using SnakeGame.SnekEngine.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SnakeGame.Services
{
    internal class MainGameRecordsService : ISnakeRecordsService
    {
        // Название файла с рекордами
        private static readonly string _recordsFileName = "mainGameRecords";
        public ObservableCollection<PlayInfo> Records { get; } = [];
        private readonly SemaphoreSlim _fileLock = new(1, 1);
        private int _nextId = 1; // record id

        public async Task AddNewRecordAsync(PlayInfo data)
        {
            await _fileLock.WaitAsync();
            try
            {
                data.Id = _nextId++;
                data.CurrentState = null;

                var index = 0;
                while (index < Records.Count &&
                       (Records[index].FinalScore > data.FinalScore))
                {
                    index++;
                }

                Records.Insert(index, data);

                for (int i = 0; i < Records.Count; i++)
                {
                    Records[i].Rank = i + 1;
                }

                await SaveRecordsToFileAsync();
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<ObservableCollection<PlayInfo>> GetRecordsAsync()
        {
            if (Records.Count == 0)
                await LoadIfNeededAsync();

            return Records;
        }

        private async Task LoadIfNeededAsync()
        {
            await _fileLock.WaitAsync();
            try
            {
                // Если файла еще нет - таблица рекордов пустая

                if (Records.Count > 0)
                    return;

                var folder = Path.Combine(FileSystem.Current.AppDataDirectory, "PlayerRecords");
                Directory.CreateDirectory(folder);
                var filePath = Path.Combine(folder, $"{_recordsFileName}.json");

                if (!File.Exists(filePath))
                {
                    _nextId = 1;
                    return;
                }

                var data = await File.ReadAllTextAsync(filePath);
                if (string.IsNullOrWhiteSpace(data))
                {
                    _nextId = 1;
                    return;
                }

                var loaded = JsonSerializer.Deserialize<List<PlayInfo>>(data) ?? [];
                Records.Clear();
                foreach (var r in loaded)
                    Records.Add(r);

                _nextId = Records.Count == 0 ? 1
                                             : Records.Max(r => r.Id) + 1;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task SaveRecordsToFileAsync()
        {
            var fileContent = JsonSerializer.Serialize(Records.ToList());

            try
            {
                var folder = Path.Combine(FileSystem.Current.AppDataDirectory, "PlayerRecords");
                Directory.CreateDirectory(folder);
                var filePath = Path.Combine(folder, $"{_recordsFileName}.json");

                await File.WriteAllTextAsync(filePath, fileContent);
            }
            catch(Exception ex)
            {

            }
        }
    }
}
