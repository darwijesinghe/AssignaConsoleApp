using ConsoleUI.ApiClient;
using ConsoleUI.Auth;
using ConsoleUI.Interfaces;
using ConsoleUI.Models;
using ConsoleUI.Response;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Task = ConsoleUI.Models.Task;

namespace ConsoleUI.Services
{
    /// <summary>
    /// Service implementation for the ITaskService
    /// </summary>
    public class TaskService : ITaskService
    {
        // Services
        private ILogger<Startup> _logger;
        private IAuthService     _authService;
        private AssignaClient    _client;

        // props
        private HttpResponseMessage? Response { get; set; }

        public TaskService(ILogger<Startup> logger, AssignaClient client, IAuthService authService)
        {
            _logger      = logger;
            _client      = client;
            _authService = authService;
        }

        /// <summary>
        /// Retrieves a list of team members.
        /// </summary>
        /// <returns>
        /// A list of <see cref="User"/> representing the team members.
        /// </returns>
        public async Task<Result<List<User>>> TeamMembers()
        {
            try
            {
                // api headers
                _client.Request.DefaultRequestHeaders.Authorization = new("Bearer", Keys.AccessToken);

                // calls the api
                using (Response = await _client.Request.GetAsync("user/members"))
                {
                    if (Response.StatusCode == HttpStatusCode.Unauthorized || Response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        // calls the refresh token
                        var refresh = new RefreshToken
                        {
                            TokenRefresh = Keys.RefreshToken
                        };

                        // gets the refresh token
                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.Success)
                        {
                            Keys.AccessToken  = token.Token;
                            Keys.RefreshToken = token.RefreshToken;
                        }

                        _client.Request.DefaultRequestHeaders.Authorization = new("Bearer", Keys.AccessToken);
                        Response = await _client.Request.GetAsync("user/members");
                    }

                    // reads the response
                    string result = await Response.Content.ReadAsStringAsync();

                    // deserializing
                    var member = JsonConvert.DeserializeObject<Result<List<User>>>(result)?.Data.ToList();

                    return new Result<List<User>>
                    {
                        Success = true,
                        Message = "Ok.",
                        Data    = member
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<User>>
                {
                    Success = false,
                    Message = "Internal error."
                };
            }
        }

        /// <summary>
        /// Retrieves a list of all categories.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Categories"/> representing all categories.
        /// </returns>
        public async Task<Result<List<Categories>>> AllCategories()
        {
            try
            {
                // api headers
                _client.Request.DefaultRequestHeaders.Authorization = new("Bearer", Keys.AccessToken);

                // calls the api
                using (Response = await _client.Request.GetAsync("category/categories"))
                {
                    if (Response.StatusCode == HttpStatusCode.Unauthorized || Response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        var refresh = new RefreshToken
                        {
                            TokenRefresh = Keys.RefreshToken
                        };

                        // gets the refresh token
                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.Success)
                        {
                            Keys.AccessToken  = token.Token;
                            Keys.RefreshToken = token.RefreshToken;
                        }

                        _client.Request.DefaultRequestHeaders.Authorization = new("Bearer", Keys.AccessToken);
                        Response = await _client.Request.GetAsync("category/categories");
                    }

                    // reads the response
                    string result = await Response.Content.ReadAsStringAsync();

                    // deserializing
                    var category = JsonConvert.DeserializeObject<Result<List<Categories>>>(result)?.Data.ToList();

                    return new Result<List<Categories>>
                    {
                        Success = true,
                        Message = "Ok.",
                        Data    = category
                    };
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Categories>>
                {
                    Success = false,
                    Message = "Internal error."
                };
            }
        }

        /// <summary>
        /// Retrieves a list of all priority levels.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Priorities"/> representing all priority levels.
        /// </returns>
        public async Task<Result<List<Priorities>>> Priorities()
        {
            try
            {
                // api headers
                _client.Request.DefaultRequestHeaders.Authorization = new("Bearer", Keys.AccessToken);

                // calls the api
                using (Response = await _client.Request.GetAsync("priority/priorities"))
                {
                    if (Response.StatusCode == HttpStatusCode.Unauthorized || Response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        var refresh = new RefreshToken
                        {
                            TokenRefresh = Keys.RefreshToken
                        };

                        // gets the refresj token
                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.Success)
                        {
                            Keys.AccessToken  = token.Token;
                            Keys.RefreshToken = token.RefreshToken;
                        }

                        _client.Request.DefaultRequestHeaders.Authorization = new("Bearer", Keys.AccessToken);
                        Response = await _client.Request.GetAsync("priority/priorities");
                    }

                    // reads the response
                    string result = await Response.Content.ReadAsStringAsync();

                    // deserializing
                    var priority = JsonConvert.DeserializeObject<Result<List<Priorities>>>(result)?.Data.ToList();

                    return new Result<List<Priorities>>
                    {
                        Success = true,
                        Message = "Ok.",
                        Data    = priority
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Priorities>>
                {
                    Success = false,
                    Message = "Internal error."
                };
            }
        }

        /// <summary>
        /// Retrieves a list of all tasks related data based on the user.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Task"/> containing all tasks.
        /// </returns>
        public async Task<Result<List<Task>>> AllTasks()
        {
            try
            {
                // gets the result
                var data = await TasksDataRequest("tasks");
                return new Result<List<Task>>
                {
                    Success = true,
                    Message = "Ok.",
                    Data    = Enumerable.ToList<Task>(data)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Task>>
                {
                    Success = false,
                    Message = "Internal error."
                };
            }
        }

        /// <summary>
        /// Retrieves a list of all pending tasks related data based on the user.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Task"/> containing all pending tasks.
        /// </returns>
        public async Task<Result<List<Task>>> Pendings()
        {
            try
            {
                // gets the result
                var data = await TasksDataRequest("pendings");
                return new Result<List<Task>>
                {
                    Success = true,
                    Message = "Ok.",
                    Data    = Enumerable.ToList<Task>(data)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Task>>
                {
                    Success = false,
                    Message = "Internal error."
                };
            }
        }

        /// <summary>
        /// Retrieves a list of all completed tasks related data based on the user.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Task"/> containing all completed tasks.
        /// </returns>
        public async Task<Result<List<Task>>> Completed()
        {
            try
            {
                // gets the result
                var data = await TasksDataRequest("completes");
                return new Result<List<Task>>
                {
                    Success = true,
                    Message = "Ok.",
                    Data    = Enumerable.ToList<Task>(data)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Task>>
                {
                    Success = false,
                    Message = "Internal error."
                };
            }
        }

        /// <summary>
        /// Retrieves a list of all high priority tasks related data based on the user.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Task"/> containing all high priority tasks.
        /// </returns>
        public async Task<Result<List<Task>>> HighPriority()
        {
            try
            {
                // gets the result
                var data = await TasksDataRequest("high-priority");
                return new Result<List<Task>>
                {
                    Success = true,
                    Message = "Ok.",
                    Data    = Enumerable.ToList<Task>(data)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Task>>
                {
                    Success = false,
                    Message = "Internal error."
                };
            }
        }

        /// <summary>
        /// Retrieves a list of all medium priority tasks related data based on the user.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Task"/> containing all medium priority tasks.
        /// </returns>
        public async Task<Result<List<Task>>> MediumPriority()
        {
            try
            {
                // gets the result
                var data = await TasksDataRequest("medium-priority");
                return new Result<List<Task>>
                {
                    Success = true,
                    Message = "Ok.",
                    Data    = Enumerable.ToList<Task>(data)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Task>>
                {
                    Success = false,
                    Message = "Internal error."
                };
            }
        }

        /// <summary>
        /// Retrieves a list of all low priority tasks related data based on the user.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Task"/> containing all low  priority tasks.
        /// </returns>
        public async Task<Result<List<Task>>> LowPriority()
        {
            try
            {
                // gets the result
                var data = await TasksDataRequest("low-priority");
                return new Result<List<Task>>
                {
                    Success = true,
                    Message = "Ok.",
                    Data    = Enumerable.ToList<Task>(data)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Task>>
                {
                    Success = false,
                    Message = "Internal error."
                };
            }
        }

        /// <summary>
        /// Gets list of task based on the type. (e.g. complete, pending)
        /// </summary>
        /// <param name="action">The task type</param>
        /// <returns>
        /// A list of <see cref="Task"/> containing the task data.
        /// </returns>
        private async Task<List<Task>> TasksDataRequest(string action)
        {
            try
            {
                // changes the controller based on the user
                string controller = (Keys.Role == Roles.lead) ? "leadtasks" : "membertasks";

                // returns the result
                return (await MakeGetRequest($"{controller}/{action}"))?.Data ?? new List<Task>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new List<Task>();
            }
        }

        /// <summary>
        /// Retrieves detailed information about a specific task of the team-lead user role.
        /// </summary>
        /// <param name="taskId">The identifier of the task for which to retrieve information.</param>
        /// <returns>
        /// A list of <see cref="Task"/> containing the task information.
        /// </returns>
        public async Task<Result<List<Task>>> LeadTaskInfo(int taskId)
        {
            try
            {
                // returns the result
                return await MakeGetRequest($"leadtasks/task-info?taskid={taskId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Task>>
                {
                    Success = false,
                    Message = "Internal error."
                };
            }
        }

        /// <summary>
        /// Retrieves detailed information about a specific task of the team-member user role.
        /// </summary>
        /// <param name="taskId">The identifier of the task for which to retrieve information.</param>
        /// <returns>
        /// A list of <see cref="Task"/> containing the task information.
        /// </returns>
        public async Task<Result<List<Task>>> MemberTaskInfo(int taskId)
        {
            try
            {
                // returns the result
                return await MakeGetRequest($"membertasks/task-info?taskid={taskId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Task>>
                {
                    Success = false,
                    Message = "Internal error."
                };
            }
        }

        /// <summary>
        /// Saves a task.
        /// </summary>
        /// <param name="data">The data containing the task information to be saved.</param>
        /// <returns>
        /// A <see cref="Result"/> indicating the outcome of the save operation.
        /// </returns>
        public async Task<Result> SaveTaskAsync(AddTask data)
        {
            try
            {
                // new task data
                var task = new AddTask
                {
                    TaskTitle    = data.TaskTitle,
                    TaskCategory = data.TaskCategory,
                    Deadline     = data.Deadline,
                    Priority     = data.Priority,
                    Member       = data.Member,
                    TaskNote     = data.TaskNote
                };

                // returns the result
                return await MakePostRequest(data: task, url: "leadtasks/add-task");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result
                {
                    Success = false,
                    Message = "Internal error."
                };
            }
        }

        /// <summary>
        /// Edits an existing task.
        /// </summary>
        /// <param name="data">The data containing the updated task information.</param>
        /// <returns>
        /// A <see cref="Result"/> indicating the outcome of the edit operation.
        /// </returns>
        public async Task<Result> EditTaskAsync(EditTask data)
        {
            try
            {
                // edit task data
                var task = new EditTask
                {
                    TaskId       = data.TaskId,
                    TaskTitle    = data.TaskTitle,
                    TaskCategory = data.TaskCategory,
                    Deadline     = data.Deadline,
                    Priority     = data.Priority,
                    Member       = data.Member,
                    TaskNote     = data.TaskNote
                };

                // returns the result
                return await MakePostRequest(data: task, url: "leadtasks/edit-task");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result
                {
                    Success = false,
                    Message = "Internal error."
                };
            }
        }

        /// <summary>
        /// Deletes a task.
        /// </summary>
        /// <param name="data">The data containing the task identifier to be deleted.</param>
        /// <returns>
        /// A <see cref="Result"/> indicating the outcome of the delete operation.
        /// </returns>
        public async Task<Result> DeleteTaskAsync(DeleteTask data)
        {
            try
            {
                // delete task data
                var task = new DeleteTask
                {
                    TaskId = data.TaskId
                };

                // returns the result
                return await MakePostRequest(data: task, url: "leadtasks/delete-task");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result
                {
                    Success = false,
                    Message = "Internal error"
                };
            }
        }

        /// <summary>
        /// Sends an email to the specified recipient.
        /// </summary>
        /// <param name="data">The data containing the email information.</param>
        /// <returns>
        /// A <see cref="Result"/> representing the result of the email sending operation.
        /// </returns>
        public async Task<Result> SendRemind(SendEmail data)
        {
            try
            {
                // email data
                var task = new SendEmail
                {
                    TaskId = data.TaskId,
                    Message = data.Message
                };

                // returns the result
                return await MakePostRequest(data: task, url: "leadtasks/send-remind");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result
                {
                    Success = false,
                    Message = "Internal error."
                };
            }
        }

        /// <summary>
        /// Adds a note to a task.
        /// </summary>
        /// <param name="data">The data containing the task information and the note to be added.</param>
        /// <returns>
        /// A <see cref="Result"/> indicating the outcome of the add note operation.
        /// </returns>
        public async Task<Result> AddTaskNoteAsync(AddNote data)
        {
            try
            {
                // task note data
                var task = new AddNote
                {
                    TaskId    = data.TaskId,
                    UserNote = data.UserNote
                };

                // returns the result
                return await MakePostRequest(data: task, url: "membertasks/write-note");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result
                {
                    Success = false,
                    Message = "Internal error."
                };
            }
        }

        /// <summary>
        /// Marks a task as done.
        /// </summary>
        /// <param name="data">The data containing the task identifier to be marked as done.</param>
        /// <returns>
        /// A a <see cref="Result"/> indicating the outcome of the mark as done operation.
        /// </returns>
        public async Task<Result> MarkasDoneAsync(MarkDone data)
        {
            try
            {
                // task data
                var task = new MarkDone
                {
                    TaskId = data.TaskId
                };

                // returns the result
                return await MakePostRequest(data: task, url: "membertasks/mark-done");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result
                {
                    Success = false,
                    Message = "Internal error"
                };
            }
        }

        // Helpers ------------------------------------------------

        /// <summary>
        /// Makes post request to the endpoint.
        /// </summary>
        /// <param name="data">The data to be sent.</param>
        /// <param name="url">The endpoint url.</param>
        /// <returns>
        /// A <see cref="Result"/> indicating the outcome of the operation.
        /// </returns>
        private async Task<Result> MakePostRequest<T>(T data, string url)
        {
            try
            {
                // serializing
                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

                // adds headers
                _client.Request.DefaultRequestHeaders.Authorization = new("Bearer", Keys.AccessToken);

                // calls the api
                using (Response = await _client.Request.PostAsync(url, content))
                {
                    if (Response.StatusCode == HttpStatusCode.Unauthorized || Response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        var refresh = new RefreshToken
                        {
                            TokenRefresh = Keys.RefreshToken
                        };

                        // gets the refresh token
                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.Success)
                        {
                            Keys.AccessToken = token.Token;
                            Keys.RefreshToken = token.RefreshToken;
                        }

                        _client.Request.DefaultRequestHeaders.Authorization = new("Bearer", Keys.AccessToken);
                        Response = await _client.Request.GetAsync("membertasks/write-note");
                    }

                    // reads the response
                    string result = await Response.Content.ReadAsStringAsync();

                    // deserializing
                    var resultt = JsonConvert.DeserializeObject<Result>(result);

                    // returns the result
                    return resultt ?? new Result();

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result
                {
                    Success = false,
                    Message = "Internal error."
                };
            }
        }

        /// <summary>
        /// Makes get request to the endpoint.
        /// </summary>
        /// <param name="url">The endpoint url.</param>
        /// <returns>
        /// A list of <see cref="Task"/> containing the task data.
        /// </returns>
        private async Task<Result<List<Task>>> MakeGetRequest(string url)
        {
            try
            {
                // adds headers
                _client.Request.DefaultRequestHeaders.Authorization = new("Bearer", Keys.AccessToken);

                // calls the api
                using (Response = await _client.Request.GetAsync(url))
                {
                    if (Response.StatusCode == HttpStatusCode.Unauthorized || Response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        var refresh = new RefreshToken
                        {
                            TokenRefresh = Keys.RefreshToken
                        };

                        // gets the refresh token
                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.Success)
                        {
                            Keys.AccessToken  = token.Token;
                            Keys.RefreshToken = token.RefreshToken;
                        }

                        _client.Request.DefaultRequestHeaders.Authorization = new("Bearer", Keys.AccessToken);
                        Response = await _client.Request.GetAsync(url);
                    }

                    // reads the response
                    string result = await Response.Content.ReadAsStringAsync();

                    // deserializing
                    var info = JsonConvert.DeserializeObject<Result<List<Task>>>(result)!.Data.ToList();

                    // returns the result
                    return new Result<List<Task>>
                    {
                        Success = true,
                        Message = "Ok.",
                        Data    = info
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Task>>
                {
                    Success = false,
                    Message = "Internal error."
                };
            }
        }
    }
}