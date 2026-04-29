using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Maui.Storage;
using static SnakeGame.GameInfo.GameState;
using SnakeGame.GameInfo;

namespace SnakeGame.Services;

/// <summary>
/// Сервис обработки игровых рекордов.
/// Включает в себя: чтение-запись из файла и хранение текущих рекордов
/// </summary>
public interface IRecordsService
{
    /// <summary>
    /// Вносим новый рекорд по итогам игры (включает в себя запись во внутренний JSON файл)
    /// </summary>
    /// <param name="data">Данные сеанса</param>
    /// <returns></returns>
    Task AddNewRecordAsync(PlayData data);
    /// <summary>
    /// Получить имеющиеся записи
    /// </summary>
    /// <returns></returns>
    ObservableCollection<PlayData> GetRecords();
    /// <summary>
    /// Внешний метод для записи в файл
    /// </summary>
    /// <returns></returns>
    Task SaveToFileAsync();
    /// <summary>
    /// Внешний метод для инициализации подгрузки из файла
    /// </summary>
    /// <returns></returns>
    Task LoadFromFile();
}

public partial class RecordsService : IRecordsService
{
    //private List<GameDataModel> _records = new List<GameDataModel>();
    // Единственная коллекция, на которую подписаны все VM
    public ObservableCollection<PlayData> Records { get; } = new();

    // НАЗВАНИЕ ФАЙЛОВ С РЕКОРДАМИ
    private static readonly string _recordsFileName = "records";

    private readonly object _fileLock = new();

    public async Task AddNewRecordAsync(PlayData data)
    {
        var index = 0;
        while (index < Records.Count &&
               (/*Records[index].DtSnapshot > data.DtSnapshot ||*/
               Records[index].Score > data.Score))
        {
            index++;
        }

        Records.Insert(index, data);

        for (int i = 0; i < Records.Count; i++)
        {
            Records[i].Rank = i + 1;
        }

        await SaveToFileAsync();
    }

    public ObservableCollection<PlayData> GetRecords()
    {
        return Records;
    }

    public async Task LoadFromFile()
    {
        var folder = Path.Combine(FileSystem.Current.AppDataDirectory, "PlayerRecords");
        if(!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        var filePath = Path.Combine(folder, $"{_recordsFileName}.json");

        // Проверяем существование файла
        if (!File.Exists(filePath))
        {
            return;
        }

        // Читаем из файла
        var data = await File.ReadAllTextAsync(filePath);

        if (string.IsNullOrEmpty(data))
        {
            return;
        }

        var loaded = JsonSerializer.Deserialize<List<PlayData>>(data);
        if (loaded == null) return;

        Records.Clear();

        foreach (var record in loaded)
        {
            Records.Add(record); // Добавляем в существующую коллекцию
        }
    }

    // Сериализуем и сохраняем
    public async Task SaveToFileAsync()
    {
        var fileContent = JsonSerializer.Serialize(Records);

        lock(_fileLock)
        {
            var folder = Path.Combine(FileSystem.Current.AppDataDirectory, "PlayerRecords");
            Directory.CreateDirectory(folder);
            var filePath = Path.Combine(folder, "records.json");

            File.WriteAllTextAsync(filePath, fileContent);
        }
    }
}
