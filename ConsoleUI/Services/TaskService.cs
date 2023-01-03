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
                var header = new AuthenticationHeaderValue("Bearer", Keys.accessToken);
                _client.request.DefaultRequestHeaders.Authorization = header;

                // calling api
                using (response = await _client.request.GetAsync("user/members"))
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                    | response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        // call refresh token
                        var refresh = new RefreshToken
                        {
                            refresh_token = Keys.refreshToken
                        };

                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.success)
                        {
                            Keys.accessToken = token.token;
                            Keys.refreshToken = token.refresh_token;
                        }

                        header = new AuthenticationHeaderValue("Bearer", Keys.accessToken);
                        _client.request.DefaultRequestHeaders.Authorization = header;

                        response = await _client.request.GetAsync("user/members");
                    }

                    // read response
                    string result = await response.Content.ReadAsStringAsync();

                    // deserializing
                    var member = JsonConvert.DeserializeObject<Result<List<Users>>>(result)!
                        .data.ToList();

                    return new Result<List<Users>>
                    {
                        success = true,
                        message = "Ok",
                        data = member
                    };
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Users>>
                {
                    success = false,
                    message = "Internal error"
                };
            }
        }

        // get categories
        public async Task<Result<List<Categories>>> AllCategories()
        {
            try
            {
                var header = new AuthenticationHeaderValue("Bearer", Keys.accessToken);
                _client.request.DefaultRequestHeaders.Authorization = header;

                using (response = await _client.request.GetAsync("category/categories"))
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                  | response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        var refresh = new RefreshToken
                        {
                            refresh_token = Keys.refreshToken
                        };

                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.success)
                        {
                            Keys.accessToken = token.token;
                            Keys.refreshToken = token.refresh_token;
                        }

                        header = new AuthenticationHeaderValue("Bearer", Keys.accessToken);
                        _client.request.DefaultRequestHeaders.Authorization = header;

                        response = await _client.request.GetAsync("category/categories");
                    }


                    string result = await response.Content.ReadAsStringAsync();

                    var category = JsonConvert.DeserializeObject<Result<List<Categories>>>(result)!
                        .data.ToList();

                    return new Result<List<Categories>>
                    {
                        success = true,
                        message = "Ok",
                        data = category
                    };
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Categories>>
                {
                    success = false,
                    message = "Internal error"
                };
            }
        }

        // get priorities
        public async Task<Result<List<Priorities>>> Priorities()
        {
            try
            {
                var header = new AuthenticationHeaderValue("Bearer", Keys.accessToken);
                _client.request.DefaultRequestHeaders.Authorization = header;

                using (response = await _client.request.GetAsync("priority/priorities"))
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                        | response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        var refresh = new RefreshToken
                        {
                            refresh_token = Keys.refreshToken
                        };

                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.success)
                        {
                            Keys.accessToken = token.token;
                            Keys.refreshToken = token.refresh_token;
                        }

                        header = new AuthenticationHeaderValue("Bearer", Keys.accessToken);
                        _client.request.DefaultRequestHeaders.Authorization = header;

                        response = await _client.request.GetAsync("priority/priorities");
                    }


                    string result = await response.Content.ReadAsStringAsync();

                    var priority = JsonConvert.DeserializeObject<Result<List<Priorities>>>(result)!
                        .data.ToList();

                    return new Result<List<Priorities>>
                    {
                        success = true,
                        message = "Ok",
                        data = priority
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Priorities>>
                {
                    success = false,
                    message = "Internal error"
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
                    success = true,
                    message = "Ok",
                    data = data.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Tasks>>
                {
                    success = false,
                    message = "Internal error"
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
                    success = true,
                    message = "Ok",
                    data = data.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Tasks>>
                {
                    success = false,
                    message = "Internal error"
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
                    success = true,
                    message = "Ok",
                    data = data.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Tasks>>
                {
                    success = false,
                    message = "Internal error"
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
                    success = true,
                    message = "Ok",
                    data = data.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Tasks>>
                {
                    success = false,
                    message = "Internal error"
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
                    success = true,
                    message = "Ok",
                    data = data.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Tasks>>
                {
                    success = false,
                    message = "Internal error"
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
                    success = true,
                    message = "Ok",
                    data = data.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Tasks>>
                {
                    success = false,
                    message = "Internal error"
                };
            }
        }

        // task category data
        private async Task<List<Tasks>> TasksDataRequest(string action)
        {
            // change controller based on user
            string controller = (Keys.role == Roles.lead) ? "leadtasks" : "membertasks";

            // adding headers
            var header = new AuthenticationHeaderValue("Bearer", Keys.accessToken);
            _client.request.DefaultRequestHeaders.Authorization = header;

            // calling api
            using(response = await _client.request.GetAsync($"{controller}/{action}"))
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized
                    | response.StatusCode == HttpStatusCode.Forbidden)
                {
                    // call refresh token
                    var refresh = new RefreshToken
                    {
                        refresh_token = Keys.refreshToken
                    };

                    var token = await _authService.RefreshTokenAsync(refresh);
                    if (token.success)
                    {
                        Keys.accessToken = token.token;
                        Keys.refreshToken = token.refresh_token;
                    }

                    header = new AuthenticationHeaderValue("Bearer", Keys.accessToken);
                    _client.request.DefaultRequestHeaders.Authorization = header;

                    response = await _client.request.GetAsync($"{controller}/{action}");

                }


                string result = await response.Content.ReadAsStringAsync();

                // deserializing
                var data = JsonConvert.DeserializeObject<Result<List<Tasks>>>(result)!
                    .data.ToList();

                return data;

            }

        }

        // task infomation
        public async Task<Result<List<Tasks>>> LeadTaskInfo(int taskId)
        {
            try
            {
                var header = new AuthenticationHeaderValue("Bearer", Keys.accessToken);
                _client.request.DefaultRequestHeaders.Authorization = header;

                using (response = await _client.request.GetAsync($"leadtasks/task-info?taskid={taskId}"))
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                    | response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        var refresh = new RefreshToken
                        {
                            refresh_token = Keys.refreshToken
                        };

                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.success)
                        {
                            Keys.accessToken = token.token;
                            Keys.refreshToken = token.refresh_token;
                        }

                        header = new AuthenticationHeaderValue("Bearer", Keys.accessToken);
                        _client.request.DefaultRequestHeaders.Authorization = header;

                        response = await _client.request.GetAsync($"leadtasks/task-info?taskid={taskId}");
                    }

                    string result = await response.Content.ReadAsStringAsync();

                    var info = JsonConvert.DeserializeObject<Result<List<Tasks>>>(result)!
                        .data.ToList();

                    return new Result<List<Tasks>>
                    {
                        success = true,
                        message = "Ok",
                        data = info
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Tasks>>
                {
                    success = false,
                    message = "Internal error"
                };
            }
        }

        public async Task<Result<List<Tasks>>> MemberTaskInfo(int taskId)
        {
            try
            {
                var header = new AuthenticationHeaderValue("Bearer", Keys.accessToken);
                _client.request.DefaultRequestHeaders.Authorization = header;

                using (response = await _client.request.GetAsync($"membertasks/task-info?taskid={taskId}"))
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                    | response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        var refresh = new RefreshToken
                        {
                            refresh_token = Keys.refreshToken
                        };

                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.success)
                        {
                            Keys.accessToken = token.token;
                            Keys.refreshToken = token.refresh_token;
                        }

                        header = new AuthenticationHeaderValue("Bearer", Keys.accessToken);
                        _client.request.DefaultRequestHeaders.Authorization = header;

                        response = await _client.request.GetAsync($"membertasks/task-info?taskid={taskId}");
                    }

                    string result = await response.Content.ReadAsStringAsync();

                    var info = JsonConvert.DeserializeObject<Result<List<Tasks>>>(result)!
                        .data.ToList();

                    return new Result<List<Tasks>>
                    {
                        success = true,
                        message = "Ok",
                        data = info
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Result<List<Tasks>>
                {
                    success = false,
                    message = "Internal error"
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
                    tsk_title = data.tsk_title,
                    tsk_category = data.tsk_category,
                    deadline = data.deadline,
                    priority = data.priority,
                    member = data.member,
                    tsk_note = data.tsk_note
                };

                // serializing
                var content = new StringContent
                    (
                        JsonConvert.SerializeObject(task),
                        encoding: Encoding.UTF8,
                        mediaType: "application/json"
                    );

                // adding headers
                var header = new AuthenticationHeaderValue("Bearer", Keys.accessToken);
                _client.request.DefaultRequestHeaders.Authorization = header;

                using (response = await _client.request.PostAsync("leadtasks/add-task", content))
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                        | response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        // call refresh token
                        var refresh = new RefreshToken
                        {
                            refresh_token = Keys.refreshToken
                        };

                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.success)
                        {
                            Keys.accessToken = token.token;
                            Keys.refreshToken = token.refresh_token;
                        }

                        header = new AuthenticationHeaderValue("Bearer", Keys.accessToken);
                        _client.request.DefaultRequestHeaders.Authorization = header;

                        response = await _client.request.GetAsync("leadtasks/add-task");
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
                    success = false,
                    message = "Internal error"
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
                    tsk_id = data.tsk_id,
                    tsk_title = data.tsk_title,
                    tsk_category = data.tsk_category,
                    deadline = data.deadline,
                    priority = data.priority,
                    member = data.member,
                    tsk_note = data.tsk_note
                };

                var content = new StringContent
                    (
                        JsonConvert.SerializeObject(task),
                        encoding: Encoding.UTF8,
                        mediaType: "application/json"
                    );


                var header = new AuthenticationHeaderValue("Bearer", Keys.accessToken);
                _client.request.DefaultRequestHeaders.Authorization = header;

                using (response = await _client.request.PostAsync("leadtasks/edit-task", content))
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                        | response.StatusCode == HttpStatusCode.Forbidden)
                    {

                        var refresh = new RefreshToken
                        {
                            refresh_token = Keys.refreshToken
                        };

                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.success)
                        {
                            Keys.accessToken = token.token;
                            Keys.refreshToken = token.refresh_token;
                        }

                        header = new AuthenticationHeaderValue("Bearer", Keys.accessToken);
                        _client.request.DefaultRequestHeaders.Authorization = header;

                        response = await _client.request.GetAsync("leadtasks/edit-task");
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
                    success = false,
                    message = "Internal error"
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
                    tsk_id = data.tsk_id
                };

                var content = new StringContent
                    (
                        JsonConvert.SerializeObject(task),
                        encoding: Encoding.UTF8,
                        mediaType: "application/json"
                    );


                var header = new AuthenticationHeaderValue("Bearer", Keys.accessToken);
                _client.request.DefaultRequestHeaders.Authorization = header;

                using (response = await _client.request.PostAsync("leadtasks/delete-task", content))
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                        | response.StatusCode == HttpStatusCode.Forbidden)
                    {

                        var refresh = new RefreshToken
                        {
                            refresh_token = Keys.refreshToken
                        };

                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.success)
                        {
                            Keys.accessToken = token.token;
                            Keys.refreshToken = token.refresh_token;
                        }

                        header = new AuthenticationHeaderValue("Bearer", Keys.accessToken);
                        _client.request.DefaultRequestHeaders.Authorization = header;

                        response = await _client.request.GetAsync("leadtasks/delete-task");
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
                    success = false,
                    message = "Internal error"
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
                    tsk_id = data.tsk_id,
                    message = data.message
                };

                var content = new StringContent
                    (
                        JsonConvert.SerializeObject(task),
                        encoding: Encoding.UTF8,
                        mediaType: "application/json"
                    );


                var header = new AuthenticationHeaderValue("Bearer", Keys.accessToken);
                _client.request.DefaultRequestHeaders.Authorization = header;

                using (response = await _client.request.PostAsync("leadtasks/send-remind", content))
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                        | response.StatusCode == HttpStatusCode.Forbidden)
                    {

                        var refresh = new RefreshToken
                        {
                            refresh_token = Keys.refreshToken
                        };

                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.success)
                        {
                            Keys.accessToken = token.token;
                            Keys.refreshToken = token.refresh_token;
                        }

                        header = new AuthenticationHeaderValue("Bearer", Keys.accessToken);
                        _client.request.DefaultRequestHeaders.Authorization = header;

                        response = await _client.request.GetAsync("leadtasks/send-remind");
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
                    success = false,
                    message = "Internal error"
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
                    tsk_id = data.tsk_id,
                    user_note = data.user_note
                };

                var content = new StringContent
                    (
                        JsonConvert.SerializeObject(task),
                        encoding: Encoding.UTF8,
                        mediaType: "application/json"
                    );


                var header = new AuthenticationHeaderValue("Bearer", Keys.accessToken);
                _client.request.DefaultRequestHeaders.Authorization = header;

                using (response = await _client.request.PostAsync("membertasks/write-note", content))
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                        | response.StatusCode == HttpStatusCode.Forbidden)
                    {

                        var refresh = new RefreshToken
                        {
                            refresh_token = Keys.refreshToken
                        };

                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.success)
                        {
                            Keys.accessToken = token.token;
                            Keys.refreshToken = token.refresh_token;
                        }

                        header = new AuthenticationHeaderValue("Bearer", Keys.accessToken);
                        _client.request.DefaultRequestHeaders.Authorization = header;

                        response = await _client.request.GetAsync("membertasks/write-note");
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
                    success = false,
                    message = "Internal error"
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
                    tsk_id = data.tsk_id
                };

                var content = new StringContent
                    (
                        JsonConvert.SerializeObject(task),
                        encoding: Encoding.UTF8,
                        mediaType: "application/json"
                    );


                var header = new AuthenticationHeaderValue("Bearer", Keys.accessToken);
                _client.request.DefaultRequestHeaders.Authorization = header;

                using (response = await _client.request.PostAsync("membertasks/mark-done", content))
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                        | response.StatusCode == HttpStatusCode.Forbidden)
                    {

                        var refresh = new RefreshToken
                        {
                            refresh_token = Keys.refreshToken
                        };

                        var token = await _authService.RefreshTokenAsync(refresh);
                        if (token.success)
                        {
                            Keys.accessToken = token.token;
                            Keys.refreshToken = token.refresh_token;
                        }

                        header = new AuthenticationHeaderValue("Bearer", Keys.accessToken);
                        _client.request.DefaultRequestHeaders.Authorization = header;

                        response = await _client.request.GetAsync("membertasks/mark-done");
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
                    success = false,
                    message = "Internal error"
                };
            }
        }
    }
}