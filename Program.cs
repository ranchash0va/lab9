namespace lab9
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Runtime.Serialization.Json;
    using System.Linq;

    [Serializable]
    class Item
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
    }

    class DataHandler<T>
    {
        public void SaveToBinaryFile(T data, string fileName)
        {
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(fs, data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении в бинарном файле: {ex.Message}");
            }
        }

        public T LoadFromBinaryFile(string fileName)
        {
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    return (T)formatter.Deserialize(fs);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке из бинарного файла: {ex.Message}");
                return default(T);
            }
        }

        public void SaveToJsonFile(T data, string fileName)
        {
            try
            {
                DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings
                {
                    UseSimpleDictionaryFormat = true,
                    EmitTypeInformation = System.Runtime.Serialization.EmitTypeInformation.Never
                };

                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(T), settings);

                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    jsonSerializer.WriteObject(fs, data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении в JSON файле: {ex.Message}");
            }
        }

        public T LoadFromJsonFile(string fileName)
        {
            try
            {
                DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings
                {
                    UseSimpleDictionaryFormat = true,
                    EmitTypeInformation = System.Runtime.Serialization.EmitTypeInformation.Never
                };

                using (FileStream fs = new FileStream(fileName, FileMode.Open))
                {
                    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(T), settings);
                    return (T)jsonSerializer.ReadObject(fs);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке из JSON файла: {ex.Message}");
                return default(T);
            }
        }
    }

    class Program
    {
        static Item[] items;

        static void Main()
        {
            DataHandler<Item[]> dataHandler = new DataHandler<Item[]>();

            if (File.Exists("binary_data.dat"))
            {
                items = dataHandler.LoadFromBinaryFile("binary_data.dat");
                if (items == null)
                {
                    Console.WriteLine("Ошибка при загрузке данных из бинарного файла. Создан новый массив.");
                    items = new Item[0];
                }
            }
            else
            {
                Console.WriteLine("Бинарный файл не существует. Создан новый массив.");
                items = new Item[0];
            }

            while (true)
            {
                Console.WriteLine("\nВыберите действие:");
                Console.WriteLine("1. Добавить элемент");
                Console.WriteLine("2. Удалить элемент");
                Console.WriteLine("3. Просмотр инвентаря");
                Console.WriteLine("4. Сортировать инвентарь");
                Console.WriteLine("5. Фильтровать инвентарь");
                Console.WriteLine("6. Закрыть");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        AddItem();
                        break;
                    case "2":
                        RemoveItem();
                        break;
                    case "3":
                        ViewInventory();
                        break;
                    case "4":
                        SortInventory();
                        break;
                    case "5":
                        FilterInventory();
                        break;
                    case "6":
                        dataHandler.SaveToBinaryFile(items, "binary_data.dat");
                        dataHandler.SaveToJsonFile(items, "json_data.json");
                        return;
                    default:
                        Console.WriteLine("Некорректный выбор. Пожалуйста, выберите от 1 до 6.");
                        break;
                }
            }
        }

        static void AddItem()
        {
            Console.WriteLine("Введите данные для нового предмета:");
            Console.Write("Имя предмета: ");
            string name = Console.ReadLine();

            Console.Write("Количество: ");
            int quantity;
            while (!int.TryParse(Console.ReadLine(), out quantity) || quantity <= 0)
            {
                Console.WriteLine("Введите корректное положительное число.");
                Console.Write("Количество: ");
            }

            int existingItemIndex = Array.FindIndex(items, item => item?.Name == name);
            if (existingItemIndex != -1)
            {
                items[existingItemIndex].Quantity += quantity;
            }
            else
            {
                Item[] newItems = new Item[items.Length + 1];
                for (int i = 0; i < items.Length; i++)
                {
                    newItems[i] = items[i];
                }
                newItems[newItems.Length - 1] = new Item { Name = name, Quantity = quantity };
                items = newItems;
            }

            SaveData();
        }

        static void RemoveItem()
        {
            if (items.Length == 0)
            {
                Console.WriteLine("Нет элементов для удаления.");
                return;
            }

            Console.WriteLine("Выберите номер элемента для удаления:");

            for (int i = 0; i < items.Length; i++)
            {
                Console.WriteLine($"{i + 1}. Name: {items[i].Name}, Quantity: {items[i].Quantity}");
            }

            int indexToRemove;
            while (!int.TryParse(Console.ReadLine(), out indexToRemove) || indexToRemove < 1 || indexToRemove > items.Length)
            {
                Console.WriteLine("Введите корректный номер элемента.");
                Console.Write("Введите номер элемента: ");
            }

            Console.Write($"Введите количество элементов для удаления (от 1 до {items[indexToRemove - 1].Quantity}): ");
            int quantityToRemove;
            while (!int.TryParse(Console.ReadLine(), out quantityToRemove) || quantityToRemove < 1 || quantityToRemove > items[indexToRemove - 1].Quantity)
            {
                Console.WriteLine($"Введите корректное число от 1 до {items[indexToRemove - 1].Quantity}.");
                Console.Write("Введите количество элементов для удаления: ");
            }

            items[indexToRemove - 1].Quantity -= quantityToRemove;

            if (items[indexToRemove - 1].Quantity == 0)
            {
                Item[] newItems = new Item[items.Length - 1];
                for (int i = 0; i < indexToRemove - 1; i++)
                {
                    newItems[i] = items[i];
                }
                for (int i = indexToRemove; i < items.Length; i++)
                {
                    newItems[i - 1] = items[i];
                }
                items = newItems;
            }

            SaveData();
        }

        static void ViewInventory()
        {
            if (items.Length == 0)
            {
                Console.WriteLine("Инвентарь пуст.");
            }
            else
            {
                Console.WriteLine("Информация о предметах в инвентаре:");
                for (int i = 0; i < items.Length; i++)
                {
                    Console.WriteLine($"{i + 1}. Name: {items[i].Name}, Quantity: {items[i].Quantity}");
                }
            }
        }

        static void SaveData()
        {
            DataHandler<Item[]> dataHandler = new DataHandler<Item[]>();
            dataHandler.SaveToBinaryFile(items, "binary_data.dat");
            dataHandler.SaveToJsonFile(items, "json_data.json");

            Console.WriteLine("Данные успешно сохранены.");
        }

        static void SortInventory()
        {
            Console.WriteLine("Выберите поле для сортировки:");
            Console.WriteLine("1. По имени");
            Console.WriteLine("2. По количеству");
            string sortChoice = Console.ReadLine();

            switch (sortChoice)
            {
                case "1":
                    Array.Sort(items, (item1, item2) => string.Compare(item1.Name, item2.Name));
                    Console.WriteLine("Инвентарь отсортирован по имени.");
                    break;
                case "2":
                    Array.Sort(items, (item1, item2) => item1.Quantity.CompareTo(item2.Quantity));
                    Console.WriteLine("Инвентарь отсортирован по количеству.");
                    break;
                default:
                    Console.WriteLine("Некорректный выбор для сортировки.");
                    break;
            }
        }

        static void FilterInventory()
        {
            Console.WriteLine("Выберите поле для фильтрации:");
            Console.WriteLine("1. По имени");
            Console.WriteLine("2. По количеству");
            string filterChoice = Console.ReadLine();

            switch (filterChoice)
            {
                case "1":
                    Console.Write("Введите имя для фильтрации: ");
                    string filterName = Console.ReadLine();
                    var filteredByName = items.Where(item => item.Name.Contains(filterName)).ToArray();
                    DisplayFilteredInventory(filteredByName);
                    break;
                case "2":
                    Console.Write("Введите минимальное количество для фильтрации: ");
                    int minQuantity;
                    while (!int.TryParse(Console.ReadLine(), out minQuantity) || minQuantity < 0)
                    {
                        Console.WriteLine("Введите корректное неотрицательное число.");
                        Console.Write("Введите минимальное количество: ");
                    }
                    var filteredByQuantity = items.Where(item => item.Quantity >= minQuantity).ToArray();
                    DisplayFilteredInventory(filteredByQuantity);
                    break;
                default:
                    Console.WriteLine("Некорректный выбор для фильтрации.");
                    break;
            }
        }

        static void DisplayFilteredInventory(Item[] filteredItems)
        {
            if (filteredItems.Length == 0)
            {
                Console.WriteLine("Нет элементов, соответствующих критериям фильтрации.");
            }
            else
            {
                Console.WriteLine("Отфильтрованный инвентарь:");
                for (int i = 0; i < filteredItems.Length; i++)
                {
                    Console.WriteLine($"{i + 1}. Name: {filteredItems[i].Name}, Quantity: {filteredItems[i].Quantity}");
                }
            }
        }
    }
}