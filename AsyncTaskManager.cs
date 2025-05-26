using System;
using System.Threading;
using System.Threading.Tasks;

Console.WriteLine("Start executing main.....");
var taskManager = new TaskManager();
taskManager.addTask("task1", new List<string> { "task2" });
taskManager.addTask("task2", new List<string> { "task3" });
taskManager.addTask("task3", new List<string> { "task4" });
taskManager.addTask("task4");
taskManager.buildDependency();
await taskManager.run();

public class UserTask
{
    // Unique id of this task
    public string TaskId;

    // List of task id's that this task is dependent on.
    public List<string> Dependencies = new();

    // List of task id's which are waiting for this task to complete.
    public List<string> Dependents = new();

    // Constructor
    public UserTask(string taskId)
    {
        TaskId = taskId;
    }

    // task that will execute in background.
    public static async Task<UserTask> DoWorkAsync(UserTask userTask, int sleepTimer)
    {
        await Task.Delay(sleepTimer);
        Console.WriteLine($"Task {userTask.TaskId} completed");
        return userTask;
    }
}

public class TaskManager
{
    // Map of task id to task object.
    private Dictionary<string, UserTask> TaskMap = new();

    // List of currently running tasks.
    private List<Task<UserTask>> readyList = new();

    // sleep timer for testing.
    public int sleepTimer = 5000;

    // Build dependency.
    public void buildDependency()
    {
        // Iterate over the TaskMap.
        foreach (KeyValuePair<string, UserTask> value in TaskMap)
        {
            // If no dependencies, add this task to readyList to start execution.
            if (value.Value.Dependencies.Count == 0)
            {
                readyList.Add(UserTask.DoWorkAsync(value.Value, sleepTimer));
            }
            else
            {
                // Update dependents info.
                foreach (string task in value.Value.Dependencies)
                {
                    TaskMap[task].Dependents.Add(value.Key);
                }
            }
        }
    }

    // Add task.
    public void addTask(string taskId, List<string>? dependencies = null)
    {
        // Create new task.
        var task = new UserTask(taskId);

        // If dependency list provided, add it to the object.
        if (dependencies != null)
        {
            task.Dependencies.AddRange(dependencies);
        }

        // Upadte the TaskMap.
        TaskMap[taskId] = task;
    }

    // Run method.
    public async Task run()
    {
        // Run until all the tasks have completed its execution.
        while (readyList.Count > 0)
        {
            // Get any task that is completed.
            var completedTask = await Task.WhenAny(readyList);

            // remove completed task from list.
            readyList.Remove(completedTask);

            // Get the task that is completed and add its Dependents that waiting for this task to get complted into the list to start execution.
            var task = await completedTask;
            foreach (var dependentTask in task.Dependents)
            {
                readyList.Add(UserTask.DoWorkAsync(TaskMap[dependentTask], sleepTimer));
            }
        }
        Console.WriteLine("All task are finished");
    }
}
