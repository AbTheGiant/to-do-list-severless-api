using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace SeverlessApi
{
    public static class ToDoApi

    {
        static List<ToDoItem> items = new List<ToDoItem>();
        [FunctionName("CreateToDoItem")]
        public static async Task<IActionResult> CreateTodoItems(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [CosmosDB(databaseName:"todolistapi", collectionName: "todolistcontainer", ConnectionStringSetting = "CosmosDBConnectionString")] ToDoItem todo,
            ILogger log)
        {
            log.LogInformation("Creating ToDo Items");

          
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<TodoCreateModel>(requestBody);
            

            var todo = new ToDoItem(){TaskDescription = input.TaskDescription};
            items.Add(todo);
            return new OkObjectResult(todo);
        }

         [FunctionName("GetToDoItem")]
        public static  IActionResult GetTodoItems(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Getting ToDo Items");
            return new OkObjectResult(items);
        }

         [FunctionName("GetToDoItemById")]
            public static async Task<IActionResult> GetTodoItemById(
                [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo{id}")] HttpRequest req,
                ILogger log, string id)
            {
                log.LogInformation("Getting ToDo Item ");

                var todo = items.FirstOrDefault(t => t.Id == id);
                if (todo == null){
                    return new NotFoundResult();
                }
                return new OkObjectResult(todo);
            }

        
         [FunctionName("UdpateToDoitem")]
            public static async Task<IActionResult> UpdateToDo(
                [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo{id}")] HttpRequest req,
                ILogger log, string id)
            {
                log.LogInformation("updating ToDo Item ");

                var todo = items.FirstOrDefault(t => t.Id == id);
                if (todo == null){
                    return new NotFoundResult();
                }


                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var updated = JsonConvert.DeserializeObject<TodoUpdateModel>(requestBody);

                todo.isCompleted= updated.isCompleted;
                if(!string.IsNullOrEmpty(updated.TaskDescription)){
                    todo.TaskDescription=updated.TaskDescription;
                }
                return new OkObjectResult(todo);
            }



          [FunctionName("DeleteToDoItem")]
            public static async Task<IActionResult> DeleteTodoItem(
                [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo{id}")] HttpRequest req,
                ILogger log, string id)
            {
                log.LogInformation("Deleting ToDo Item ");

                var todo = items.FirstOrDefault(t => t.Id == id);

                if (todo == null){
                    return new NotFoundResult();
                }
                items.Remove(todo);
                return new OkResult();
            }
        }
}
