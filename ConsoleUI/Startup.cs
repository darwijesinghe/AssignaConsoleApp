using ConsoleUI.Auth;
using ConsoleUI.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleUI
{
    public class Startup
    {
        // Services
        private ILogger<Startup> _logger;
        private IAuthService     _authService;
        private ITaskService     _taskService;

        public Startup(ILogger<Startup> logger, IAuthService authService, ITaskService taskService)
        {
            _logger      = logger;
            _authService = authService;
            _taskService = taskService;
        }

        /// <summary>
        /// User selection
        /// </summary>
        private int selection      = 0;

        const string empty         = "";

        /// <summary>
        /// Regex for validate the email address
        /// </summary>
        private const string regex = @"^[^@\s]+@[^@\s]+\.(com|net|org|gov)$";

        /// <summary>
        /// Starting point
        /// </summary>
        public async Task Run()
        {
            try
            {
                // startup options
                Console.WriteLine("Please select option to continue");
                Console.WriteLine(("").PadRight(32, '-'));
                Console.WriteLine("1. Register");
                Console.WriteLine("2. Login");
                Console.WriteLine("3. Forgot password");
                Console.WriteLine("4. Logout");
                Console.WriteLine();

                // user selection
                selection = int.Parse(Console.ReadLine().Trim());

                switch (selection)
                {
                    case 1: // registration
                        await Register();
                        break;

                    case 2: // login
                        await Login();
                        break;

                    case 3: // forgot password
                        await ForgotPassword();
                        break;

                    default: // logout
                        Console.WriteLine("Are you sure to logout? [Yes / No]");
                        string answer = Console.ReadLine().Trim();
                        if (answer.ToUpper() == "YES")
                        {
                            Console.WriteLine();
                            Console.WriteLine("You are logged out.");
                        }
                        else
                        {
                            Console.WriteLine();
                            await Run();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error, Please try again.");
                _logger.LogError(ex.StackTrace);
                await Run();
            }
        }

        /// <summary>
        /// Registration process.
        /// </summary>
        private async Task Register()
        {
            try
            {
                Console.WriteLine();
                var user = new Register();
                Console.WriteLine("Enter your username (ex: peter@user):");
                user.UserName = Console.ReadLine().Trim();
                do
                {
                    // validates username
                    if (user.UserName.Contains(" "))
                    {
                        Console.WriteLine("Username is not valid, Please enter valid username:");
                        user.UserName = Console.ReadLine().Trim();
                    }
                }
                while (user.UserName.Contains(" "));
                Console.WriteLine("Enter your first name (ex: peter):");
                user.FirstName = Console.ReadLine().Trim();
                Console.WriteLine("Enter your email address (ex: peter@example.com):");
                user.Email = Console.ReadLine().Trim();
                do
                {
                    // validates email address
                    if (!Regex.IsMatch(user.Email, regex, RegexOptions.IgnoreCase))
                    {
                        Console.WriteLine("Email address is not valid, Please enter valid email:");
                        user.Email = Console.ReadLine().Trim();
                    }
                }
                while (!Regex.IsMatch(user.Email, regex, RegexOptions.IgnoreCase));
                Console.WriteLine("Enter your password (ex: peter@123):");
                user.Password = Console.ReadLine().Trim();
                Console.WriteLine("Select your user role:");
                Console.WriteLine($"1. {Roles.lead}");
                Console.WriteLine($"2. {Roles.member}");
                string role = Console.ReadLine().Trim();
                user.Role = (role == "1") ? Roles.lead : Roles.member;

                // calls the api
                var result = await _authService.UserRegisterAsync(user);
                if (result.Success)
                {
                    Console.WriteLine();
                    Console.WriteLine("Successful, Please login now.");
                    await Login();
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Error, Please try again.");
                    await Register();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error, Please try again.");
                _logger.LogError(ex.Message);
                await Register();
            }
        }

        /// <summary>
        /// Forgot password process.
        /// </summary>
        private async Task ForgotPassword()
        {
            try
            {
                Console.WriteLine();
                var user = new ForgotPassword();
                Console.WriteLine("Enter your email to reset password:");
                user.Email = Console.ReadLine().Trim();
                do
                {
                    // validates email address
                    if (!Regex.IsMatch(user.Email, regex, RegexOptions.IgnoreCase))
                    {
                        Console.WriteLine("Email address is not valid, Please enter valid email:");
                        user.Email = Console.ReadLine().Trim();
                    }
                }
                while (!Regex.IsMatch(user.Email, regex, RegexOptions.IgnoreCase));

                // gets reset token
                var result = await _authService.ForgotPasswordAsync(user);
                if (result.Success)
                {
                    // stores password reset token
                    Keys.ResetToken = result.ResetToken;

                    Console.WriteLine("Enter new password (ex: newpass@123):");
                    string newPassword = Console.ReadLine().Trim();
                    Console.WriteLine("Enter confirm password:");
                    string conPassword = Console.ReadLine().Trim();

                    // resets password
                    await ResetPassword(newPassword, conPassword);
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Error, Please try again.");
                    await ForgotPassword();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error, Please try again.");
                _logger.LogError(ex.Message);
                await ForgotPassword();
            }
        }

        /// <summary>
        /// Reset password process.
        /// </summary>
        /// <param name="newPassword">New password to reset the old password.</param>
        /// <param name="conPassword">New confirmation password.</param>
        private async Task ResetPassword(string newPassword, string conPassword)
        {
            try
            {
                // reset password data
                var user = new ResetPassword
                {
                    Password    = newPassword,
                    ConPassword = conPassword,
                    ResetToken  = Keys.ResetToken
                };

                // calls the api
                var result = await _authService.ResetPasswordAsync(user);
                if (result.Success)
                {
                    Console.WriteLine();
                    Console.WriteLine("Successful, Please login.");
                    await Login();
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Error, Please try again.");
                    await ResetPassword(newPassword, conPassword);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error, Please try again.");
                _logger.LogError(ex.Message);
                await ForgotPassword();
            }
        }

        /// <summary>
        /// Login process.
        /// </summary>
        private async Task Login()
        {
            try
            {
                Console.WriteLine();
                var user      = new Login();
                Console.WriteLine("Enter your username:");
                user.UserName = Console.ReadLine().Trim();
                Console.WriteLine("Enter your password:");
                user.Password = Console.ReadLine().Trim();

                // sends the login request
                var result = await _authService.UserLoginAsync(user);
                if (result.Success)
                {
                    // stores user tokens
                    Keys.AccessToken = result.Token;
                    Keys.RefreshToken = result.RefreshToken;

                    // reads JWT token to identify the uer role
                    var handler = new JwtSecurityTokenHandler();
                    Keys.Role = handler.ReadJwtToken(Keys.AccessToken).Payload["role"].ToString();

                    // directs to application internal options
                    Console.WriteLine();
                    Console.WriteLine(("").PadRight(46, '-'));
                    Console.WriteLine("HELLO, WELCOME TO ASSIGNA CONSOLE APPLICATION");
                    Console.WriteLine(("").PadRight(46, '-'));
                    if (Keys.Role == Roles.lead)
                    {
                        await LeadOptions(null);
                    }
                    else
                    {
                        await MemberOptions(null);
                    }
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Username or password is incorrect.");
                    await Login();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error, Please try again.");
                _logger.LogError(ex.Message);
                await Login();
            }
        }

        /// <summary>
        /// Directs to team-lead opetions.
        /// </summary>
        /// <param name="type">The type of opertaion.</param>
        private async Task LeadOptions(int? type)
        {
            if (type is null)
            {
                Console.WriteLine();
                Console.WriteLine("Select option to continue");
                Console.WriteLine(("").PadRight(25, '-'));
                Console.WriteLine("1. View members tasks");
                Console.WriteLine("2. Manage tasks");
                Console.WriteLine("3. Logout");
                Console.WriteLine();
                selection = int.Parse(Console.ReadLine().Trim());
            }
            else
            {
                selection = (int)type;
            }

            switch (selection)
            {
                case 1:
                    Console.WriteLine();
                    Console.WriteLine("Select option to continue");
                    Console.WriteLine(("").PadRight(25, '-'));
                    Console.WriteLine("1. All tasks");
                    Console.WriteLine("2. Pending tasks");
                    Console.WriteLine("3. Completed tasks");
                    Console.WriteLine("4. High priority tasks");
                    Console.WriteLine("5. Medium priority tasks");
                    Console.WriteLine("6. Low priority tasks");
                    Console.WriteLine("7. Task Information");
                    Console.WriteLine("8. Back");
                    Console.WriteLine();
                    selection = int.Parse(Console.ReadLine().Trim());
                    await LeadOperations(1, selection);
                    break;

                case 2:
                    Console.WriteLine();
                    Console.WriteLine("Select option to continue");
                    Console.WriteLine(("").PadRight(25, '-'));
                    Console.WriteLine("1. Add a new task");
                    Console.WriteLine("2. Back");
                    Console.WriteLine();
                    selection = int.Parse(Console.ReadLine().Trim());
                    await LeadOperations(2, selection);
                    break;

                case 3:
                    Console.WriteLine();
                    Console.WriteLine("Are you sure to logout? [Yes / No]");
                    string answer = Console.ReadLine().Trim();
                    if (answer.ToUpper() == "YES")
                    {
                        Console.WriteLine();
                        Console.WriteLine("You are logged out.");
                    }
                    else
                    {
                        await LeadOptions(null);
                    }
                    break;

                default:
                    await LeadOptions(null);
                    break;
            }
        }

        /// <summary>
        /// Directs to team-member opetions.
        /// </summary>
        /// <param name="type">The type of opertaion.</param>
        private async Task MemberOptions(int? type)
        {
            if (type is null)
                selection = 1;
            selection = (int)type;

            switch (selection)
            {
                case 1:
                    Console.WriteLine();
                    Console.WriteLine("Select option to continue");
                    Console.WriteLine(("").PadRight(24, '-'));
                    Console.WriteLine("1. All tasks");
                    Console.WriteLine("2. Pending tasks");
                    Console.WriteLine("3. Completed tasks");
                    Console.WriteLine("4. High priority tasks");
                    Console.WriteLine("5. Medium priority tasks");
                    Console.WriteLine("6. Low priority tasks");
                    Console.WriteLine("7. Task Information");
                    Console.WriteLine("8. Logout");
                    Console.WriteLine();
                    selection = int.Parse(Console.ReadLine().Trim());
                    await MemberOperations(1, selection);
                    break;
            }
        }

        /// <summary>
        /// Manages the team-lead operations.
        /// </summary>
        /// <param name="type">The type of operation.</param>
        /// <param name="selection">The user selected option.</param>
        private async Task LeadOperations(int type, int selection)
        {
            try
            {
                switch (type)
                {
                    case 1:
                        // all tasks
                        if (selection == 1)
                        {
                            // gets the result
                            var result = await _taskService.AllTasks();
                            if (result.Success)
                                await LeadTasks(type, result.Data!, true);
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again.");
                                await LeadOptions(type);
                            }
                        }
                        // pending tasks
                        if (selection == 2)
                        {
                            // gets the result
                            var result = await _taskService.Pendings();
                            if (result.Success)
                                await LeadTasks(type, result.Data!, true);
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again.");
                                await LeadOptions(type);
                            }
                        }
                        // completed tasks
                        if (selection == 3)
                        {
                            // gets the result
                            var result = await _taskService.Completed();
                            if (result.Success)
                                await LeadTasks(type, result.Data!, true);
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again.");
                                await LeadOptions(type);
                            }
                        }
                        // high priority tasks
                        if (selection == 4)
                        {
                            // gets the result
                            var result = await _taskService.HighPriority();
                            if (result.Success)
                                await LeadTasks(type, result.Data!, true);
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again.");
                                await LeadOptions(type);
                            }
                        }
                        // medium priority tasks
                        if (selection == 5)
                        {
                            // gets the result
                            var result = await _taskService.MediumPriority();
                            if (result.Success)
                                await LeadTasks(type, result.Data!, true);
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again.");
                                await LeadOptions(type);
                            }
                        }
                        // low priority tasks
                        if (selection == 6)
                        {
                            // gets the result
                            var result = await _taskService.LowPriority();
                            if (result.Success)
                                await LeadTasks(type, result.Data!, true);
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again.");
                                await LeadOptions(type);
                            }
                        }
                        // task info
                        if (selection == 7)
                        {
                            // gets the result
                            var result = await _taskService.AllTasks();
                            if (result.Success)
                            {

                                await LeadTasks(type, result.Data!, false);

                                Console.WriteLine("1. View information");
                                Console.WriteLine("2. Back");
                                Console.WriteLine();
                                selection = int.Parse(Console.ReadLine().Trim());

                                if (selection == 1)
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Enter task id:");
                                    int id = int.Parse(Console.ReadLine());
                                    Console.WriteLine();

                                    // calls the api
                                    var info = await _taskService.LeadTaskInfo(id);
                                    if (info.Success)
                                    {
                                        // empty data
                                        if (info.Data!.Count <= 0)
                                        {
                                            Console.WriteLine("No data found to display.");
                                            Console.WriteLine();
                                            await LeadOptions(type);
                                        }

                                        var data = info.Data;

                                        // task info
                                        TaskInfo(data);

                                        Console.WriteLine();
                                        Console.WriteLine("1. Edit");
                                        Console.WriteLine("2. Delete");
                                        Console.WriteLine("3. Remind");
                                        Console.WriteLine("4. Back");
                                        Console.WriteLine();
                                        selection = int.Parse(Console.ReadLine().Trim());

                                        // edit, delete, remind options
                                        await LeadTaskActions(type, selection, data);
                                    }
                                    else
                                    {
                                        Console.WriteLine();
                                        Console.WriteLine("Error, Please try again.");
                                        await LeadOptions(type);
                                    }
                                }

                                await LeadOptions(type);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again.");
                                await LeadOptions(type);
                            }
                        }
                        // back
                        if (selection == 8)
                        {
                            await LeadOptions(null);
                        }
                        break;

                    case 2:
                        // new
                        if (selection == 1)
                        {
                            // new task
                            var task = await NewTask();

                            Console.WriteLine("1. Save task");
                            Console.WriteLine("2. Back");
                            Console.WriteLine();
                            selection = int.Parse(Console.ReadLine().Trim());

                            // saves the task
                            if (selection == 1)
                            {
                                // calls the api
                                var result = await _taskService.SaveTaskAsync(task);
                                if (result.Success)
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Successful.");
                                    await LeadOptions(type);
                                }
                                else
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Error, Please try again.");
                                    await LeadOptions(type);
                                }
                            }

                            await LeadOptions(type);
                        }
                        // back
                        if (selection == 2)
                        {
                            await LeadOptions(null);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error, Please try again.");
                _logger.LogError(ex.Message);
                await LeadOptions(null);
            }
        }

        /// <summary>
        /// Manages the team-member operations.
        /// </summary>
        /// <param name="type">The type of operation.</param>
        /// <param name="selection">The type of task data. (e.g. pending)</param>
        private async Task MemberOperations(int type, int selection)
        {
            try
            {
                switch (type)
                {
                    case 1:
                        // all tasks
                        if (selection == 1)
                        {
                            // calls the api
                            var result = await _taskService.AllTasks();
                            if (result.Success)
                                await MemberTasks(type, result.Data!, true);
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again.");
                                await MemberOptions(type);
                            }
                        }
                        // pending tasks
                        if (selection == 2)
                        {
                            // calls the api
                            var result = await _taskService.Pendings();
                            if (result.Success)
                                await MemberTasks(type, result.Data!, true);
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again.");
                                await MemberOptions(type);
                            }
                        }
                        // completed tasks
                        if (selection == 3)
                        {
                            // calls the api
                            var result = await _taskService.Completed();
                            if (result.Success)
                                await MemberTasks(type, result.Data!, true);
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again.");
                                await MemberOptions(type);
                            }
                        }
                        // high priority tasks
                        if (selection == 4)
                        {
                            // calls the api
                            var result = await _taskService.HighPriority();
                            if (result.Success)
                                await MemberTasks(type, result.Data!, true);
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again.");
                                await MemberOptions(type);
                            }
                        }
                        // medium priority tasks
                        if (selection == 5)
                        {
                            // calls the api
                            var result = await _taskService.MediumPriority();
                            if (result.Success)
                                await MemberTasks(type, result.Data!, true);
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again.");
                                await MemberOptions(type);
                            }
                        }
                        // low priority tasks
                        if (selection == 6)
                        {
                            // calls the api
                            var result = await _taskService.LowPriority();
                            if (result.Success)
                                await MemberTasks(type, result.Data!, true);
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again.");
                                await MemberOptions(type);
                            }
                        }
                        // task info
                        if (selection == 7)
                        {
                            // calls the api
                            var result = await _taskService.AllTasks();
                            if (result.Success)
                            {
                                await MemberTasks(type, result.Data!, false);

                                Console.WriteLine("1. View information");
                                Console.WriteLine("2. Back");
                                Console.WriteLine();
                                selection = int.Parse(Console.ReadLine().Trim());

                                if (selection == 1)
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Enter task id:");
                                    int id = int.Parse(Console.ReadLine());
                                    Console.WriteLine();

                                    // calling api
                                    var info = await _taskService.MemberTaskInfo(id);
                                    if (info.Success)
                                    {
                                        // empty data
                                        if (info.Data!.Count <= 0)
                                        {
                                            Console.WriteLine("No data found to display.");
                                            Console.WriteLine();
                                            await MemberOptions(type);
                                        }

                                        var data = info.Data;

                                        // task info
                                        TaskInfo(data);

                                        Console.WriteLine();
                                        Console.WriteLine("1. Add note");
                                        Console.WriteLine("2. Done");
                                        Console.WriteLine("3. Back");
                                        Console.WriteLine();
                                        selection = int.Parse(Console.ReadLine().Trim());

                                        // note, done options
                                        await MemberTaskActions(type, selection, data);
                                    }
                                    else
                                    {
                                        Console.WriteLine();
                                        Console.WriteLine("Error, Please try again.");
                                        await MemberOptions(type);
                                    }
                                }

                                await MemberOptions(type);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again.");
                                await MemberOptions(type);
                            }
                        }
                        if (selection == 8)
                        {
                            Console.WriteLine("Are you sure to logout? [Yes / No]");
                            string answer = Console.ReadLine().Trim();
                            if (answer.ToUpper() == "YES")
                            {
                                Console.WriteLine();
                                Console.WriteLine("You are logged out.");
                            }
                            else
                            {
                                await LeadOptions(null);
                            }
                        }
                        else
                        {
                            await MemberOptions(type);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error, Please try again.");
                _logger.LogError(ex.Message);
                await MemberOptions(null);
            }
        }

        // Helper methods -----------------------------------------------------------------------------------

        /// <summary>
        /// Team-lead tasks table render method.
        /// </summary>
        /// <param name="type">The type of operation.</param>
        /// <param name="data">The data to manage.</param>
        /// <param name="back">Indicates whether the user needs to go back or not.</param>
        private async Task LeadTasks(int type, List<Models.Task> data, bool back)
        {
            // empty data
            if (data!.Count <= 0)
            {
                Console.WriteLine();
                Console.WriteLine("No data found to display.");
                await LeadOptions(type);
            }
            else
            {
                // display data
                Console.WriteLine();
                Console.WriteLine("---------------------------------------------");
                Console.WriteLine("Id  | Task Title               | Deadline    ");
                Console.WriteLine("---------------------------------------------");
                foreach (var itm in data)
                {
                    Console.WriteLine
                    (
                        string.Format
                        (
                            "{0,-3} | {1,-24} | {2,-12}",
                            itm.TaskId, itm.TaskTitle, itm.Deadline.ToString("yyyy-MM-dd")
                        )
                    );
                }
                Console.WriteLine("---------------------------------------------");
                Console.WriteLine();

                // back to options
                if (back)
                {
                    Console.WriteLine("1. Back");
                    Console.WriteLine();

                    Console.ReadLine();
                    await LeadOptions(type);
                }
            }

        }

        /// <summary>
        /// Tema-member tasks table render method.
        /// </summary>
        /// <param name="type">The type of operation.</param>
        /// <param name="data">The data to manage.</param>
        /// <param name="back">Indicates whether the user needs to go back or not.</param>
        private async Task MemberTasks(int type, List<Models.Task> data, bool back)
        {
            // empty data
            if (data!.Count <= 0)
            {
                Console.WriteLine();
                Console.WriteLine("No data found to display.");
                await MemberOptions(type);
            }
            else
            {
                // display data
                Console.WriteLine();
                Console.WriteLine("---------------------------------------------");
                Console.WriteLine("Id  | Task Title               | Deadline    ");
                Console.WriteLine("---------------------------------------------");
                foreach (var itm in data)
                {
                    Console.WriteLine
                    (
                        string.Format
                        (
                            "{0,-3} | {1,-24} | {2,-12}",
                            itm.TaskId, itm.TaskTitle, itm.Deadline.ToString("yyyy-MM-dd")
                        )
                    );
                }
                Console.WriteLine("---------------------------------------------");
                Console.WriteLine();

                // back to options
                if (back)
                {
                    Console.WriteLine("1. Back");
                    Console.WriteLine();

                    Console.ReadLine();
                    await MemberOptions(type);
                }
            }
        }

        /// <summary>
        /// Renders the task info.
        /// </summary>
        /// <param name="data">The data containing the task infomation.</param>
        private void TaskInfo(List<Models.Task> data)
        {
            // display data
            Console.WriteLine($"Id        :    {data.First().TaskId}");
            Console.WriteLine($"Title     :    {data.First().TaskTitle}");
            Console.WriteLine($"Category  :    {data.First().CatName}");
            Console.WriteLine($"Deadline  :    {data.First().Deadline.ToString("yyyy-MM-dd")}");

            string priority = (data.First().HighPriority) ? "High" : (data.First().MediumPriority) ? "Medium" : "Low";

            Console.WriteLine($"Priority  :    {priority}");
            Console.WriteLine($"Assignee  :    {data.First().FirstName}");
            Console.WriteLine($"Note      :    {data.First().TaskNote}");
            string status = (data.First().Pending) ? "Pending" : "Completed";
            Console.WriteLine($"Status    :    {status}");
            string? note = (data.First().UserNote == empty) ? "Not available" : data.First().UserNote;
            Console.WriteLine($"Asi.Note  :    {note}");
        }

        /// <summary>
        /// Makes the task object data.
        /// </summary>
        private async Task<Models.AddTask> NewTask()
        {
            var task = new Models.AddTask();

            try
            {
                Console.WriteLine();
                Console.WriteLine("Enter task title:");
                task.TaskTitle = Console.ReadLine().Trim();
                Console.WriteLine("Select category:");

                // gets the categories
                var category = await _taskService.AllCategories();
                if (category.Success)
                {
                    if (category.Data!.Count > 0)
                        foreach (var itm in category.Data)
                        {
                            Console.WriteLine($"{itm.CatId}. {itm.CatName}");
                        }

                    Console.WriteLine("Error, Enter any number to continue:");
                }
                else
                {
                    Console.WriteLine("Error, Enter any number to continue:");
                }

                task.TaskCategory = int.Parse(Console.ReadLine().Trim());
                Console.WriteLine("Enter due date (yyyy-MM-dd):");
                task.Deadline     = Console.ReadLine().Trim();
                string[] formats  = { "yyyy-MM-dd" };
                bool valid        = false;

                do
                {
                    if (DateTime.TryParseExact(task.Deadline, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                        valid = true;
                    else
                    {
                        Console.WriteLine("Not valid format, Enter due date (yyyy-MM-dd):");
                        task.Deadline = Console.ReadLine().Trim();
                        valid         = false;
                    }

                    date = Convert.ToDateTime(task.Deadline);
                    if (date.Date < DateTime.Now.Date)
                    {
                        Console.WriteLine("Please enter future date:");
                        task.Deadline = Console.ReadLine().Trim();
                        valid         = false;
                    }
                    else
                    {
                        valid = true;
                    }
                }

                while (!valid);
                Console.WriteLine("Select task assignee:");

                // gets the team memebers
                var memeber = await _taskService.TeamMembers();
                if (memeber.Success)
                {
                    if (memeber.Data!.Count > 0)
                        foreach (var itm in memeber.Data)
                        {
                            Console.WriteLine($"{itm.UserId}. {itm.FirstName}");
                        }

                    Console.WriteLine("Error, Enter any number to continue:");
                }
                else
                {
                    Console.WriteLine("Error, Enter any number to continue:");
                }

                task.Member = int.Parse(Console.ReadLine().Trim());
                Console.WriteLine("Select task priority:");

                // gets the task priorities
                var priority = await _taskService.Priorities();
                if (priority.Success)
                {
                    if (priority.Data!.Count > 0)
                        foreach (var itm in priority.Data)
                        {
                            Console.WriteLine($"- {itm.PriorityName}");
                        }

                    Console.WriteLine("Error, Enter High, Medium or Low to continue:");
                }
                else
                {
                    Console.WriteLine("Error, Enter High, Medium or Low to continue:");
                }

                task.Priority = Console.ReadLine().Trim();
                task.Priority = $"{char.ToUpper(task.Priority[0])}{task.Priority.Substring(1)}";
                Console.WriteLine("Enter task note:");
                task.TaskNote = Console.ReadLine().Trim();
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error, Please try again.");
                _logger.LogError(ex.Message);
                await LeadOptions(null);
            }

            return task;
        }

        /// <summary>
        /// Manages the edit, delete, remind operations.
        /// </summary>
        /// <param name="type">The type of operation.</param>
        /// <param name="selection">The user selected option.</param>
        /// <param name="data">The data containing the task infomation.</param>
        private async Task LeadTaskActions(int type, int selection, List<Models.Task> data)
        {
            try
            {
                Console.WriteLine();

                switch (selection)
                {
                    // edit task
                    case 1:
                        if (data.First().Complete)
                        {
                            Console.WriteLine("Already completed.");
                            await MemberOptions(type);
                        }

                        // edit task data
                        var task = new Models.EditTask
                        {
                            TaskId = data.First().TaskId
                        };

                        Console.WriteLine("NOTE: KEEP THE SAME VALUE, PRESS [-]");
                        Console.WriteLine();
                        Console.WriteLine("Enter task title:");
                        task.TaskTitle = Console.ReadLine().Trim();

                        if (task.TaskTitle == "-")
                            task.TaskTitle = data.First().TaskTitle;

                        Console.WriteLine("Select category:");

                        // gets all categories
                        var category = await _taskService.AllCategories();
                        if (category.Success)
                        {
                            if (category.Data!.Count > 0)
                                foreach (var itm in category.Data)
                                {
                                    Console.WriteLine($"{itm.CatId}. {itm.CatName}");
                                }

                            Console.WriteLine("Error, Enter any number to continue:");
                        }
                        else
                        {
                            Console.WriteLine("Error, Enter any number to continue:");
                        }

                        string value = Console.ReadLine().Trim();
                        if (value == "-")
                            task.TaskCategory = data.First().CatId;
                        task.TaskCategory = int.Parse(value);

                        Console.WriteLine("Enter due date (yyyy-MM-dd):");

                        task.Deadline = Console.ReadLine().Trim();
                        if (task.Deadline == "-")
                            task.Deadline = data.First().Deadline.ToString("yyyy-MM-dd");

                        string[] formats = { "yyyy-MM-dd" };
                        bool valid;
                        do
                        {
                            if (DateTime.TryParseExact(task.Deadline, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                                valid = true;
                            else
                            {
                                Console.WriteLine("Not valid format, Enter due date (yyyy-MM-dd):");
                                task.Deadline = Console.ReadLine().Trim();
                                valid = false;
                            }

                            date = Convert.ToDateTime(task.Deadline);
                            if (date.Date < DateTime.Now.Date)
                            {
                                Console.WriteLine("Please enter future date:");
                                task.Deadline = Console.ReadLine().Trim();
                                valid = false;
                            }
                            else
                            {
                                valid = true;
                            }
                        }

                        while (!valid);
                        Console.WriteLine("Select task assignee:");

                        // gets team members
                        var memeber = await _taskService.TeamMembers();
                        if (memeber.Success)
                        {
                            if (memeber.Data!.Count > 0)
                                foreach (var itm in memeber.Data)
                                {
                                    Console.WriteLine($"{itm.UserId}. {itm.FirstName}");
                                }

                            Console.WriteLine("Error, Enter any number to continue:");
                        }
                        else
                        {
                            Console.WriteLine("Error, Enter any number to continue:");
                        }

                        value = Console.ReadLine().Trim();
                        if (value == "-")
                            task.Member = data.First().UserId;
                        task.Member = int.Parse(value);

                        Console.WriteLine("Select task priority:");

                        // gets task priorities
                        var priority = await _taskService.Priorities();
                        if (priority.Success)
                        {
                            if (priority.Data!.Count > 0)
                                foreach (var itm in priority.Data)
                                {
                                    Console.WriteLine($"- {itm.PriorityName}");
                                }

                            Console.WriteLine("Error, Enter High, Medium or Low to continue:");
                        }
                        else
                        {
                            Console.WriteLine("Error, Enter High, Medium or Low to continue:");
                        }

                        task.Priority = Console.ReadLine().Trim();
                        if (task.Priority == "-")
                            task.Priority = (data.First().HighPriority) ? "High" : (data.First().MediumPriority) ? "Medium" : "Low";

                        task.Priority = $"{char.ToUpper(task.Priority[0])}{task.Priority.Substring(1)}";

                        Console.WriteLine("Enter task note:");

                        task.TaskNote = Console.ReadLine().Trim();
                        if (task.TaskNote == "-")
                            task.TaskNote = data.First().TaskNote;

                        Console.WriteLine();

                        Console.WriteLine("1. Save");
                        Console.WriteLine("2. Back");
                        Console.WriteLine();

                        selection = int.Parse(Console.ReadLine().Trim());
                        if (selection == 1)
                        {
                            // calls the api
                            var edit = await _taskService.EditTaskAsync(task);
                            if (edit.Success)
                            {
                                Console.WriteLine();
                                Console.WriteLine("Successful.");
                                await LeadOptions(type);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again.");
                                await LeadOptions(type);
                            }
                        }

                        await LeadOptions(type);

                        break;
                    // delete task
                    case 2:
                        var delete = new Models.DeleteTask();

                        Console.WriteLine("Enter task id to delete");
                        delete.TaskId = int.Parse(Console.ReadLine().Trim());
                        Console.WriteLine();

                        Console.WriteLine("1. Delete");
                        Console.WriteLine("2. Back");
                        Console.WriteLine();
                        selection = int.Parse(Console.ReadLine().Trim());

                        if (selection == 1)
                        {
                            // calls the api
                            var result = await _taskService.DeleteTaskAsync(delete);
                            if (result.Success)
                            {
                                Console.WriteLine();
                                Console.WriteLine("Successful.");
                                await LeadOptions(type);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again.");
                                await LeadOptions(type);
                            }
                        }

                        await LeadOptions(type);

                        break;
                    // send remind
                    case 3:
                        var remind = new Models.SendEmail();

                        Console.WriteLine("Enter task id:");
                        remind.TaskId = int.Parse(Console.ReadLine().Trim());
                        Console.WriteLine("Enter your message:");
                        remind.Message = Console.ReadLine().Trim();
                        Console.WriteLine();

                        Console.WriteLine("1. Send");
                        Console.WriteLine("2. Back");
                        Console.WriteLine();
                        selection = int.Parse(Console.ReadLine().Trim());
                        
                        if (selection == 1)
                        {
                            // calls the api
                            var result = await _taskService.SendRemind(remind);
                            if (result.Success)
                            {
                                Console.WriteLine();
                                Console.WriteLine("Successful.");
                                await LeadOptions(type);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again.");
                                await LeadOptions(type);
                            }
                        }

                        await LeadOptions(type);

                        break;
                    default:
                        await LeadOptions(type);
                        break;
                }
            }
            
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error, Please try again.");
                _logger.LogError(ex.Message);
                await LeadOptions(null);
            }
        }

        /// <summary>
        /// Manages  add note and mark as done operations.
        /// </summary>
        /// <param name="type">The type of operation.</param>
        /// <param name="selection">The user selected option.</param>
        /// <param name="data">The data containing the task infomation.</param>
        private async Task MemberTaskActions(int type, int selection, List<Models.Task> data)
        {
            try
            {

                switch (selection)
                {
                    // add note
                    case 1:
                        if (data.First().Complete)
                        {
                            Console.WriteLine("Already completed.");
                            await MemberOptions(type);
                        }

                        // note data
                        var note = new Models.AddNote
                        {
                            TaskId = data.First().TaskId
                        };

                        Console.WriteLine();
                        Console.WriteLine("Enter your note:");
                        note.UserNote = Console.ReadLine().Trim();
                        Console.WriteLine();

                        Console.WriteLine("1. Add note");
                        Console.WriteLine("2. Back");
                        Console.WriteLine();
                        selection = int.Parse(Console.ReadLine().Trim());
                       
                        if (selection == 1)
                        {
                            // calls the api
                            var edit = await _taskService.AddTaskNoteAsync(note);
                            if (edit.Success)
                            {
                                Console.WriteLine();
                                Console.WriteLine("Successful.");
                                await MemberOptions(type);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again.");
                                await MemberOptions(type);
                            }
                        }

                        await MemberOptions(type);

                        break;

                    // mark done
                    case 2:

                        // mark as done data
                        var done = new Models.MarkDone
                        {
                            TaskId = data.First().TaskId
                        };

                        Console.WriteLine();
                        Console.WriteLine("Are you sure to mark as done? [Yes / No]");
                        string answer = Console.ReadLine().Trim();
                        Console.WriteLine();
                        if (answer.ToUpper() == "YES")
                        {
                            Console.WriteLine("1. Done");
                            Console.WriteLine("2. Back");
                            Console.WriteLine();
                            selection = int.Parse(Console.ReadLine().Trim());

                            if (selection == 1)
                            {
                                // calls the api
                                var mark = await _taskService.MarkasDoneAsync(done);
                                if (mark.Success)
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Successful.");
                                    await MemberOptions(type);
                                }
                                else
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Error, Please try again.");
                                    await MemberOptions(type);
                                }
                            }
                        }
                        else
                        {
                            await MemberOptions(type);
                        }

                        await MemberOptions(type);

                        break;
                    default:
                        await MemberOptions(type);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error, Please try again.");
                _logger.LogError(ex.Message);
                await MemberOptions(null);
            }
        }
    }
}