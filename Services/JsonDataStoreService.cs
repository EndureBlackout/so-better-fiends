using MelonLoader;
using Newtonsoft.Json;

namespace ExampleMod.Services
{
    public class JsonDataStoreService<T> where T : class
    {
        private readonly string filePath;
        private List<T> items;

        public JsonDataStoreService(string filePath)
        {
            this.filePath = filePath;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    items = JsonConvert.DeserializeObject<List<T>>(json);
                }
                else
                {
                    items = new List<T>();
                    SaveData();
                }
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Error loading data from {filePath}: {e.Message}");
                items = new List<T>();
            }
        }

        public void SaveData()
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                string json = JsonConvert.SerializeObject(items, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Failed to save data: {ex.Message}");
            }
        }

        public void Add(T item)
        {
            items.Add(item);
            SaveData();
        }

        public void AddRange(IEnumerable<T> newItems)
        {
            items.AddRange(newItems);
            SaveData();
        }

        public bool Remove(T item)
        {
            bool result = items.Remove(item);
            if (result) SaveData();
            return result;
        }

        public void Clear()
        {
            items.Clear();
            SaveData();
        }

        public List<T> GetAll()
        {
            return new List<T>(items);
        }

        public T FindFirst(Func<T, bool> predicate)
        {
            return items.FirstOrDefault(predicate);
        }

        public List<T> FindAll(Func<T, bool> predicate)
        {
            return items.Where(predicate).ToList();
        }

        public void Update(Func<T, bool> predicate, Action<T> updateAction)
        {
            foreach (var item in items.Where(predicate))
            {
                updateAction(item);
            }
            SaveData();
        }
    }
}
