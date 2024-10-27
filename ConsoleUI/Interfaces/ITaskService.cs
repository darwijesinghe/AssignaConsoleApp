using ConsoleUI.Models;
using ConsoleUI.Response;
using System.Collections.Generic;
using System.Threading.Tasks;
using Task = ConsoleUI.Models.Task;

namespace ConsoleUI.Interfaces
{
    /// <summary>
    /// Interface for common tasks operations
    /// </summary>
    public interface ITaskService
    {
        /// <summary>
        /// Retrieves a list of team members.
        /// </summary>
        /// <returns>
        /// A list of <see cref="User"/> representing the team members.
        /// </returns>
        Task<Result<List<User>>> TeamMembers();

        /// <summary>
        /// Retrieves a list of all categories.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Categories"/> representing all categories.
        /// </returns>
        Task<Result<List<Categories>>> AllCategories();

        /// <summary>
        /// Retrieves a list of all priority levels.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Priorities"/> representing all priority levels.
        /// </returns>
        Task<Result<List<Priorities>>> Priorities();

        /// <summary>
        /// Retrieves a list of all tasks related data based on the user.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Task"/> containing all tasks.
        /// </returns>
        Task<Result<List<Task>>> AllTasks();

        /// <summary>
        /// Retrieves a list of all pending tasks related data based on the user.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Task"/> containing all pending tasks.
        /// </returns>
        Task<Result<List<Task>>> Pendings();

        /// <summary>
        /// Retrieves a list of all completed tasks related data based on the user.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Task"/> containing all completed tasks.
        /// </returns>
        Task<Result<List<Task>>> Completed();

        /// <summary>
        /// Retrieves a list of all high priority tasks related data based on the user.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Task"/> containing all high priority tasks.
        /// </returns>
        Task<Result<List<Task>>> HighPriority();

        /// <summary>
        /// Retrieves a list of all medium priority tasks related data based on the user.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Task"/> containing all medium priority tasks.
        /// </returns>
        Task<Result<List<Task>>> MediumPriority();

        /// <summary>
        /// Retrieves a list of all low priority tasks related data based on the user.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Task"/> containing all low  priority tasks.
        /// </returns>
        Task<Result<List<Task>>> LowPriority();

        /// <summary>
        /// Retrieves detailed information about a specific task of the team-lead user role.
        /// </summary>
        /// <param name="taskId">The identifier of the task for which to retrieve information.</param>
        /// <returns>
        /// A list of <see cref="Task"/> containing the task information.
        /// </returns>
        Task<Result<List<Task>>> LeadTaskInfo(int taskId);

        /// <summary>
        /// Retrieves detailed information about a specific task of the team-member user role.
        /// </summary>
        /// <param name="taskId">The identifier of the task for which to retrieve information.</param>
        /// <returns>
        /// A list of <see cref="Task"/> containing the task information.
        /// </returns>
        Task<Result<List<Task>>> MemberTaskInfo(int taskId);

        /// <summary>
        /// Saves a task.
        /// </summary>
        /// <param name="data">The data containing the task information to be saved.</param>
        /// <returns>
        /// A <see cref="Result"/> indicating the outcome of the save operation.
        /// </returns>
        Task<Result> SaveTaskAsync(AddTask data);

        /// <summary>
        /// Edits an existing task.
        /// </summary>
        /// <param name="data">The data containing the updated task information.</param>
        /// <returns>
        /// A <see cref="Result"/> indicating the outcome of the edit operation.
        /// </returns>
        Task<Result> EditTaskAsync(EditTask data);

        /// <summary>
        /// Deletes a task.
        /// </summary>
        /// <param name="data">The data containing the task identifier to be deleted.</param>
        /// <returns>
        /// A <see cref="Result"/> indicating the outcome of the delete operation.
        /// </returns>
        Task<Result> DeleteTaskAsync(DeleteTask data);

        /// <summary>
        /// Sends an email to the specified recipient.
        /// </summary>
        /// <param name="data">The data containing the email information.</param>
        /// <returns>
        /// A <see cref="Result"/> representing the result of the email sending operation.
        /// </returns>
        Task<Result> SendRemind(SendEmail data);

        /// <summary>
        /// Adds a note to a task.
        /// </summary>
        /// <param name="data">The data containing the task information and the note to be added.</param>
        /// <returns>
        /// A <see cref="Result"/> indicating the outcome of the add note operation.
        /// </returns>
        Task<Result> AddTaskNoteAsync(AddNote data);

        /// <summary>
        /// Marks a task as done.
        /// </summary>
        /// <param name="data">The data containing the task identifier to be marked as done.</param>
        /// <returns>
        /// A a <see cref="Result"/> indicating the outcome of the mark as done operation.
        /// </returns>
        Task<Result> MarkasDoneAsync(MarkDone data);
    }
}
