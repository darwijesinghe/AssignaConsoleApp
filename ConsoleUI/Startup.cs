using ConsoleUI.Auth;
using ConsoleUI.Interfaces;
using ConsoleUI.Models;
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
        // services
        private ILogger<Startup> Logger { get; }
        private IAuthService AuthService { get; }
        private ITaskService TaskService { get; }

        public Startup(ILogger<Startup> logger, IAuthService authService,
            ITaskService taskService)
        {
            Logger = logger;
            AuthService = authService;
            TaskService = taskService;
        }

        // variables
        private int selection = 0;
        const string empty = "";
        private const string regex = @"^[^@\s]+@[^@\s]+\.(com|net|org|gov)$";

        // start
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
                        await this.Register();
                        break;

                    case 2: // login
                        await this.Login();
                        break;

                    case 3: // forgot password
                        await this.ForgotPassword();
                        break;

                    default: // logout
                        Console.WriteLine("Are you sure to logout? [Yes / No]");
                        string answer = Console.ReadLine().Trim();
                        if (answer.ToUpper() == "YES")
                        {
                            Console.WriteLine();
                            Console.WriteLine("You are logged out");
                        }
                        else
                        {
                            Console.WriteLine();
                            await this.Run();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error, Please try again");
                Logger.LogError(ex.StackTrace);
                await this.Run();
            }
        }

        // registration
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
                    // validate username
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
                    // validate email address
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

                // api calling
                var result = await AuthService.UserRegisterAsync(user);
                if (result.Success)
                {
                    Console.WriteLine();
                    Console.WriteLine("Successful, Please login now");
                    await this.Login();
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Error, Please try again");
                    await this.Register();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error, Please try again");
                Logger.LogError(ex.Message);
                await this.Register();
            }
        }

        // forgot password
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
                    // validate email address
                    if (!Regex.IsMatch(user.Email, regex, RegexOptions.IgnoreCase))
                    {
                        Console.WriteLine("Email address is not valid, Please enter valid email:");
                        user.Email = Console.ReadLine().Trim();
                    }
                }
                while (!Regex.IsMatch(user.Email, regex, RegexOptions.IgnoreCase));

                // get reset token
                var result = await AuthService.ForgotPasswordAsync(user);
                if (result.Success)
                {
                    // store password reset token
                    Keys.ResetToken = result.ResetToken;

                    Console.WriteLine("Enter new password (ex: newpass@123):");
                    string newPassword = Console.ReadLine().Trim();
                    Console.WriteLine("Enter confirm password:");
                    string conPassword = Console.ReadLine().Trim();

                    // reset password
                    await this.ResetPassword(newPassword, conPassword);
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Error, Please try again");
                    await this.ForgotPassword();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error, Please try again");
                Logger.LogError(ex.Message);
                await this.ForgotPassword();
            }
        }

        // reset password
        private async Task ResetPassword(string newPassword, string conPassword)
        {
            try
            {
                var user = new ResetPassword
                {
                    Password = newPassword,
                    ConPassword = conPassword,
                    ResetToken = Keys.ResetToken
                };

                // api calling
                var result = await AuthService.ResetPasswordAsync(user);
                if (result.Success)
                {
                    Console.WriteLine();
                    Console.WriteLine("Successful, Please login");
                    await this.Login();
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Error, Please try again");
                    await this.ResetPassword(newPassword, conPassword);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error, Please try again");
                Logger.LogError(ex.Message);
                await this.ForgotPassword();
            }
        }

        // login
        private async Task Login()
        {
            try
            {
                Console.WriteLine();
                var user = new Login();
                Console.WriteLine("Enter your username:");
                user.UserName = Console.ReadLine().Trim();
                Console.WriteLine("Enter your password:");
                user.Password = Console.ReadLine().Trim();

                // login request
                var result = await AuthService.UserLoginAsync(user);
                if (result.Success)
                {
                    // store user tokens
                    Keys.AccessToken = result.Token;
                    Keys.RefreshToken = result.RefreshToken;

                    // read JWT token to identify the uer role
                    var handler = new JwtSecurityTokenHandler();
                    Keys.Role = handler.ReadJwtToken(Keys.AccessToken).Payload["role"].ToString();

                    // direct to application internal options
                    Console.WriteLine();
                    Console.WriteLine(("").PadRight(46, '-'));
                    Console.WriteLine("HELLO, WELCOME TO ASSIGNA CONSOLE APPLICATION");
                    Console.WriteLine(("").PadRight(46, '-'));
                    if (Keys.Role == Roles.lead)
                    {

                        await this.LeadOptions(null);
                    }
                    else
                    {
                        await this.MemberOptions(null);
                    }
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Username or password is incorrect");
                    await this.Login();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error, Please try again");
                Logger.LogError(ex.Message);
                await this.Login();
            }
        }

        // lead options
        /// <summary>
        /// we use retry param for when the sub operations would be fail,
        /// then we need user to give retry the options
        /// </summary>
        /// <param name="retry"></param>
        /// <returns></returns>
        private async Task LeadOptions(int? retry)
        {
            if (retry is null)
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
                selection = (int)retry;
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
                    this.selection = int.Parse(Console.ReadLine().Trim());
                    await this.LeadOperations(1, this.selection);
                    break;

                case 2:
                    Console.WriteLine();
                    Console.WriteLine("Select option to continue");
                    Console.WriteLine(("").PadRight(25, '-'));
                    Console.WriteLine("1. Add a new task");
                    Console.WriteLine("2. Back");
                    Console.WriteLine();
                    this.selection = int.Parse(Console.ReadLine().Trim());
                    await this.LeadOperations(2, this.selection);
                    break;

                case 3:
                    Console.WriteLine();
                    Console.WriteLine("Are you sure to logout? [Yes / No]");
                    string answer = Console.ReadLine().Trim();
                    if (answer.ToUpper() == "YES")
                    {
                        Console.WriteLine();
                        Console.WriteLine("You are logged out");
                    }
                    else
                    {
                        await this.LeadOptions(null);
                    }
                    break;

                default:
                    await this.LeadOptions(null);
                    break;
            }
        }

        // member options
        /// <summary>
        /// we use retry param for when the sub operations would be fail,
        /// then we need user to give retry the options
        /// </summary>
        /// <param name="retry"></param>
        /// <returns></returns>
        private async Task MemberOptions(int? retry)
        {
            if (retry is null)
            {
                selection = 1;
            }
            else
            {
                selection = (int)retry;
            }

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
                    this.selection = int.Parse(Console.ReadLine().Trim());
                    await this.MemberOperations(1, this.selection);
                    break;
            }
        }

        // lead operations
        private async Task LeadOperations(int group, int selection)
        {
            try
            {
                switch (group)
                {
                    case 1:
                        // all
                        if (selection == 1)
                        {
                            var result = await TaskService.AllTasks();
                            if (result.Success)
                            {
                                await this.LeadTasks(group, result.Data!, true);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again");
                                await this.LeadOptions(group);
                            }
                        }
                        // pendings
                        if (selection == 2)
                        {
                            var result = await TaskService.Pendings();
                            if (result.Success)
                            {
                                await this.LeadTasks(group, result.Data!, true);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again");
                                await this.LeadOptions(group);
                            }
                        }
                        // completed
                        if (selection == 3)
                        {
                            var result = await TaskService.Completed();
                            if (result.Success)
                            {
                                await this.LeadTasks(group, result.Data!, true);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again");
                                await this.LeadOptions(group);
                            }
                        }
                        // high
                        if (selection == 4)
                        {
                            var result = await TaskService.HighPriority();
                            if (result.Success)
                            {
                                await this.LeadTasks(group, result.Data!, true);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again");
                                await this.LeadOptions(group);
                            }
                        }
                        // medium
                        if (selection == 5)
                        {
                            var result = await TaskService.MediumPriority();
                            if (result.Success)
                            {

                                await this.LeadTasks(group, result.Data!, true);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again");
                                await this.LeadOptions(group);
                            }
                        }
                        // low
                        if (selection == 6)
                        {
                            var result = await TaskService.LowPriority();
                            if (result.Success)
                            {
                                await this.LeadTasks(group, result.Data!, true);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again");
                                await this.LeadOptions(group);
                            }
                        }
                        // info
                        if (selection == 7)
                        {
                            var result = await TaskService.AllTasks();
                            if (result.Success)
                            {

                                await this.LeadTasks(group, result.Data!, false);

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
                                    var info = await TaskService.LeadTaskInfo(id);
                                    if (info.Success)
                                    {
                                        // empty data
                                        if (info.Data!.Count <= 0)
                                        {
                                            Console.WriteLine("No data found to display");
                                            Console.WriteLine();
                                            await this.LeadOptions(group);
                                        }

                                        var data = info.Data;

                                        // task info
                                        this.TaskInfo(data);

                                        Console.WriteLine();
                                        Console.WriteLine("1. Edit");
                                        Console.WriteLine("2. Delete");
                                        Console.WriteLine("3. Remind");
                                        Console.WriteLine("4. Back");
                                        Console.WriteLine();
                                        selection = int.Parse(Console.ReadLine().Trim());

                                        // edit, delete, remind
                                        await LeadTaskActions(group, selection, data);
                                    }
                                    else
                                    {
                                        Console.WriteLine();
                                        Console.WriteLine("Error, Please try again");
                                        await this.LeadOptions(group);
                                    }
                                }

                                await this.LeadOptions(group);

                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again");
                                await this.LeadOptions(group);
                            }
                        }
                        // back
                        if (selection == 8)
                        {
                            await this.LeadOptions(null);
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

                            // save task
                            if (selection == 1)
                            {
                                // calling api
                                var result = await TaskService.SaveTaskAsync(task);
                                if (result.Success)
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Successful");
                                    await this.LeadOptions(group);
                                }
                                else
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Error, Please try again");
                                    await this.LeadOptions(group);
                                }
                            }

                            await this.LeadOptions(group);
                        }
                        // back
                        if (selection == 2)
                        {
                            await this.LeadOptions(null);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error, Please try again");
                Logger.LogError(ex.Message);
                await this.LeadOptions(null);
            }
        }

        // member operations
        private async Task MemberOperations(int group, int selection)
        {
            try
            {
                switch (group)
                {
                    case 1:
                        // all
                        if (selection == 1)
                        {
                            var result = await TaskService.AllTasks();
                            if (result.Success)
                            {

                                // all tasks
                                await this.MemberTasks(group, result.Data!, true);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again");
                                await this.MemberOptions(group);
                            }
                        }
                        // pendings
                        if (selection == 2)
                        {
                            var result = await TaskService.Pendings();
                            if (result.Success)
                            {

                                await this.MemberTasks(group, result.Data!, true);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again");
                                await this.MemberOptions(group);
                            }
                        }
                        // completed
                        if (selection == 3)
                        {
                            var result = await TaskService.Completed();
                            if (result.Success)
                            {

                                await this.MemberTasks(group, result.Data!, true);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again");
                                await this.MemberOptions(group);
                            }
                        }
                        // high
                        if (selection == 4)
                        {
                            var result = await TaskService.HighPriority();
                            if (result.Success)
                            {
                                await this.MemberTasks(group, result.Data!, true);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again");
                                await this.MemberOptions(group);
                            }
                        }
                        // medium
                        if (selection == 5)
                        {
                            var result = await TaskService.MediumPriority();
                            if (result.Success)
                            {
                                await this.MemberTasks(group, result.Data!, true);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again");
                                await this.MemberOptions(group);
                            }
                        }
                        // low
                        if (selection == 6)
                        {
                            var result = await TaskService.LowPriority();
                            if (result.Success)
                            {
                                await this.MemberTasks(group, result.Data!, true);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again");
                                await this.MemberOptions(group);
                            }
                        }
                        // info
                        if (selection == 7)
                        {
                            var result = await TaskService.AllTasks();
                            if (result.Success)
                            {

                                await this.MemberTasks(group, result.Data!, false);

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
                                    var info = await TaskService.MemberTaskInfo(id);
                                    if (info.Success)
                                    {
                                        // empty data
                                        if (info.Data!.Count <= 0)
                                        {
                                            Console.WriteLine("No data found to display");
                                            Console.WriteLine();
                                            await this.MemberOptions(group);
                                        }

                                        var data = info.Data;

                                        // task info
                                        this.TaskInfo(data);

                                        Console.WriteLine();
                                        Console.WriteLine("1. Add note");
                                        Console.WriteLine("2. Done");
                                        Console.WriteLine("3. Back");
                                        Console.WriteLine();
                                        selection = int.Parse(Console.ReadLine().Trim());

                                        // note, done
                                        await MemberTaskActions(group, selection, data);
                                    }
                                    else
                                    {
                                        Console.WriteLine();
                                        Console.WriteLine("Error, Please try again");
                                        await this.MemberOptions(group);
                                    }
                                }

                                await this.MemberOptions(group);

                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again");
                                await this.MemberOptions(group);
                            }
                        }
                        if (selection == 8)
                        {
                            Console.WriteLine("Are you sure to logout? [Yes / No]");
                            string answer = Console.ReadLine().Trim();
                            if (answer.ToUpper() == "YES")
                            {
                                Console.WriteLine();
                                Console.WriteLine("You are logged out");
                            }
                            else
                            {
                                await this.LeadOptions(null);
                            }
                        }
                        else
                        {
                            await this.MemberOptions(group);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error, Please try again");
                Logger.LogError(ex.Message);
                await this.MemberOptions(null);
            }
        }

        #region helper methods

        // tasks table render method
        private async Task LeadTasks(int group, List<Tasks> data, bool back)
        {
            // empty data
            if (data!.Count <= 0)
            {
                Console.WriteLine();
                Console.WriteLine("No data found to display");
                await this.LeadOptions(group);
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
                        String.Format
                        (
                            "{0,-3} | {1,-24} | {2,-12}",
                            itm.TskId, itm.TskTitle, itm.Deadline.ToString("yyyy-MM-dd")
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
                    await this.LeadOptions(group);
                }
            }

        }
        private async Task MemberTasks(int group, List<Tasks> data, bool back)
        {
            // empty data
            if (data!.Count <= 0)
            {
                Console.WriteLine();
                Console.WriteLine("No data found to display");
                await this.MemberOptions(group);
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
                        String.Format
                        (
                            "{0,-3} | {1,-24} | {2,-12}",
                            itm.TskId, itm.TskTitle, itm.Deadline.ToString("yyyy-MM-dd")
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
                    await this.MemberOptions(group);
                }
            }

        }

        // task info
        void TaskInfo(List<Tasks> data)
        {
            // display data
            Console.WriteLine($"Id        :    {data.First().TskId}");
            Console.WriteLine($"Title     :    {data.First().TskTitle}");
            Console.WriteLine($"Category  :    {data.First().CatName}");
#pragma warning disable IDE0071 // Simplify interpolation
            Console.WriteLine($"Deadline  :    {data.First().Deadline.ToString("yyyy-MM-dd")}");
#pragma warning restore IDE0071 // Simplify interpolation
            string priority = (data.First().PriHigh) ? "High" : (data.First().PriMedium) ? "Medium" : "Low";
            Console.WriteLine($"Priority  :    {priority}");
            Console.WriteLine($"Assignee  :    {data.First().FirstName}");
            Console.WriteLine($"Note      :    {data.First().TskNote}");
            string status = (data.First().Pending) ? "Pending" : "Completed";
            Console.WriteLine($"Status    :    {status}");
            string? note = (data.First().UserNote == empty) ? "Not available" : data.First().UserNote;
            Console.WriteLine($"Asi.Note  :    {note}");
        }

        // new task
        private async Task<AddTask> NewTask()
        {
            var task = new AddTask();
            try
            {
                Console.WriteLine();
                Console.WriteLine("Enter task title:");
                task.TskTitle = Console.ReadLine().Trim();
                Console.WriteLine("Select category:");
                var category = await TaskService.AllCategories();
                if (category.Success)
                {
                    if (category.Data!.Count > 0)
                    {
                        foreach (var itm in category.Data)
                        {
                            Console.WriteLine($"{itm.CatId}. {itm.CatName}");
                        }

                    }
                    else
                    {
                        Console.WriteLine("Error, Enter any number to continue:");
                    }
                }
                else
                {
                    Console.WriteLine("Error, Enter any number to continue:");
                }
                task.TskCategory = int.Parse(Console.ReadLine().Trim());
                Console.WriteLine("Enter due date (yyyy-MM-dd):");
                task.Deadline = Console.ReadLine().Trim();
                string[] formats = { "yyyy-MM-dd" };
                bool valid = false;
                do
                {
                    if (DateTime.TryParseExact(task.Deadline, formats,
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                    {
                        valid = true;
                    }
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
                var memeber = await TaskService.TeamMembers();
                if (memeber.Success)
                {
                    if (memeber.Data!.Count > 0)
                    {
                        foreach (var itm in memeber.Data)
                        {
                            Console.WriteLine($"{itm.UserId}. {itm.FirstName}");
                        }

                    }
                    else
                    {
                        Console.WriteLine("Error, Enter any number to continue:");
                    }
                }
                else
                {
                    Console.WriteLine("Error, Enter any number to continue:");
                }
                task.Member = int.Parse(Console.ReadLine().Trim());
                Console.WriteLine("Select task priority:");
                var priority = await TaskService.Priorities();
                if (priority.Success)
                {
                    if (priority.Data!.Count > 0)
                    {
                        foreach (var itm in priority.Data)
                        {
                            Console.WriteLine($"- {itm.PriName}");
                        }

                    }
                    else
                    {
                        Console.WriteLine("Error, Enter High, Medium or Low to continue:");
                    }
                }
                else
                {
                    Console.WriteLine("Error, Enter High, Medium or Low to continue:");
                }
                task.Priority = Console.ReadLine().Trim();
#pragma warning disable IDE0057 // Use range operator
                task.Priority = $"{char.ToUpper(task.Priority[0])}{task.Priority.Substring(1)}";
#pragma warning restore IDE0057 // Use range operator
                Console.WriteLine("Enter task note:");
                task.TskNote = Console.ReadLine().Trim();
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error, Please try again");
                Logger.LogError(ex.Message);
                await this.LeadOptions(null);
            }

            return task;
        }

        // edit, delete, remind task
        private async Task LeadTaskActions(int group, int selection, List<Tasks> data)
        {
            try
            {
                Console.WriteLine();

                switch (selection)
                {
                    // edit
                    case 1:
                        if (data.First().Complete)
                        {
                            Console.WriteLine("Already completed");
                            await this.MemberOptions(group);
                        }
                        var task = new EditTask
                        {
                            TskId = data.First().TskId
                        };
                        Console.WriteLine("NOTE: KEEP THE SAME VALUE, PRESS [-]");
                        Console.WriteLine();
                        Console.WriteLine("Enter task title:");
                        task.TskTitle = Console.ReadLine().Trim();
                        if (task.TskTitle == "-")
                        {
                            task.TskTitle = data.First().TskTitle;
                        }                    
                        Console.WriteLine("Select category:");
                        var category = await TaskService.AllCategories();
                        if (category.Success)
                        {
                            if (category.Data!.Count > 0)
                            {
                                foreach (var itm in category.Data)
                                {
                                    Console.WriteLine($"{itm.CatId}. {itm.CatName}");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Error, Enter any number to continue:");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error, Enter any number to continue:");
                        }
                        string value = Console.ReadLine().Trim();
                        if (value == "-")
                        {
                            task.TskCategory = data.First().CatId;
                        }
                        else
                        {
                            task.TskCategory = int.Parse(value);
                        }
                        Console.WriteLine("Enter due date (yyyy-MM-dd):");
                        task.Deadline = Console.ReadLine().Trim();
                        if (task.Deadline == "-")
                        {
                            task.Deadline = data.First().Deadline.ToString("yyyy-MM-dd");
                        }
                        string[] formats = { "yyyy-MM-dd" };
                        bool valid;
                        do
                        {
                            if (DateTime.TryParseExact(task.Deadline, formats,
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                            {
                                valid = true;
                            }
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
                        var memeber = await TaskService.TeamMembers();
                        if (memeber.Success)
                        {
                            if (memeber.Data!.Count > 0)
                            {
                                foreach (var itm in memeber.Data)
                                {
                                    Console.WriteLine($"{itm.UserId}. {itm.FirstName}");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Error, Enter any number to continue:");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error, Enter any number to continue:");
                        }
                        value = Console.ReadLine().Trim();
                        if (value == "-")
                        {
                            task.Member = data.First().UserId;
                        }
                        else
                        {
                            task.Member = int.Parse(value);
                        }
                        Console.WriteLine("Select task priority:");
                        var priority = await TaskService.Priorities();
                        if (priority.Success)
                        {
                            if (priority.Data!.Count > 0)
                            {
                                foreach (var itm in priority.Data)
                                {
                                    Console.WriteLine($"- {itm.PriName}");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Error, Enter High, Medium or Low to continue:");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error, Enter High, Medium or Low to continue:");
                        }
                        task.Priority = Console.ReadLine().Trim();
                        if (task.Priority == "-")
                        {
                            task.Priority = (data.First().PriHigh) ? "High" : (data.First().PriMedium) ? "Medium" : "Low";
                        }
#pragma warning disable IDE0057 // Use range operator
                        task.Priority = $"{char.ToUpper(task.Priority[0])}{task.Priority.Substring(1)}";
#pragma warning restore IDE0057 // Use range operator
                        Console.WriteLine("Enter task note:");
                        task.TskNote = Console.ReadLine().Trim();
                        if (task.TskNote == "-")
                        {
                            task.TskNote = data.First().TskNote;
                        }
                        Console.WriteLine();

                        Console.WriteLine("1. Save");
                        Console.WriteLine("2. Back");
                        Console.WriteLine();
                        selection = int.Parse(Console.ReadLine().Trim());

                        // calling api
                        if (selection == 1)
                        {
                            var edit = await TaskService.EditTaskAsync(task);
                            if (edit.Success)
                            {
                                Console.WriteLine();
                                Console.WriteLine("Successful");
                                await this.LeadOptions(group);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again");
                                await this.LeadOptions(group);
                            }
                        }

                        await this.LeadOptions(group);

                        break;
                    // delete
                    case 2:
                        var delete = new DeleteTask();
                        Console.WriteLine("Enter task id to delete");
                        delete.TskId = int.Parse(Console.ReadLine().Trim());
                        Console.WriteLine();

                        Console.WriteLine("1. Delete");
                        Console.WriteLine("2. Back");
                        Console.WriteLine();
                        selection = int.Parse(Console.ReadLine().Trim());

                        // calling api
                        if (selection == 1)
                        {
                            var result = await TaskService.DeleteTaskAsync(delete);
                            if (result.Success)
                            {
                                Console.WriteLine();
                                Console.WriteLine("Successful");
                                await this.LeadOptions(group);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again");
                                await this.LeadOptions(group);
                            }
                        }

                        await this.LeadOptions(group);

                        break;
                    // remind
                    case 3:
                        var remind = new SendEmail();
                        Console.WriteLine("Enter task id:");
                        remind.TskId = int.Parse(Console.ReadLine().Trim());
                        Console.WriteLine("Enter your message:");
                        remind.Message = Console.ReadLine().Trim();
                        Console.WriteLine();

                        Console.WriteLine("1. Send");
                        Console.WriteLine("2. Back");
                        Console.WriteLine();
                        selection = int.Parse(Console.ReadLine().Trim());

                        // calling api
                        if (selection == 1)
                        {
                            var result = await TaskService.SendRemind(remind);
                            if (result.Success)
                            {
                                Console.WriteLine();
                                Console.WriteLine("Successful");
                                await this.LeadOptions(group);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again");
                                await this.LeadOptions(group);
                            }
                        }

                        await this.LeadOptions(group);

                        break;
                    default:
                        await this.LeadOptions(group);
                        break;
                }
            }
            
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error, Please try again");
                Logger.LogError(ex.Message);
                await this.LeadOptions(null);
            }
        }

        // add note and mark as done
        private async Task MemberTaskActions(int group, int selection, List<Tasks> data)
        {
            try
            {

                switch (selection)
                {
                    // add note
                    case 1:
                        if (data.First().Complete)
                        {
                            Console.WriteLine("Already completed");
                            await this.MemberOptions(group);
                        }
                        var note = new AddNote
                        {
                            TskId = data.First().TskId
                        };
                        Console.WriteLine();
                        Console.WriteLine("Enter your note:");
                        note.UserNote = Console.ReadLine().Trim();
                        Console.WriteLine();

                        Console.WriteLine("1. Add note");
                        Console.WriteLine("2. Back");
                        Console.WriteLine();
                        selection = int.Parse(Console.ReadLine().Trim());

                        // calling api
                        if (selection == 1)
                        {
                            var edit = await TaskService.AddTaskNoteAsync(note);
                            if (edit.Success)
                            {
                                Console.WriteLine();
                                Console.WriteLine("Successful");
                                await this.MemberOptions(group);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error, Please try again");
                                await this.MemberOptions(group);
                            }
                        }

                        await this.MemberOptions(group);

                        break;
                    // mark done
                    case 2:
                        var done = new MarkDone
                        {
                            TskId = data.First().TskId
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

                            // calling api
                            if (selection == 1)
                            {
                                var edit = await TaskService.MarkasDoneAsync(done);
                                if (edit.Success)
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Successful");
                                    await this.MemberOptions(group);
                                }
                                else
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Error, Please try again");
                                    await this.MemberOptions(group);
                                }
                            }
                        }
                        else
                        {
                            await this.MemberOptions(group);
                        }

                        await this.MemberOptions(group);

                        break;
                    default:
                        await this.MemberOptions(group);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error, Please try again");
                Logger.LogError(ex.Message);
                await this.MemberOptions(null);
            }
        }

        #endregion helper methods
    }
}