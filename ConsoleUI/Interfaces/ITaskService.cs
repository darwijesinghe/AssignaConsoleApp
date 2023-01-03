using ConsoleUI.Models;
using ConsoleUI.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleUI.Interfaces
{
    public interface ITaskService
    {
        // get team members
        Task<Result<List<Users>>> TeamMembers();

        // get categories
        Task<Result<List<Categories>>> AllCategories();

        // get priorities
        Task<Result<List<Priorities>>> Priorities();

        // all tasks
        Task<Result<List<Tasks>>> AllTasks();

        // pending taks
        Task<Result<List<Tasks>>> Pendings();

        // completed tasks
        Task<Result<List<Tasks>>> Completed();

        // high priority tasks
        Task<Result<List<Tasks>>> HighPriority();

        // medium priority tasks
        Task<Result<List<Tasks>>> MediumPriority();

        // low priority tasks
        Task<Result<List<Tasks>>> LowPriority();

        // task infomation
        Task<Result<List<Tasks>>> LeadTaskInfo(int taskId);

        Task<Result<List<Tasks>>> MemberTaskInfo(int taskId);

        // add a new task
        Task<Result> SaveTaskAsync(AddTask data);

        // edit a task
        Task<Result> EditTaskAsync(EditTask data);

        // delete a task
        Task<Result> DeleteTaskAsync(DeleteTask data);

        // send remind
        Task<Result> SendRemind(SendEmail data);

        // add task note
        Task<Result> AddTaskNoteAsync(AddNote data);

        // mark as done
        Task<Result> MarkasDoneAsync(MarkDone data);
    }
}
