namespace prak3_
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    namespace TaskManager
    {
        public class User
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class TaskItem
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Priority { get; set; } // low, medium, high
            public string Status { get; set; } // not started, in progress, completed
        }

        public class Program
        {
            private static readonly string UsersFilePath = "users.txt";
            private static readonly string TasksFilePath = "tasks.txt";
            private static List<User> users = new List<User>();
            private static Dictionary<string, List<TaskItem>> userTasks = new Dictionary<string, List<TaskItem>>();

            public static async Task Main(string[] args)
            {
                await LoadUsersAsync();
                await LoadTasksAsync();

                Console.WriteLine("Добро пожаловать!");
                bool isRunning = true;

                while (isRunning)
                {
                    Console.WriteLine("1. Регистрация");
                    Console.WriteLine("2. Вход");
                    Console.WriteLine("3. Выход");

                    string choice = Console.ReadLine();
                    switch (choice)
                    {
                        case "1":
                            await RegisterAsync();
                            break;
                        case "2":
                            await LoginAndManageTasksAsync();
                            break;
                        case "3":
                            isRunning = false;
                            break;
                        default:
                            Console.WriteLine("Некорректный выбор. Попробуйте снова.");
                            break;
                    }
                }
            }

            private static async Task RegisterAsync()
            {
                Console.Write("Введите имя пользователя: ");
                string username = Console.ReadLine();
                Console.Write("Введите пароль: ");
                string password = Console.ReadLine();

                if (users.Any(u => u.Username == username))
                {
                    Console.WriteLine("Пользователь с таким именем уже существует.");
                    return;
                }

                users.Add(new User { Username = username, Password = password });
                await SaveUsersAsync();
                Console.WriteLine("Регистрация успешно завершена.");
            }

            private static async Task LoginAndManageTasksAsync()
            {
                Console.Write("Введите имя пользователя: ");
                string username = Console.ReadLine();
                Console.Write("Введите пароль: ");
                string password = Console.ReadLine();

                var user = users.FirstOrDefault(u => u.Username == username && u.Password == password);
                if (user == null)
                {
                    Console.WriteLine("Неверное имя пользователя или пароль.");
                    return;
                }

                bool managingTasks = true;
                while (managingTasks)
                {
                    Console.WriteLine("1. Создать задачу");
                    Console.WriteLine("2. Редактировать задачу");
                    Console.WriteLine("3. Удалить задачу");
                    Console.WriteLine("4. Показать задачи");
                    Console.WriteLine("5. Выйти");

                    string choice = Console.ReadLine();
                    switch (choice)
                    {
                        case "1":
                            await CreateTaskAsync(username);
                            break;
                        case "2":
                            await EditTaskAsync(username);
                            break;
                        case "3":
                            await DeleteTaskAsync(username);
                            break;
                        case "4":
                            ShowTasks(username);
                            break;
                        case "5":
                            managingTasks = false;
                            break;
                        default:
                            Console.WriteLine("Некорректный выбор. Попробуйте снова.");
                            break;
                    }
                }
            }

            private static async Task CreateTaskAsync(string username)
            {
                Console.Write("Введите заголовок задачи: ");
                string title = Console.ReadLine();
                Console.Write("Введите описание задачи: ");
                string description = Console.ReadLine();
                Console.Write("Введите приоритет (низкий, средний, высокий): ");
                string priority = Console.ReadLine();
                string status = "недоступна";

                if (!userTasks.ContainsKey(username))
                {
                    userTasks[username] = new List<TaskItem>();
                }

                userTasks[username].Add(new TaskItem { Title = title, Description = description, Priority = priority, Status = status });
                await SaveTasksAsync();
                Console.WriteLine("Задача успешно создана.");
            }

            private static async Task EditTaskAsync(string username)
            {
                ShowTasks(username);
                Console.Write("Введите номер задачи для редактирования: ");
                int taskNumber;
                if (int.TryParse(Console.ReadLine(), out taskNumber) && taskNumber > 0 && taskNumber <= userTasks[username].Count)
                {
                    var task = userTasks[username][taskNumber - 1];
                    Console.Write("Введите новый заголовок задачи (оставьте пустым для пропуска): ");
                    string newTitle = Console.ReadLine();
                    Console.Write("Введите новое описание задачи (оставьте пустым для пропуска): ");
                    string newDescription = Console.ReadLine();
                    Console.Write("Введите новый приоритет (низкий, средний, высокий) (оставьте пустым для пропуска): ");
                    string newPriority = Console.ReadLine();

                    if (!string.IsNullOrEmpty(newTitle))
                        task.Title = newTitle;

                    if (!string.IsNullOrEmpty(newDescription))
                        task.Description = newDescription;

                    if (!string.IsNullOrEmpty(newPriority))
                        task.Priority = newPriority;

                    await SaveTasksAsync();
                    Console.WriteLine("Задача успешно отредактирована.");
                }
                else
                {
                    Console.WriteLine("Некорректный номер задачи.");
                }
            }

            private static async Task DeleteTaskAsync(string username)
            {
                ShowTasks(username);
                Console.Write("Введите номер задачи для удаления: ");
                int taskNumber;
                if (int.TryParse(Console.ReadLine(), out taskNumber) && taskNumber > 0 && taskNumber <= userTasks[username].Count)
                {
                    userTasks[username].RemoveAt(taskNumber - 1);
                    await SaveTasksAsync();
                    Console.WriteLine("Задача успешно удалена.");
                }
                else
                {
                    Console.WriteLine("Некорректный номер задачи.");
                }
            }

            private static void ShowTasks(string username)
            {
                if (!userTasks.ContainsKey(username) || userTasks[username].Count == 0)
                {
                    Console.WriteLine("Нет задач для отображения.");
                    return;
                }

                Console.WriteLine("Ваши задачи:");
                for (int i = 0; i < userTasks[username].Count; i++)
                {
                    var task = userTasks[username][i];
                    Console.WriteLine($"{i + 1}. {task.Title} - {task.Description} [Приоритет: {task.Priority}, Статус: {task.Status}]");
                }
            }

            private static async Task LoadUsersAsync()
            {
                if (File.Exists(UsersFilePath))
                {
                    var lines = await File.ReadAllLinesAsync(UsersFilePath);
                    foreach (var line in lines)
                    {
                        var parts = line.Split(';');
                        if (parts.Length == 2)
                        {
                            users.Add(new User { Username = parts[0], Password = parts[1] });
                        }
                    }
                }
            }

            private static async Task SaveUsersAsync()
            {
                var lines = users.Select(u => $"{u.Username};{u.Password}");
                await File.WriteAllLinesAsync(UsersFilePath, lines);
            }

            private static async Task LoadTasksAsync()
            {
                if (File.Exists(TasksFilePath))
                {
                    var lines = await File.ReadAllLinesAsync(TasksFilePath);
                    foreach (var line in lines)
                    {
                        var parts = line.Split(';');
                        if (parts.Length == 5)
                        {
                            var username = parts[0];
                            if (!userTasks.ContainsKey(username))
                            {
                                userTasks[username] = new List<TaskItem>();
                            }

                            userTasks[username].Add(new TaskItem
                            {
                                Title = parts[1],
                                Description = parts[2],
                                Priority = parts[3],
                                Status = parts[4]
                            });
                        }
                    }
                }
            }

            private static async Task SaveTasksAsync()
            {
                var lines = userTasks.SelectMany(u => u.Value.Select(t => $"{u.Key};{t.Title};{t.Description};{t.Priority};{t.Status}"));
                await File.WriteAllLinesAsync(TasksFilePath, lines);
            }
        }
    }

}
