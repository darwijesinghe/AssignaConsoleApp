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
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleUI.Services
{
    public class TaskService : ITaskService
    {
        // services
        private ILogger<Startup> _logger { get; }
        private IAuthService _authService { get; }
        private AssignaClient _client { get; }

        // props
        private HttpResponseMessage? response { get; set; }

        public TaskService(ILogger<Startup> logger,
            AssignaClient client, IAuthService authService)
        {
            _logger = logger;
            _client = client;
            _authService = authService;
        }

        // get team members
        public async Task<Result<List<Users>>> TeamMembers()
        {
            try
            {
                // adding headers
                var header = new AuthenticationHeaderValue("Bearer", Keys.AccessToken);
                _client.Request.DefaultRequestHeaders.Authorization = header;

                // calling api
                using (response = await _client.Request.GetAsync("user/members"))
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                    | response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        // call refresh token
                        var refresh = new RefreshToken
                        {
                            TokenRefresh = Keys.RefreshToken
                        };

                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.Success)
                        {
                            Keys.AccessToken = token.Token;
                            Keys.RefreshToken = token.RefreshToken;
                        }

                        header = new AuthenticationHeaderValue("Bearer", Keys.AccessToken);
                        _client.Request.DefaultRequestHeaders.Authorization = header;

                        response = await _client.Request.GetAsync("user/members");
                    }

                    // read response
                    string result = await response.Content.ReadAsStringAsync();

                    // deserializing
                    var member = JsonConvert.DeserializeObject<Result<List<Users>>>(result)!
                        .Data.ToList();

                    return new Result<List<Users>>
                    {
                        Success = true,
                        Message = "Ok",
                        Data = member
                    };
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Users>>
                {
                    Success = false,
                    Message = "Internal error"
                };
            }
        }

        // get categories
        public async Task<Result<List<Categories>>> AllCategories()
        {
            try
            {
                var header = new AuthenticationHeaderValue("Bearer", Keys.AccessToken);
                _client.Request.DefaultRequestHeaders.Authorization = header;

                using (response = await _client.Request.GetAsync("category/categories"))
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                  | response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        var refresh = new RefreshToken
                        {
                            TokenRefresh = Keys.RefreshToken
                        };

                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.Success)
                        {
                            Keys.AccessToken = token.Token;
                            Keys.RefreshToken = token.RefreshToken;
                        }

                        header = new AuthenticationHeaderValue("Bearer", Keys.AccessToken);
                        _client.Request.DefaultRequestHeaders.Authorization = header;

                        response = await _client.Request.GetAsync("category/categories");
                    }


                    string result = await response.Content.ReadAsStringAsync();

                    var category = JsonConvert.DeserializeObject<Result<List<Categories>>>(result)!
                        .Data.ToList();

                    return new Result<List<Categories>>
                    {
                        Success = true,
                        Message = "Ok",
                        Data = category
                    };
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Categories>>
                {
                    Success = false,
                    Message = "Internal error"
                };
            }
        }

        // get priorities
        public async Task<Result<List<Priorities>>> Priorities()
        {
            try
            {
                var header = new AuthenticationHeaderValue("Bearer", Keys.AccessToken);
                _client.Request.DefaultRequestHeaders.Authorization = header;

                using (response = await _client.Request.GetAsync("priority/priorities"))
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                        | response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        var refresh = new RefreshToken
                        {
                            TokenRefresh = Keys.RefreshToken
                        };

                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.Success)
                        {
                            Keys.AccessToken = token.Token;
                            Keys.RefreshToken = token.RefreshToken;
                        }

                        header = new AuthenticationHeaderValue("Bearer", Keys.AccessToken);
                        _client.Request.DefaultRequestHeaders.Authorization = header;

                        response = await _client.Request.GetAsync("priority/priorities");
                    }


                    string result = await response.Content.ReadAsStringAsync();

                    var priority = JsonConvert.DeserializeObject<Result<List<Priorities>>>(result)!
                        .Data.ToList();

                    return new Result<List<Priorities>>
                    {
                        Success = true,
                        Message = "Ok",
                        Data = priority
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Priorities>>
                {
                    Success = false,
                    Message = "Internal error"
                };
            }
        }

        // all tasks
        public async Task<Result<List<Tasks>>> AllTasks()
        {
            try
            {
                var data = await TasksDataRequest("tasks");
                return new Result<List<Tasks>>
                {
                    Success = true,
                    Message = "Ok",
                    Data = data.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Tasks>>
                {
                    Success = false,
                    Message = "Internal error"
                };
            }
        }

        // pending taks
        public async Task<Result<List<Tasks>>> Pendings()
        {
            try
            {
                var data = await TasksDataRequest("pendings");
                return new Result<List<Tasks>>
                {
                    Success = true,
                    Message = "Ok",
                    Data = data.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Tasks>>
                {
                    Success = false,
                    Message = "Internal error"
                };
            }
        }

        // completed tasks
        public async Task<Result<List<Tasks>>> Completed()
        {
            try
            {
                var data = await TasksDataRequest("completes");
                return new Result<List<Tasks>>
                {
                    Success = true,
                    Message = "Ok",
                    Data = data.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Tasks>>
                {
                    Success = false,
                    Message = "Internal error"
                };
            }
        }

        // high priority tasks
        public async Task<Result<List<Tasks>>> HighPriority()
        {
            try
            {
                var data = await TasksDataRequest("high-priority");
                return new Result<List<Tasks>>
                {
                    Success = true,
                    Message = "Ok",
                    Data = data.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Tasks>>
                {
                    Success = false,
                    Message = "Internal error"
                };
            }
        }

        // medium priority tasks
        public async Task<Result<List<Tasks>>> MediumPriority()
        {
            try
            {
                var data = await TasksDataRequest("medium-priority");
                return new Result<List<Tasks>>
                {
                    Success = true,
                    Message = "Ok",
                    Data = data.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Tasks>>
                {
                    Success = false,
                    Message = "Internal error"
                };
            }
        }

        // low priority tasks
        public async Task<Result<List<Tasks>>> LowPriority()
        {
            try
            {
                var data = await TasksDataRequest("low-priority");
                return new Result<List<Tasks>>
                {
                    Success = true,
                    Message = "Ok",
                    Data = data.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Tasks>>
                {
                    Success = false,
                    Message = "Internal error"
                };
            }
        }

        // task category data
        private async Task<List<Tasks>> TasksDataRequest(string action)
        {
            // change controller based on user
            string controller = (Keys.Role == Roles.lead) ? "leadtasks" : "membertasks";

            // adding headers
            var header = new AuthenticationHeaderValue("Bearer", Keys.AccessToken);
            _client.Request.DefaultRequestHeaders.Authorization = header;

            // calling api
            using(response = await _client.Request.GetAsync($"{controller}/{action}"))
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized
                    | response.StatusCode == HttpStatusCode.Forbidden)
                {
                    // call refresh token
                    var refresh = new RefreshToken
                    {
                        TokenRefresh = Keys.RefreshToken
                    };

                    var token = await _authService.RefreshTokenAsync(refresh);
                    if (token.Success)
                    {
                        Keys.AccessToken = token.Token;
                        Keys.RefreshToken = token.RefreshToken;
                    }

                    header = new AuthenticationHeaderValue("Bearer", Keys.AccessToken);
                    _client.Request.DefaultRequestHeaders.Authorization = header;

                    response = await _client.Request.GetAsync($"{controller}/{action}");

                }


                string result = await response.Content.ReadAsStringAsync();

                // deserializing
                var data = JsonConvert.DeserializeObject<Result<List<Tasks>>>(result)!
                    .Data.ToList();

                return data;

            }

        }

        // task infomation
        public async Task<Result<List<Tasks>>> LeadTaskInfo(int taskId)
        {
            try
            {
                var header = new AuthenticationHeaderValue("Bearer", Keys.AccessToken);
                _client.Request.DefaultRequestHeaders.Authorization = header;

                using (response = await _client.Request.GetAsync($"leadtasks/task-info?taskid={taskId}"))
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                    | response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        var refresh = new RefreshToken
                        {
                            TokenRefresh = Keys.RefreshToken
                        };

                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.Success)
                        {
                            Keys.AccessToken = token.Token;
                            Keys.RefreshToken = token.RefreshToken;
                        }

                        header = new AuthenticationHeaderValue("Bearer", Keys.AccessToken);
                        _client.Request.DefaultRequestHeaders.Authorization = header;

                        response = await _client.Request.GetAsync($"leadtasks/task-info?taskid={taskId}");
                    }

                    string result = await response.Content.ReadAsStringAsync();

                    var info = JsonConvert.DeserializeObject<Result<List<Tasks>>>(result)!
                        .Data.ToList();

                    return new Result<List<Tasks>>
                    {
                        Success = true,
                        Message = "Ok",
                        Data = info
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Tasks>>
                {
                    Success = false,
                    Message = "Internal error"
                };
            }
        }

        public async Task<Result<List<Tasks>>> MemberTaskInfo(int taskId)
        {
            try
            {
                var header = new AuthenticationHeaderValue("Bearer", Keys.AccessToken);
                _client.Request.DefaultRequestHeaders.Authorization = header;

                using (response = await _client.Request.GetAsync($"membertasks/task-info?taskid={taskId}"))
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                    | response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        var refresh = new RefreshToken
                        {
                            TokenRefresh = Keys.RefreshToken
                        };

                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.Success)
                        {
                            Keys.AccessToken = token.Token;
                            Keys.RefreshToken = token.RefreshToken;
                        }

                        header = new AuthenticationHeaderValue("Bearer", Keys.AccessToken);
                        _client.Request.DefaultRequestHeaders.Authorization = header;

                        response = await _client.Request.GetAsync($"membertasks/task-info?taskid={taskId}");
                    }

                    string result = await response.Content.ReadAsStringAsync();

                    var info = JsonConvert.DeserializeObject<Result<List<Tasks>>>(result)!
                        .Data.ToList();

                    return new Result<List<Tasks>>
                    {
                        Success = true,
                        Message = "Ok",
                        Data = info
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Tasks>>
                {
                    Success = false,
                    Message = "Internal error"
                };
            }
        }

        // add a new task
        public async Task<Result> SaveTaskAsync(AddTask data)
        {
            try
            {
                var task = new AddTask
                {
                    TskTitle = data.TskTitle,
                    TskCategory = data.TskCategory,
                    Deadline = data.Deadline,
                    Priority = data.Priority,
                    Member = data.Member,
                    TskNote = data.TskNote
                };

                // serializing
                var content = new StringContent
                    (
                        JsonConvert.SerializeObject(task),
                        encoding: Encoding.UTF8,
                        mediaType: "application/json"
                    );

                // adding headers
                var header = new AuthenticationHeaderValue("Bearer", Keys.AccessToken);
                _client.Request.DefaultRequestHeaders.Authorization = header;

                using (response = await _client.Request.PostAsync("leadtasks/add-task", content))
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                        | response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        // call refresh token
                        var refresh = new RefreshToken
                        {
                            TokenRefresh = Keys.RefreshToken
                        };

                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.Success)
                        {
                            Keys.AccessToken = token.Token;
                            Keys.RefreshToken = token.RefreshToken;
                        }

                        header = new AuthenticationHeaderValue("Bearer", Keys.AccessToken);
                        _client.Request.DefaultRequestHeaders.Authorization = header;

                        response = await _client.Request.GetAsync("leadtasks/add-task");
                    }

                    // read response
                    string result = await response.Content.ReadAsStringAsync();

                    // deserializing
                    Result? resultt = JsonConvert.DeserializeObject<Result>(result);
                    return resultt ?? new Result();
                }

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

        // edit a task
        public async Task<Result> EditTaskAsync(EditTask data)
        {
            try
            {
                var task = new EditTask
                {
                    TskId = data.TskId,
                    TskTitle = data.TskTitle,
                    TskCategory = data.TskCategory,
                    Deadline = data.Deadline,
                    Priority = data.Priority,
                    Member = data.Member,
                    TskNote = data.TskNote
                };

                var content = new StringContent
                    (
                        JsonConvert.SerializeObject(task),
                        encoding: Encoding.UTF8,
                        mediaType: "application/json"
                    );


                var header = new AuthenticationHeaderValue("Bearer", Keys.AccessToken);
                _client.Request.DefaultRequestHeaders.Authorization = header;

                using (response = await _client.Request.PostAsync("leadtasks/edit-task", content))
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                        | response.StatusCode == HttpStatusCode.Forbidden)
                    {

                        var refresh = new RefreshToken
                        {
                            TokenRefresh = Keys.RefreshToken
                        };

                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.Success)
                        {
                            Keys.AccessToken = token.Token;
                            Keys.RefreshToken = token.RefreshToken;
                        }

                        header = new AuthenticationHeaderValue("Bearer", Keys.AccessToken);
                        _client.Request.DefaultRequestHeaders.Authorization = header;

                        response = await _client.Request.GetAsync("leadtasks/edit-task");
                    }

                    string result = await response.Content.ReadAsStringAsync();

                    Result? resultt = JsonConvert.DeserializeObject<Result>(result);
                    return resultt ?? new Result();
                }

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

        // delete a task
        public async Task<Result> DeleteTaskAsync(DeleteTask data)
        {
            try
            {
                var task = new DeleteTask
                {
                    TskId = data.TskId
                };

                var content = new StringContent
                    (
                        JsonConvert.SerializeObject(task),
                        encoding: Encoding.UTF8,
                        mediaType: "application/json"
                    );


                var header = new AuthenticationHeaderValue("Bearer", Keys.AccessToken);
                _client.Request.DefaultRequestHeaders.Authorization = header;

                using (response = await _client.Request.PostAsync("leadtasks/delete-task", content))
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                        | response.StatusCode == HttpStatusCode.Forbidden)
                    {

                        var refresh = new RefreshToken
                        {
                            TokenRefresh = Keys.RefreshToken
                        };

                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.Success)
                        {
                            Keys.AccessToken = token.Token;
                            Keys.RefreshToken = token.RefreshToken;
                        }

                        header = new AuthenticationHeaderValue("Bearer", Keys.AccessToken);
                        _client.Request.DefaultRequestHeaders.Authorization = header;

                        response = await _client.Request.GetAsync("leadtasks/delete-task");
                    }


                    string result = await response.Content.ReadAsStringAsync();

                    Result? resultt = JsonConvert.DeserializeObject<Result>(result);
                    return resultt ?? new Result();
                }

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

        // send remind
        public async Task<Result> SendRemind(SendEmail data)
        {
            try
            {
                var task = new SendEmail
                {
                    TskId = data.TskId,
                    Message = data.Message
                };

                var content = new StringContent
                    (
                        JsonConvert.SerializeObject(task),
                        encoding: Encoding.UTF8,
                        mediaType: "application/json"
                    );


                var header = new AuthenticationHeaderValue("Bearer", Keys.AccessToken);
                _client.Request.DefaultRequestHeaders.Authorization = header;

                using (response = await _client.Request.PostAsync("leadtasks/send-remind", content))
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                        | response.StatusCode == HttpStatusCode.Forbidden)
                    {

                        var refresh = new RefreshToken
                        {
                            TokenRefresh = Keys.RefreshToken
                        };

                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.Success)
                        {
                            Keys.AccessToken = token.Token;
                            Keys.RefreshToken = token.RefreshToken;
                        }

                        header = new AuthenticationHeaderValue("Bearer", Keys.AccessToken);
                        _client.Request.DefaultRequestHeaders.Authorization = header;

                        response = await _client.Request.GetAsync("leadtasks/send-remind");
                    }

                    string result = await response.Content.ReadAsStringAsync();

                    Result? resultt = JsonConvert.DeserializeObject<Result>(result);
                    return resultt ?? new Result();
                }

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

        // add task note
        public async Task<Result> AddTaskNoteAsync(AddNote data)
        {
            try
            {
                var task = new AddNote
                {
                    TskId = data.TskId,
                    UserNote = data.UserNote
                };

                var content = new StringContent
                    (
                        JsonConvert.SerializeObject(task),
                        encoding: Encoding.UTF8,
                        mediaType: "application/json"
                    );


                var header = new AuthenticationHeaderValue("Bearer", Keys.AccessToken);
                _client.Request.DefaultRequestHeaders.Authorization = header;

                using (response = await _client.Request.PostAsync("membertasks/write-note", content))
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                        | response.StatusCode == HttpStatusCode.Forbidden)
                    {

                        var refresh = new RefreshToken
                        {
                            TokenRefresh = Keys.RefreshToken
                        };

                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.Success)
                        {
                            Keys.AccessToken = token.Token;
                            Keys.RefreshToken = token.RefreshToken;
                        }

                        header = new AuthenticationHeaderValue("Bearer", Keys.AccessToken);
                        _client.Request.DefaultRequestHeaders.Authorization = header;

                        response = await _client.Request.GetAsync("membertasks/write-note");
                    }

                    string result = await response.Content.ReadAsStringAsync();

                    Result? resultt = JsonConvert.DeserializeObject<Result>(result);
                    return resultt ?? new Result();
                }

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

        // mark as done
        public async Task<Result> MarkasDoneAsync(MarkDone data)
        {
            try
            {
                var task = new AddNote
                {
                    TskId = data.TskId
                };

                var content = new StringContent
                    (
                        JsonConvert.SerializeObject(task),
                        encoding: Encoding.UTF8,
                        mediaType: "application/json"
                    );


                var header = new AuthenticationHeaderValue("Bearer", Keys.AccessToken);
                _client.Request.DefaultRequestHeaders.Authorization = header;

                using (response = await _client.Request.PostAsync("membertasks/mark-done", content))
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                        | response.StatusCode == HttpStatusCode.Forbidden)
                    {

                        var refresh = new RefreshToken
                        {
                            TokenRefresh = Keys.RefreshToken
                        };

                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.Success)
                        {
                            Keys.AccessToken = token.Token;
                            Keys.RefreshToken = token.RefreshToken;
                        }

                        header = new AuthenticationHeaderValue("Bearer", Keys.AccessToken);
                        _client.Request.DefaultRequestHeaders.Authorization = header;

                        response = await _client.Request.GetAsync("membertasks/mark-done");
                    }

                    string result = await response.Content.ReadAsStringAsync();

                    Result? resultt = JsonConvert.DeserializeObject<Result>(result);
                    return resultt ?? new Result();
                }

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
    }
}