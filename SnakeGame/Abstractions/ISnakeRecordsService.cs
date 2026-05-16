using SnakeGame.SnekEngine.Abstractions.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SnakeGame.Abstractions
{
    public interface ISnakeRecordsService
    {
        /// <summary>
        /// Запись нового рекорда
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task AddNewRecordAsync(PlayInfo data);
        /// <summary>
        /// Получение рекордов (если пустая коллекция, пробуем загрузить сохраненные)
        /// </summary>
        /// <returns></returns>
        Task<ObservableCollection<PlayInfo>> GetRecordsAsync();
        /// <summary>
        /// Сохранить имеющиеся рекорды в файл
        /// </summary>
        /// <returns></returns>
        Task SaveRecordsToFileAsync();
    }
}
