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
        private ILogger<Startup> _logger { get; }
        private IAuthService _authService { get; }
        private ITaskService _taskService { get; }

        public Startup(ILogger<Startup> logger, IAuthService authService,
            ITaskService taskService)
        {
            _logger = logger;
            _authService = authService;
            _taskService = taskService;
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
                _logger.LogError(ex.StackTrace);
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
                user.user_name = Console.ReadLine().Trim();
                do
                {
                    // validate username
                    if (user.user_name.Contains(" "))
                    {
                        Console.WriteLine("Username is not valid, Please enter valid username:");
                        user.user_name = Console.ReadLine().Trim();
                    }
                }
                while (user.user_name.Contains(" "));
                Console.WriteLine("Enter your first name (ex: peter):");
                user.first_name = Console.ReadLine().Trim();
                Console.WriteLine("Enter your email address (ex: peter@example.com):");
                user.email = Console.ReadLine().Trim();
                do
                {
                    // validate email address
                    if (!Regex.IsMatch(user.email, regex, RegexOptions.IgnoreCase))
                    {
                        Console.WriteLine("Email address is not valid, Please enter valid email:");
                        user.email = Console.ReadLine().Trim();
                    }
                }
                while (!Regex.IsMatch(user.email, regex, RegexOptions.IgnoreCase));
                Console.WriteLine("Enter your password (ex: peter@123):");
                user.password = Console.ReadLine().Trim();
                Console.WriteLine("Select your user role:");
                Console.WriteLine($"1. {Roles.lead}");
                Console.WriteLine($"2. {Roles.member}");
                string role = Console.ReadLine().Trim();
                user.role = (role == "1") ? Roles.lead : Roles.member;

                // api calling
                var result = await _authService.UserRegisterAsync(user);
                if (result.success)
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
                _logger.LogError(ex.Message);
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
                user.email = Console.ReadLine().Trim();
                do
                {
                    // validate email address
                    if (!Regex.IsMatch(user.email, regex, RegexOptions.IgnoreCase))
                    {
                        Console.WriteLine("Email address is not valid, Please enter valid email:");
                        user.email = Console.ReadLine().Trim();
                    }
                }
                while (!Regex.IsMatch(user.email, regex, RegexOptions.IgnoreCase));

                // get reset token
                var result = await _authService.ForgotPasswordAsync(user);
                if (result.success)
                {
                    // store password reset token
                    Keys.resetToken = result.reset_token;

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
                _logger.LogError(ex.Message);
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
                    password = newPassword,
                    con_password = conPassword,
                    reset_token = Keys.resetToken
                };

                // api calling
                var result = await _authService.ResetPasswordAsync(user);
                if (result.success)
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
                _logger.LogError(ex.Message);
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
                user.user_name = Console.ReadLine().Trim();
                Console.WriteLine("Enter your password:");
                user.password = Console.ReadLine().Trim();

                // login request
                var result = await _authService.UserLoginAsync(user);
                if (result.success)
                {
                    // store user tokens
                    Keys.accessToken = result.token;
                    Keys.refreshToken = result.refresh_token;

                    // read JWT token to identify the uer role
                    var handler = new JwtSecurityTokenHandler();
                    Keys.role = handler.ReadJwtToken(Keys.accessToken).Payload["role"].ToString();

                    // direct to application internal options
                    Console.WriteLine();
                    Console.WriteLine(("").PadRight(46, '-'));
                    Console.WriteLine("HELLO, WELCOME TO ASSIGNA CONSOLE APPLICATION");
                    Console.WriteLine(("").PadRight(46, '-'));
                    if (Keys.role == Roles.lead)
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
                _logger.LogError(ex.Message);
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
                            var result = await _taskService.AllTasks();
                            if (result.success)
                            {
                                await this.LeadTasks(group, result.data!, true);
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
                            var result = await _taskService.Pendings();
                            if (result.success)
                            {
                                await this.LeadTasks(group, result.data!, true);
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
                            var result = await _taskService.Completed();
                            if (result.success)
                            {
                                await this.LeadTasks(group, result.data!, true);
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
                            var result = await _taskService.HighPriority();
                            if (result.success)
                            {
                                await this.LeadTasks(group, result.data!, true);
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
                            var result = await _taskService.MediumPriority();
                            if (result.success)
                            {

                                await this.LeadTasks(group, result.data!, true);
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
                            var result = await _taskService.LowPriority();
                            if (result.success)
                            {
                                await this.LeadTasks(group, result.data!, true);
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
                            var result = await _taskService.AllTasks();
                            if (result.success)
                            {

                                await this.LeadTasks(group, result.data!, false);

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
                                    var info = await _taskService.LeadTaskInfo(id);
                                    if (info.success)
                                    {
                                        // empty data
                                        if (info.data!.Count <= 0)
                                        {
                                            Console.WriteLine("No data found to display");
                                            Console.WriteLine();
                                            await this.LeadOptions(group);
                                        }

                                        var data = info.data;

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
                                var result = await _taskService.SaveTaskAsync(task);
                                if (result.success)
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
                _logger.LogError(ex.Message);
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
                            var result = await _taskService.AllTasks();
                            if (result.success)
                            {

                                // all tasks
                                await this.MemberTasks(group, result.data!, true);
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
                            var result = await _taskService.Pendings();
                            if (result.success)
                            {

                                await this.MemberTasks(group, result.data!, true);
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
                            var result = await _taskService.Completed();
                            if (result.success)
                            {

                                await this.MemberTasks(group, result.data!, true);
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
                            var result = await _taskService.HighPriority();
                            if (result.success)
                            {
                                await this.MemberTasks(group, result.data!, true);
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
                            var result = await _taskService.MediumPriority();
                            if (result.success)
                            {
                                await this.MemberTasks(group, result.data!, true);
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
                            var result = await _taskService.LowPriority();
                            if (result.success)
                            {
                                await this.MemberTasks(group, result.data!, true);
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
                            var result = await _taskService.AllTasks();
                            if (result.success)
                            {

                                await this.MemberTasks(group, result.data!, false);

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
                                    if (info.success)
                                    {
                                        // empty data
                                        if (info.data!.Count <= 0)
                                        {
                                            Console.WriteLine("No data found to display");
                                            Console.WriteLine();
                                            await this.MemberOptions(group);
                                        }

                                        var data = info.data;

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
                _logger.LogError(ex.Message);
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
                            itm.tsk_id, itm.tsk_title, itm.deadline.ToString("yyyy-MM-dd")
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
                            itm.tsk_id, itm.tsk_title, itm.deadline.ToString("yyyy-MM-dd")
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
            Console.WriteLine($"Id        :    {data.First().tsk_id}");
            Console.WriteLine($"Title     :    {data.First().tsk_title}");
            Console.WriteLine($"Category  :    {data.First().cat_name}");
            Console.WriteLine($"Deadline  :    {data.First().deadline.ToString("yyyy-MM-dd")}");
            string priority = (data.First().pri_high) ? "High" : (data.First().pri_medium) ? "Medium" : "Low";
            Console.WriteLine($"Priority  :    {priority}");
            Console.WriteLine($"Assignee  :    {data.First().first_name}");
            Console.WriteLine($"Note      :    {data.First().tsk_note}");
            string status = (data.First().pending) ? "Pending" : "Completed";
            Console.WriteLine($"Status    :    {status}");
            string? note = (data.First().user_note == empty) ? "Not available" : data.First().user_note;
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
                task.tsk_title = Console.ReadLine().Trim();
                Console.WriteLine("Select category:");
                var category = await _taskService.AllCategories();
                if (category.success)
                {
                    if (category.data!.Count > 0)
                    {
                        foreach (var itm in category.data)
                        {
                            Console.WriteLine($"{itm.cat_id}. {itm.cat_name}");
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
                task.tsk_category = int.Parse(Console.ReadLine().Trim());
                Console.WriteLine("Enter due date (yyyy-MM-dd):");
                task.deadline = Console.ReadLine().Trim();
                string[] formats = { "yyyy-MM-dd" };
                bool valid = false;
                do
                {
                    if (DateTime.TryParseExact(task.deadline, formats,
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                    {
                        valid = true;
                    }
                    else
                    {
                        Console.WriteLine("Not valid format, Enter due date (yyyy-MM-dd):");
                        task.deadline = Console.ReadLine().Trim();
                        valid = false;
                    }

                    date = Convert.ToDateTime(task.deadline);
                    if (date.Date < DateTime.Now.Date)
                    {
                        Console.WriteLine("Please enter future date:");
                        task.deadline = Console.ReadLine().Trim();
                        valid = false;
                    }
                    else
                    {
                        valid = true;
                    }
                }
                while (!valid);
                Console.WriteLine("Select task assignee:");
                var memeber = await _taskService.TeamMembers();
                if (memeber.success)
                {
                    if (memeber.data!.Count > 0)
                    {
                        foreach (var itm in memeber.data)
                        {
                            Console.WriteLine($"{itm.user_id}. {itm.first_name}");
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
                task.member = int.Parse(Console.ReadLine().Trim());
                Console.WriteLine("Select task priority:");
                var priority = await _taskService.Priorities();
                if (priority.success)
                {
                    if (priority.data!.Count > 0)
                    {
                        foreach (var itm in priority.data)
                        {
                            Console.WriteLine($"- {itm.pri_name}");
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
                task.priority = Console.ReadLine().Trim();
                task.priority = $"{char.ToUpper(task.priority[0])}{task.priority.Substring(1)}";
                Console.WriteLine("Enter task note:");
                task.tsk_note = Console.ReadLine().Trim();
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error, Please try again");
                _logger.LogError(ex.Message);
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
                        if (data.First().complete)
                        {
                            Console.WriteLine("Already completed");
                            await this.MemberOptions(group);
                        }
                        var task = new EditTask();
                        task.tsk_id = data.First().tsk_id;
                        Console.WriteLine("NOTE: KEEP THE SAME VALUE, PRESS [-]");
                        Console.WriteLine();
                        Console.WriteLine("Enter task title:");
                        task.tsk_title = Console.ReadLine().Trim();
                        if (task.tsk_title == "-")
                        {
                            task.tsk_title = data.First().tsk_title;
                        }                    
                        Console.WriteLine("Select category:");
                        var category = await _taskService.AllCategories();
                        if (category.success)
                        {
                            if (category.data!.Count > 0)
                            {
                                foreach (var itm in category.data)
                                {
                                    Console.WriteLine($"{itm.cat_id}. {itm.cat_name}");
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
                            task.tsk_category = data.First().cat_id;
                        }
                        else
                        {
                            task.tsk_category = int.Parse(value);
                        }
                        Console.WriteLine("Enter due date (yyyy-MM-dd):");
                        task.deadline = Console.ReadLine().Trim();
                        if (task.deadline == "-")
                        {
                            task.deadline = data.First().deadline.ToString("yyyy-MM-dd");
                        }
                        string[] formats = { "yyyy-MM-dd" };
                        bool valid;
                        do
                        {
                            if (DateTime.TryParseExact(task.deadline, formats,
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                            {
                                valid = true;
                            }
                            else
                            {
                                Console.WriteLine("Not valid format, Enter due date (yyyy-MM-dd):");
                                task.deadline = Console.ReadLine().Trim();
                                valid = false;
                            }

                            date = Convert.ToDateTime(task.deadline);
                            if (date.Date < DateTime.Now.Date)
                            {
                                Console.WriteLine("Please enter future date:");
                                task.deadline = Console.ReadLine().Trim();
                                valid = false;
                            }
                            else
                            {
                                valid = true;
                            }
                        }
                        while (!valid);
                        Console.WriteLine("Select task assignee:");
                        var memeber = await _taskService.TeamMembers();
                        if (memeber.success)
                        {
                            if (memeber.data!.Count > 0)
                            {
                                foreach (var itm in memeber.data)
                                {
                                    Console.WriteLine($"{itm.user_id}. {itm.first_name}");
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
                            task.member = data.First().user_id;
                        }
                        else
                        {
                            task.member = int.Parse(value);
                        }
                        Console.WriteLine("Select task priority:");
                        var priority = await _taskService.Priorities();
                        if (priority.success)
                        {
                            if (priority.data!.Count > 0)
                            {
                                foreach (var itm in priority.data)
                                {
                                    Console.WriteLine($"- {itm.pri_name}");
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
                        task.priority = Console.ReadLine().Trim();
                        if (task.priority == "-")
                        {
                            task.priority = (data.First().pri_high) ? "High" : (data.First().pri_medium) ? "Medium" : "Low";
                        }
                        task.priority = $"{char.ToUpper(task.priority[0])}{task.priority.Substring(1)}";
                        Console.WriteLine("Enter task note:");
                        task.tsk_note = Console.ReadLine().Trim();
                        if (task.tsk_note == "-")
                        {
                            task.tsk_note = data.First().tsk_note;
                        }
                        Console.WriteLine();

                        Console.WriteLine("1. Save");
                        Console.WriteLine("2. Back");
                        Console.WriteLine();
                        selection = int.Parse(Console.ReadLine().Trim());

                        // calling api
                        if (selection == 1)
                        {
                            var edit = await _taskService.EditTaskAsync(task);
                            if (edit.success)
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
                        delete.tsk_id = int.Parse(Console.ReadLine().Trim());
                        Console.WriteLine();

                        Console.WriteLine("1. Delete");
                        Console.WriteLine("2. Back");
                        Console.WriteLine();
                        selection = int.Parse(Console.ReadLine().Trim());

                        // calling api
                        if (selection == 1)
                        {
                            var result = await _taskService.DeleteTaskAsync(delete);
                            if (result.success)
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
                        remind.tsk_id = int.Parse(Console.ReadLine().Trim());
                        Console.WriteLine("Enter your message:");
                        remind.message = Console.ReadLine().Trim();
                        Console.WriteLine();

                        Console.WriteLine("1. Send");
                        Console.WriteLine("2. Back");
                        Console.WriteLine();
                        selection = int.Parse(Console.ReadLine().Trim());

                        // calling api
                        if (selection == 1)
                        {
                            var result = await _taskService.SendRemind(remind);
                            if (result.success)
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
                _logger.LogError(ex.Message);
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
                        if (data.First().complete)
                        {
                            Console.WriteLine("Already completed");
                            await this.MemberOptions(group);
                        }
                        var note = new AddNote();
                        note.tsk_id = data.First().tsk_id;
                        Console.WriteLine();
                        Console.WriteLine("Enter your note:");
                        note.user_note = Console.ReadLine().Trim();
                        Console.WriteLine();

                        Console.WriteLine("1. Add note");
                        Console.WriteLine("2. Back");
                        Console.WriteLine();
                        selection = int.Parse(Console.ReadLine().Trim());

                        // calling api
                        if (selection == 1)
                        {
                            var edit = await _taskService.AddTaskNoteAsync(note);
                            if (edit.success)
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
                        var done = new MarkDone();
                        done.tsk_id = data.First().tsk_id;
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
                                var edit = await _taskService.MarkasDoneAsync(done);
                                if (edit.success)
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
                _logger.LogError(ex.Message);
                await this.MemberOptions(null);
            }
        }

        #endregion helper methods
    }
}