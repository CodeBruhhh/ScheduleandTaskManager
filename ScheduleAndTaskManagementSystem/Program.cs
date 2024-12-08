using Microsoft.VisualBasic;
using System;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ScheduleAndTaskManagementSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            int choice = 1;
            UserManager userManager = new UserManager();

            Console.Write("Press any key to start: ");
            Console.ReadKey();

            userManager.DisplayTitle();

            //Login Menu
            while (true)
            {
                userManager.LoginMenu();
                Console.Write("Enter Option: ");
                try
                {
                    int menuChoice = int.Parse(Console.ReadLine());

                    if (menuChoice == 1)
                    {
                        userManager.Register();
                    }
                    else if (menuChoice == 2)
                    {
                        if (userManager.Login())
                        {
                            break;
                        }
                    }
                    else if (menuChoice == 3)
                    {
                        return;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid Option. Please try again.\n");
                        Console.ResetColor();
                    }
                }

                catch (IOException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.ResetColor();
                }
                catch (OutOfMemoryException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Sorry. You have insufficient memory.\n");
                    Console.ResetColor();
                }
                catch (IndexOutOfRangeException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid Input. Object does not exist.\n");
                    Console.ResetColor();
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.ResetColor();
                }
            }

            //Task Manager Menu
            do
            {
                userManager.CurrentUser.TaskAndEventManager = FileManager.LoadTasksAndEventsFromCsv(userManager.CurrentUser.UserName);

                userManager.CurrentUser.TaskAndEventManager.CheckReminders();
                userManager.CurrentUser.TaskAndEventManager.DisplayMenu();

                Console.Write("Choose Option: ");
                try
                {
                    choice = int.Parse(Console.ReadLine());

                    if (choice == 0)
                    {
                        Console.WriteLine("GOOD BYE!");
                    }
                    else if (choice == 1)
                    {
                        userManager.CurrentUser.TaskAndEventManager.CreateTaskOrEvent();
                        FileManager.SaveTasksAndEventsToCsv(userManager.CurrentUser.UserName, userManager.CurrentUser.TaskAndEventManager);
                    }
                    else if (choice == 2)
                    {
                        userManager.CurrentUser.TaskAndEventManager.EditTaskOrEvent();
                        FileManager.SaveTasksAndEventsToCsv(userManager.CurrentUser.UserName, userManager.CurrentUser.TaskAndEventManager);
                    }
                    else if (choice == 3)
                    {
                        userManager.CurrentUser.TaskAndEventManager.DeleteTaskOrEvent();
                        FileManager.SaveTasksAndEventsToCsv(userManager.CurrentUser.UserName, userManager.CurrentUser.TaskAndEventManager);
                    }
                    else if (choice == 4)
                    {
                        userManager.CurrentUser.TaskAndEventManager.ViewTasksOrEvents();
                        FileManager.SaveTasksAndEventsToCsv(userManager.CurrentUser.UserName, userManager.CurrentUser.TaskAndEventManager);
                    }
                    else if (choice == 5)
                    {
                        userManager.CurrentUser.TaskAndEventManager.MarkTaskOrEventAsDone();
                        FileManager.SaveTasksAndEventsToCsv(userManager.CurrentUser.UserName, userManager.CurrentUser.TaskAndEventManager);
                    }
                    else if (choice == 6)
                    {
                        userManager.LogOut();

                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid Option. Please try again.\n");
                        Console.ResetColor();
                    }
                }
                catch (IOException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.ResetColor();
                }
                catch (OutOfMemoryException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Sorry. You have insufficient memory.\n");
                    Console.ResetColor();
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid Input. Object does not exist.\n");
                    Console.ResetColor();
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.ResetColor();
                }
            } while (choice != 0);
        }
    }

    public enum PriorityLevel
    {
        Urgent,
        Important,
        Postponable
    }

    class Task
    {
        private static int taskCounter = FileManager.LoadTaskCounter();

        private int taskId;
        private string title;
        private string description;
        private PriorityLevel priority;
        private DateTime dueDate;
        private bool isCompleted;

        public int TaskId
        {
            get { return this.taskId; }
            set { this.taskId = value; }
        }

        public string Title
        {
            get { return this.title; }
            set { this.title = value; }
        }

        public string Description
        {
            get { return this.description; }
            set { this.description = value; }
        }

        public PriorityLevel Priority
        {
            get { return this.priority; }
            set { this.priority = value; }
        }

        public DateTime DueDate
        {
            get { return this.dueDate; }
            set { this.dueDate = value; }
        }

        public Task(string Title, string Description, PriorityLevel Priority, DateTime DueDate)
        {
            title = Title;
            taskId = ++taskCounter;
            description = Description;
            priority = Priority;
            dueDate = DueDate;
            isCompleted = false;

            FileManager.SaveTaskCounter(taskId);
        }

        public Task()
        {
            title = "Title";
            taskId = 0;
            description = "Description";
            priority = PriorityLevel.Important;
            isCompleted = false;
        }

        public override string ToString()
        {
            return $"{taskId},{title},{description},{priority},{dueDate},{isCompleted}";
        }

        public static Task FromCsv(string csvLine)
        {
            string[] parts = csvLine.Split(',');
            return new Task
            {
                taskId = int.Parse(parts[0]),
                title = parts[1],
                description = parts[2],
                priority = (PriorityLevel)Enum.Parse(typeof(PriorityLevel), parts[3], true),
                dueDate = DateTime.Parse(parts[4]),
                isCompleted = bool.Parse(parts[5])
            };
        }

        public void EditTaskDetails()
        {
            Console.Write("Enter Title: ");
            title = Console.ReadLine();

            Console.Write("Enter description: ");
            description = Console.ReadLine();

            while (true)
            {
                Console.Write("Choose priority level[ Urgent, Important, and Postponable ]: ");
                string input = Console.ReadLine();

                if (Enum.TryParse(input, true, out PriorityLevel Priority) && Enum.IsDefined(typeof(PriorityLevel), Priority))
                {
                    priority = Priority;
                    break;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid input. Please try again.");
                    Console.ResetColor();
                }
            }

            Console.WriteLine("Enter Deadline:");
            while (true)
            {
                Console.Write("Please enter a date and time (e.g., 2024-10-31 14:30): ");
                string userInput = Console.ReadLine();

                if (DateTime.TryParse(userInput, out dueDate))
                {
                    Console.WriteLine("Deadline set successfully!");
                    break;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid input. Please try again.");
                    Console.ResetColor();
                }
            }
        }

        public void Reminder()
        {
            DateTime now = DateTime.Now;

            if (dueDate.AddDays(-1) <= now && dueDate >= now && isCompleted == false)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Reminder: {title} is due in less than a day");
                Console.ResetColor();
            }
            else if (dueDate < now && isCompleted == false)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed; // Overdue task
                Console.WriteLine($"Overdue: {title} is overdue!");
                Console.ResetColor();
            }
        }

        public void MarkAsDone()
        {
            isCompleted = true;
        }
    }

    class Event
    {
        private static int eventCounter = FileManager.LoadEventCounter();

        private int eventId;
        private string eventName;
        private string eventDetails;
        private string location;
        private DateTime eventDate;
        private bool isCompleted;

        public int EventId
        {
            get { return this.eventId; }
            set { this.eventId = value; }
        }
        public string EventName
        {
            get { return this.eventName; }
            set { this.eventName = value; }
        }
        public string EventDetails
        {
            get { return this.eventDetails ; }
            set { this.eventDetails = value; }
        }
        public string Location
        {
            get { return this.location; }
            set { this.location = value; }
        }
        public DateTime EventDate
        {
            get { return this.eventDate; }
            set { this.eventDate = value; }
        }

        public Event(string EventName, string EventDetails, string Location, DateTime EventDate)
        {
            eventId = ++eventCounter;
            eventName = EventName;
            eventDetails = EventDetails;
            location = Location;
            eventDate = EventDate;
            isCompleted = false;

            FileManager.SaveEventCounter(eventId);
        }

        public Event()
        {
            eventName = "Untitled";
            eventDetails = "Event Details...";
            location = "Location";
        }

        public override string ToString()
        {
            return $"{eventId},{eventName},{eventDetails},{location},{eventDate},{isCompleted}";
        }

        public static Event FromCsv(string csvLine)
        {
            string[] parts = csvLine.Split(',');
            return new Event
            {
                eventId = int.Parse(parts[0]),
                eventName = parts[1],
                eventDetails = parts[2],
                location = parts[3],
                eventDate = DateTime.Parse(parts[4]),
                isCompleted = bool.Parse(parts[5])
            };
        }

        public void EditEventDetails()
        {
            Console.Write("Enter Event Name: ");
            eventName = Console.ReadLine();

            Console.Write("Enter Event Details: ");
            eventDetails = Console.ReadLine();

            Console.Write("Enter Event Location or Address: ");
            location = Console.ReadLine();

            Console.WriteLine("Enter Deadline:");
            while (true)
            {
                Console.Write("Please enter a date and time (e.g., 2024-10-31 14:30): ");
                string userInput = Console.ReadLine();

                if (DateTime.TryParse(userInput, out eventDate))
                {
                    Console.WriteLine("Deadline set successfully!");
                    break;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid input. Please try again.");
                    Console.ResetColor();
                }
            }
        }

        public void Reminder()
        {
            DateTime now = DateTime.Now;

            if (eventDate.AddDays(-1) <= now && eventDate >= now && isCompleted == false)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Reminder: {eventName} is due in less than a day");
                Console.ResetColor();
            }
            else if (eventDate < now && isCompleted == false)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed; // Overdue task
                Console.WriteLine($"Overdue: {eventName} is overdue!");
                Console.ResetColor();
            }
        }
        public void MarkAsDone()
        {
            isCompleted = true;
        }
    }

    class TaskAndEventManager
    {
        private List<Task> tasks = new List<Task>();
        private List<Event> events = new List<Event>();

        public List<Task> Tasks
        {
            get { return this.tasks; }
            set { this.tasks = value; }
        }

        public List<Event> Events
        {
            get { return this.events; }
            set { this.events = value; }
        }

        public void CheckReminders()
        {
            foreach (var task in tasks)
            {
                task.Reminder();
            }
            foreach (var ev in events)
            {
                ev.Reminder();
            }
        }

        public void DisplayMenu()
        {
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("| S C H E D U L E   A N D   T A S K   M A N A G E R |");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("| 1. Create a Task / Event                          |");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("| 2. Edit Tasks / Events                            |");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("| 3. Delete Tasks / Events                          |");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("| 4. Display Tasks / Events                         |");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("| 5. Mark Task / Event as Done                      |");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("| 0. End Program                                    |");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------\n");
        }

        public void CreateTaskOrEvent()
        {
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("| 1. Create Task                                    |");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("| 2. Create Event                                   |");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------\n");

            Console.Write("Choose Option: ");
            int option = int.Parse(Console.ReadLine());

            if (option == 1)
            {
                string title, description;
                PriorityLevel priority;
                DateTime dueDate;

                Console.Write("Enter Title: ");
                title = Console.ReadLine();

                Console.Write("Enter description: ");
                description = Console.ReadLine();

                while (true)
                {
                    Console.Write("Choose priority level[ Urgent, Important, and Postponable ]: ");
                    string input = Console.ReadLine();

                    if (Enum.TryParse(input, true, out PriorityLevel output) && Enum.IsDefined(typeof(PriorityLevel), output))
                    {
                        priority = output;
                        break;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid input. Please try again.\n");
                        Console.ResetColor();
                    }
                }

                Console.WriteLine("Enter Deadline:");
                while (true)
                {
                    Console.Write("Please enter a date and time (e.g., 2024-10-31 14:30): ");
                    string userInput = Console.ReadLine();

                    if (DateTime.TryParse(userInput, out dueDate))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Deadline set successfully!\n");
                        Console.ResetColor();
                        break;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid input. Please try again.\n");
                        Console.ResetColor();
                    }
                }
                Task task = new Task(title, description, priority, dueDate);
                tasks.Add(task);
            }
            else if (option == 2)
            {
                string EventName, EventDetails, Location;
                DateTime EventDate;

                Console.Write("Enter Event Name: ");
                EventName = Console.ReadLine();

                Console.Write("Enter Event Details: ");
                EventDetails = Console.ReadLine();

                Console.Write("Enter Event Location or Address: ");
                Location = Console.ReadLine();

                Console.WriteLine("Enter Deadline:");
                while (true)
                {
                    Console.Write("Please enter a date and time (e.g., 2024-10-31 14:30): ");
                    string userInput = Console.ReadLine();

                    if (DateTime.TryParse(userInput, out EventDate))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Deadline set successfully!\n");
                        Console.ResetColor();
                        break;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid input. Please try again.\n");
                        Console.ResetColor();
                    }
                }
                Event ev = new Event(EventName, EventDetails, Location, EventDate);
                events.Add(ev);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid Option. Please try again.");
                Console.ResetColor();
            }
        }

        public void EditTaskOrEvent()
        {
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("| 1. Edit Task                                      |");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("| 2. Edit Event                                     |");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------\n");

            Console.Write("Choose Option: ");
            int option = int.Parse(Console.ReadLine());

            if(option == 1)
            {
                Console.Write("Enter the id number of the task you want to edit: ");
                int id = int.Parse(Console.ReadLine());

                int index = tasks.FindIndex(t => t.TaskId == id);
                tasks[index].EditTaskDetails();
            }
            else if (option == 2)
            {
                Console.Write("Enter the id number of the Event you want to edit: ");
                int id = int.Parse(Console.ReadLine());

                int index = events.FindIndex(e => e.EventId == id);
                events[index].EditEventDetails();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid Option. Please try again.");
                Console.ResetColor();
            }
        }

        public void DeleteTaskOrEvent()
        {
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("| 1. Delete Task                                    |");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("| 2. Delete Event                                   |");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------\n");

            Console.Write("Choose Option: ");
            int option = int.Parse(Console.ReadLine());

            if (option == 1)
            {
                Console.Write("Enter the id number of the task you want to Delete: ");
                int id = int.Parse(Console.ReadLine());

                int index = tasks.FindIndex(t => t.TaskId == id);
                tasks.RemoveAt(index);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Successfully Deleted Task\n");
                Console.ResetColor();
            }
            else if (option == 2)
            {
                Console.Write("Enter the id number of the event you want to delete: ");
                int id = int.Parse(Console.ReadLine());

                int index = events.FindIndex(e => e.EventId == id);
                events.RemoveAt(index);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Successfully Deleted Event\n");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid Option. Please try again.");
                Console.ResetColor();
            }
        }

        public void ViewTasks()
        {
            if (tasks.Count == 0)
            {
                Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                Console.WriteLine("\n\t\t\t\tNo Tasks available.\n");
                Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                return;
            }

            foreach (var task in tasks)
            {
                Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                Console.WriteLine($"\n\t\t\t\tID #{task.TaskId}\n\t\t\t\tTitle: {task.Title}\n\t\t\t\tDescription: {task.Description}\n\t\t\t\tPriority Level: {task.Priority.ToString()}\n\t\t\t\tDeadline: {task.DueDate}\n");
                Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
            }
        }
        public void ViewEvents()
        {
            if (events.Count == 0)
            {
                Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                Console.WriteLine("\n\t\t\t\tNo Events available.\n");
                Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                return;
            }

            foreach (var ev in events)
            {
                Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                Console.WriteLine($"\n\t\t\t\tID #{ev.EventId}\n\t\t\t\tName: {ev.EventName}\n\t\t\t\tDetails: {ev.EventDetails}\n\t\t\t\tLocation/Address: {ev.Location}\n\t\t\t\tDate: {ev.EventDate}\n");
                Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
            }
        }

        public void ViewTasksOrEvents()
        {
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("| 1. View Tasks                                     |");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("| 2. View Events                                    |");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------\n");

            Console.Write("Choose Option: ");
            int option = int.Parse(Console.ReadLine());

            if (option == 1)
                {
                if (tasks.Count == 0)
                {
                    Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                    Console.WriteLine("\n\t\t\t\tNo Tasks available.\n");
                    Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                    return;
                }

                ViewTasks();

                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("-----------------------------------------------------");
                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("|                      S O R T                      |");
                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("-----------------------------------------------------");
                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("-----------------------------------------------------");
                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("| 1. Sort by Closest Deadline                       |");
                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("-----------------------------------------------------");
                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("-----------------------------------------------------");
                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("| 2. Sort by Priority                               |");
                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("-----------------------------------------------------");
                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("-----------------------------------------------------");
                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("| 3. View by Month                                  |");
                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("-----------------------------------------------------");
                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("-----------------------------------------------------");
                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("| 0. Exit                                           |");
                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("-----------------------------------------------------\n");
                Console.Write("Choose Option: ");
                int choice = int.Parse(Console.ReadLine());

                if (choice == 0)
                {
                    Console.WriteLine();
                }
                else if (choice == 1)
                {
                    tasks.Sort((task1, task2) => task1.DueDate.CompareTo(task2.DueDate));

                    foreach (var task in tasks)
                    {
                        Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                        Console.WriteLine($"\n\t\t\t\tID #{task.TaskId}\n\t\t\t\tTitle: {task.Title}\n\t\t\t\tDescription: {task.Description}\n\t\t\t\tPriority Level: {task.Priority.ToString()}\n\t\t\t\tDeadline: {task.DueDate}\n");
                        Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                    }
                }
                else if (choice == 2)
                {
                    tasks.Sort((task1, task2) => task1.Priority.CompareTo(task2.Priority));

                    foreach (var task in tasks)
                    {
                        Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                        Console.WriteLine($"\n\t\t\t\tID #{task.TaskId}\n\t\t\t\tTitle: {task.Title}\n\t\t\t\tDescription: {task.Description}\n\t\t\t\tPriority Level: {task.Priority.ToString()}\n\t\t\t\tDeadline: {task.DueDate}\n");
                        Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                    }
                }
                else if (choice == 3)
                {
                    int month;
                    do
                    {
                        Console.Write("Choose Month (1-12, 0 to exit): ");
                        month = int.Parse(Console.ReadLine());

                        if (month == 0)
                        {
                            break;
                        }

                        if (month < 1 || month > 12)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Invalid month. Please enter a number between 1 and 12.");
                            Console.ResetColor();
                            continue;
                        }

                        string monthName = new DateTime(2024, month, 1).ToString("MMMM", CultureInfo.InvariantCulture);

                        var tasksForMonth = tasks.Where(t => t.DueDate.Month == month).ToList();

                        Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                        Console.WriteLine($"\n\t\t\t\tTasks for {monthName}\n");
                        Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");

                        if (tasksForMonth.Count == 0)
                        {
                            Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                            Console.WriteLine($"\n\t\t\t\tNo Tasks for {monthName}.\n");
                            Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                        }
                        else
                        {
                            foreach (var task in tasksForMonth)
                            {
                                Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                                Console.WriteLine($"\n\t\t\t\tID #{task.TaskId}\n\t\t\t\tTitle: {task.Title}\n\t\t\t\tDescription: {task.Description}\n\t\t\t\tPriority Level: {task.Priority.ToString()}\n\t\t\t\tDeadline: {task.DueDate.ToString("d")}\n");
                                Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                            }
                        }

                    } while (month != 0);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid Option. Please try again.");
                    Console.ResetColor();
                }
            }
            else if (option == 2)
            {
                if (events.Count == 0)
                {
                    Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                    Console.WriteLine("\n\t\t\t\tNo Events available.\n");
                    Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                    return;
                }

                ViewEvents();

                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("-----------------------------------------------------");
                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("|                      S O R T                      |");
                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("-----------------------------------------------------");
                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("-----------------------------------------------------");
                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("| 1. Sort by Closest Deadline                       |");
                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("-----------------------------------------------------");
                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("-----------------------------------------------------");
                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("| 2. View by Month                                  |");
                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("-----------------------------------------------------");
                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("-----------------------------------------------------");
                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("| 0. Exit                                           |");
                Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
                Console.WriteLine("-----------------------------------------------------\n");
                Console.Write("Choose Option: ");
                int choice = int.Parse(Console.ReadLine());

                if (choice == 0)
                {
                    Console.WriteLine();
                }
                else if (choice == 1)
                {
                    events.Sort((event1, event2) => event1.EventDate.CompareTo(event2.EventDate));

                    foreach (var ev in events)
                    {
                        Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                        Console.WriteLine($"\n\t\t\t\tID #{ev.EventId}\n\t\t\t\tName: {ev.EventName}\n\t\t\t\tDetails: {ev.EventDetails}\n\t\t\t\tLocation/Address: {ev.Location}\n\t\t\t\tDate: {ev.EventDate}\n");
                        Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                    }
                }
                else if (choice == 2)
                {
                    int month;
                    do
                    {
                        Console.Write("Choose Month (1-12, 0 to exit): ");
                        month = int.Parse(Console.ReadLine());

                        if (month == 0)
                        {
                            break;
                        }

                        if (month < 1 || month > 12)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Invalid month. Please enter a number between 1 and 12.");
                            Console.ResetColor();
                            continue;
                        }

                        string monthName = new DateTime(2024, month, 1).ToString("MMMM", CultureInfo.InvariantCulture);

                        var eventsForMonth = events.Where(e => e.EventDate.Month == month).ToList();

                        Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                        Console.WriteLine($"\n\t\t\t\tEvents for {monthName}\n");
                        Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");

                        if (eventsForMonth.Count == 0)
                        {
                            Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                            Console.WriteLine($"\n\t\t\t\tNo Events for {monthName}.\n");
                            Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                        }
                        else
                        {
                            foreach (var ev in eventsForMonth)
                            {
                                Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                                Console.WriteLine($"\n\t\t\t\tID #{ev.EventId}\n\t\t\t\tName: {ev.EventName}\n\t\t\t\tDetails: {ev.EventDetails}\n\t\t\t\tLocation/Address: {ev.Location}\n\t\t\t\tDate: {ev.EventDate}\n");
                                Console.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                            }
                        }

                    } while (month != 0);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid Option. Please try again.");
                    Console.ResetColor();
                }
            }
        }

        public void MarkTaskOrEventAsDone()
        {

            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("| 1. Mark Task as Done                              |");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("| 2. Mark Event as Done                             |");
            Console.SetCursorPosition((Console.WindowWidth - 53) / 2, Console.CursorTop);
            Console.WriteLine("-----------------------------------------------------\n");

            Console.Write("Choose Option: ");
            int option = int.Parse(Console.ReadLine());

            if (option == 1)
            {
                Console.Write("Enter the id number of the task you want to Mark as Done: ");
                int id = int.Parse(Console.ReadLine());

                int index = tasks.FindIndex(t => t.TaskId == id);
                tasks[index].MarkAsDone();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Successfully Marked {tasks[index].Title} as Done");
                Console.ResetColor();
            }
            else if (option == 2)
            {
                Console.Write("Enter the id number of the Event you want to Mark as Done: ");
                int id = int.Parse(Console.ReadLine());

                int index = events.FindIndex(e => e.EventId == id);
                events[index].MarkAsDone();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Successfully Marked {events[index].EventName} as Done");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid Option. Please try again.");
                Console.ResetColor();
            }
        }
    }

    class User
    {
        private int userId;
        private string userName;
        private string hashedPassword;
        private byte[] salt;

        private TaskAndEventManager taskAndEventManager;

        public int UserId
        {
            get { return this.userId; }
            set { this.userId = value; }
        }

        public string UserName
        {
            get { return this.userName; }
            set { this.userName = value; }
        }

        public string HashedPassword
        {
            get { return this.hashedPassword; }
            set { this.hashedPassword = value; }
        }

        public byte[] Salt
        {
            get { return this.salt; }
            set { this.salt = value; }
        }

        public TaskAndEventManager TaskAndEventManager
        {
            get { return this.taskAndEventManager; }
            set { this.taskAndEventManager = value; }
        }

        public User(int UserId, string UserName, string Password)
        {
            userName = UserName;
            userId = UserId;
            salt = GenerateSalt();
            hashedPassword = HashPasswordWithSalt(Password, salt);
            taskAndEventManager = new TaskAndEventManager();
        }

        public User(int UserId, string UserName, string HashedPassword, string Salt)
        {
            userName = UserName;
            userId = UserId;
            hashedPassword = HashedPassword;
            salt = Convert.FromBase64String(Salt);
            taskAndEventManager = new TaskAndEventManager();
        }

        public static string HashPasswordWithSalt(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000))
            {
                byte[] hash = pbkdf2.GetBytes(32);
                return Convert.ToBase64String(hash);
            }
        }

        private static byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        public static bool VerifyPassword(string enteredPassword, string storedHash, byte[] storedSalt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, storedSalt, 100000))
            {
                byte[] hash = pbkdf2.GetBytes(32);
                string enteredHash = Convert.ToBase64String(hash); 
                return enteredHash == storedHash;
            }
        }
    }

    class UserManager
    {
        private List<User> users = new List<User>();
        private User? currentUser;

        public UserManager()
        {
            users = FileManager.LoadUsersFromFile();
        }

        public User? CurrentUser
        {
            get { return this.currentUser; }
            set { this.currentUser = value; }
        }

        public void DisplayTitle()
        {
            string[] art = {
            "  _______        _      __  __                                   ",
            " |__   __|      | |    |  \\/  |                                  ",
            "    | | __ _ ___| | __ | \\  / | __ _ _ __   __ _  __ _  ___ _ __ ",
            "    | |/ _` / __| |/ / | |\\/| |/ _` | '_ \\ / _` |/ _` |/ _ \\ '__|",
            "    | | (_| \\__ \\   <  | |  | | (_| | | | | (_| | (_| |  __/ |   ",
            "    |__\\__,_|___/_|\\_\\ |_|  |_|\\__,_|_| |_|\\__,_|\\__, |\\___|_|   ",
            "                                                  __/ |          ",
            "                                                  |___/            "
        };

            int consoleWidth = Console.WindowWidth;

            Console.WriteLine();
            foreach (var line in art)
                Console.WriteLine($"{new string(' ', (consoleWidth - line.Length) / 2)}{line}");

            Console.SetCursorPosition((Console.WindowWidth - 15) / 2, Console.CursorTop);
            Console.WriteLine("W E L C O M E !\n");
        }

        public void LoginMenu()
        {
            //Console.WriteLine("  _______        _      __  __                                   \r\n |__   __|      | |    |  \\/  |                                  \r\n    | | __ _ ___| | __ | \\  / | __ _ _ __   __ _  __ _  ___ _ __ \r\n    | |/ _` / __| |/ / | |\\/| |/ _` | '_ \\ / _` |/ _` |/ _ \\ '__|\r\n    | | (_| \\__ \\   <  | |  | | (_| | | | | (_| | (_| |  __/ |   \r\n    |_|\\__,_|___/_|\\_\\ |_|  |_|\\__,_|_| |_|\\__,_|\\__, |\\___|_|   \r\n                                                  __/ |          \r\n                                                 |___/           ");
            //Console.WriteLine(".--.      .--.    .-''-.    .---.        _______      ,-----.    ,---.    ,---.    .-''-.  .---.  \r\n|  |_     |  |  .'_ _   \\   | ,_|       /   __  \\   .'  .-,  '.  |    \\  /    |  .'_ _   \\ \\   /  \r\n| _( )_   |  | / ( ` )   ',-./  )      | ,_/  \\__) / ,-.|  \\ _ \\ |  ,  \\/  ,  | / ( ` )   '|   |  \r\n|(_ o _)  |  |. (_ o _)  |\\  '_ '`)  ,-./  )      ;  \\  '_ /  | :|  |\\_   /|  |. (_ o _)  | \\ /   \r\n| (_,_) \\ |  ||  (_,_)___| > (_)  )  \\  '_ '`)    |  _`,/ \\ _/  ||  _( )_/ |  ||  (_,_)___|  v    \r\n|  |/    \\|  |'  \\   .---.(  .  .-'   > (_)  )  __: (  '\\_/ \\   ;| (_ o _) |  |'  \\   .---. _ _   \r\n|  '  /\\  `  | \\  `-'    / `-'`-'|___(  .  .-'_/  )\\ `\"/  \\  ) / |  (_,_)  |  | \\  `-'    /(_I_)  \r\n|    /  \\    |  \\       /   |        \\`-'`-'     /  '. \\_/``\".'  |  |      |  |  \\       /(_(=)_) \r\n`---'    `---`   `'-..-'    `--------`  `._____.'     '-----'    '--'      '--'   `'-..-'  (_I_)  \r\n                                                                                                  ");

            Console.SetCursorPosition((Console.WindowWidth - 25) / 2, Console.CursorTop);
            Console.WriteLine("========================");
            Console.SetCursorPosition((Console.WindowWidth - 25) / 2, Console.CursorTop);
            Console.WriteLine("|| 1. R E G I S T E R ||");
            Console.SetCursorPosition((Console.WindowWidth - 25) / 2, Console.CursorTop);
            Console.WriteLine("========================");
            Console.SetCursorPosition((Console.WindowWidth - 25) / 2, Console.CursorTop);
            Console.WriteLine("|| 2. L O G   I N     ||");
            Console.SetCursorPosition((Console.WindowWidth - 25) / 2, Console.CursorTop);
            Console.WriteLine("========================");
            Console.SetCursorPosition((Console.WindowWidth - 25) / 2, Console.CursorTop);
            Console.WriteLine("|| 3. E X I T         ||");
            Console.SetCursorPosition((Console.WindowWidth - 25) / 2, Console.CursorTop);
            Console.WriteLine("========================\n");
        }

        public bool Register()
        {
            Console.Write("Enter username: ");
            string username = Console.ReadLine();

            Console.Write("Enter password: ");

            string password = string.Empty;
            ConsoleKey key;

            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password[0..^1];
                    Console.Write("\b \b"); // Erase the last '*'
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    password += keyInfo.KeyChar;
                    Console.Write("*");
                }
            } while (key != ConsoleKey.Enter);
            Console.WriteLine();


            if (users.Exists(u => u.UserName == username))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Username already exists.\n");
                Console.ResetColor();
                return false;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Registration successful!\n");
            Console.ResetColor();
            int userId = users.Count + 1;
            users.Add(new User(userId, username, password));

            FileManager.SaveUsersToFile(users);
            return true;
        }

        public bool Login()
        {
            Console.Write("Enter username: ");
            string username = Console.ReadLine();
            Console.Write("Enter password: ");

            string password = string.Empty;
            ConsoleKey key;

            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password[0..^1];
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    password += keyInfo.KeyChar;
                    Console.Write("*");
                }
            } while (key != ConsoleKey.Enter);
            Console.WriteLine();

            User? user = users.Find(u => u.UserName == username);

            if (user != null)
            {
                // Verify the password using the static VerifyPassword method
                bool isPasswordValid = User.VerifyPassword(password, user.HashedPassword, user.Salt);

                if (isPasswordValid)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Login successful!\n");
                    Console.ResetColor();

                    currentUser = user;
                    return true;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid username or password.\n");
                    Console.ResetColor();
                    return false;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid username or password.\n");
                Console.ResetColor();
                return false;
            }
        }

        public void LogOut()
        {
            currentUser = null;
        }
    }

    static class FileManager
    {
        private static string usersFilePath = "users.csv";
        private static string TaskCounterFilePath = "taskCounter.txt";
        private static string EventCounterFilePath = "eventCounter.txt";

        public static void SaveTasksAndEventsToCsv(string username, TaskAndEventManager taskManager)
        {
            string TasksFilePath = $"{username}_Tasks.csv";
            string EventsFilePath = $"{username}_Events.csv";

            using (var writer = new StreamWriter(TasksFilePath))
            {
                writer.WriteLine("Id,Title,Description,Priority,Due Date,Status"); // Header
                foreach (var task in taskManager.Tasks)
                {
                    writer.WriteLine(task.ToString());
                }
            }

            using (var writer = new StreamWriter(EventsFilePath))
            {
                writer.WriteLine("Id,Event Name,Details,Location,Date,Status"); // Header
                foreach (var ev in taskManager.Events)
                {
                    writer.WriteLine(ev.ToString());
                }
            }
        }

        public static TaskAndEventManager LoadTasksAndEventsFromCsv(string username)
        {
            string TasksFilePath = $"{username}_Tasks.csv";
            string EventsFilePath = $"{username}_Events.csv";

            TaskAndEventManager taskManager = new TaskAndEventManager();

            if (File.Exists(TasksFilePath))
            {
                using (var reader = new StreamReader(TasksFilePath))
                {
                    reader.ReadLine(); // Skip the header
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        taskManager.Tasks.Add(Task.FromCsv(line));
                    }
                }
            }

            if (File.Exists(EventsFilePath))
            {
                using (var reader = new StreamReader(EventsFilePath))
                {
                    reader.ReadLine(); // Skip the header
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        taskManager.Events.Add(Event.FromCsv(line));
                    }
                }
            }

            return taskManager;
        }

        public static void SaveUsersToFile(List<User> users)
        {
            using (StreamWriter writer = new StreamWriter(usersFilePath))
            {
                writer.WriteLine("Id,Username,Hashed Password, Salt");  // Header row

                foreach (var user in users)
                {
                    writer.WriteLine($"{user.UserId},{user.UserName},{user.HashedPassword}, {Convert.ToBase64String(user.Salt)}");
                }
            }
        }

        public static List<User> LoadUsersFromFile()
        {
            List<User> users = new List<User>();

            if (!File.Exists(usersFilePath))
            {
                return users;
            }

            using (StreamReader reader = new StreamReader(usersFilePath))
            {
                string headerLine = reader.ReadLine();  // Skip header line

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] parts = line.Split(',');

                    if (parts.Length == 4)
                    {
                        User user = new User(int.Parse(parts[0]), parts[1], parts[2], parts[3]);
                        users.Add(user);
                    }
                }
            }

            return users;
        }

        public static void SaveTaskCounter(int counter)
        {
            File.WriteAllText(TaskCounterFilePath, counter.ToString());
        }

        public static int LoadTaskCounter()
        {
            if (File.Exists(TaskCounterFilePath))
            {
                string fileContent = File.ReadAllText(TaskCounterFilePath);
                if (int.TryParse(fileContent, out int savedCounter))
                {
                    return savedCounter;
                }
            }
            return 0;
        }

        public static void SaveEventCounter(int counter)
        {
            File.WriteAllText(EventCounterFilePath, counter.ToString());
        }

        public static int LoadEventCounter()
        {
            if (File.Exists(EventCounterFilePath))
            {
                string fileContent = File.ReadAllText(EventCounterFilePath);
                if (int.TryParse(fileContent, out int savedCounter))
                {
                    return savedCounter;
                }
            }
            return 0;
        }
    }
}