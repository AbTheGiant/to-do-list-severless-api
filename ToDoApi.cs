
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Documents.Client;
using System.Collections.Generic;
using System.Linq;


namespace SeverlessApi
{
    public static class ToDoApi

    {
        static List<ToDoItem> items = new List<ToDoItem>();
        [FunctionName("CreateToDoItemAbiola")]
        public static async Task<IActionResult> CreateToDoItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [CosmosDB(databaseName:"testtodo", collectionName: "abiolatest", ConnectionStringSetting = "CosmosDBConnectionString" )] IAsyncCollector<object> todoitems,
            ILogger log)
        {

            try{

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var input = JsonConvert.DeserializeObject<ToDoItem>(requestBody);
                var todoitem = new ToDoItem
                {
                    Name = input.Name,
                    TaskDescription = input.TaskDescription,
                    Category = input.Category
                    
                };
                await todoitems.AddAsync(todoitem);

                return new OkObjectResult(todoitem);
                
            }
            catch (Exception ex)
                {
                log.LogError($"Couldn't insert item. Exception thrown: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                 }
        }

    
        [FunctionName("GetToDoItems")]
        public static  async Task<IActionResult> GetToDoItems(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
            [CosmosDB(
                databaseName: "testtodo",
                collectionName: "abiolatest",
                ConnectionStringSetting = "CosmosDBConnectionString",
                SqlQuery ="SELECT * FROM c ")] IEnumerable<ToDoItem> toDoItem,
            ILogger log)
        {
            if (toDoItem == null)
            {
                return null;
            }
            

            return new OkObjectResult(toDoItem);
        }


        [FunctionName("GetToDoItemsById")]
        public static  async Task<IActionResult> GetToDoItemsById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req,
            [CosmosDB(
                databaseName: "testtodo",
                collectionName: "abiolatest",
                ConnectionStringSetting = "CosmosDBConnectionString",
                SqlQuery ="SELECT * FROM c WHERE c.id={id} ")] IEnumerable<ToDoItem> toDoItem,
            ILogger log,
            string id)
        {
            if (toDoItem == null)
            {
                return null;
            }

            return new OkObjectResult(toDoItem);
        }

        [FunctionName("UpdateToDoItesm")]
        public static  async Task<IActionResult> UpdateToDoItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
                [CosmosDB(ConnectionStringSetting = "CosmosDBConnectionString")] DocumentClient client,
                ILogger log,
                string id)
        {


            
            
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    var updated = JsonConvert.DeserializeObject<TodoUpdateModel>(requestBody);
                    Uri collectionUri = UriFactory.CreateDocumentCollectionUri("testtodo", "abiolatest");
                    var document = client.CreateDocumentQuery(collectionUri).Where(c => c.Id == id)
                                    .AsEnumerable().FirstOrDefault();
                    if (document == null)
                    {
                        return new NotFoundResult();
                    }
                    

                    document.SetPropertyValue("isCompleted", updated.isCompleted);
                    if (!string.IsNullOrEmpty(updated.TaskDescription))
                    {
                        document.SetPropertyValue("TaskDescription", updated.TaskDescription);
                    }

                    await client.ReplaceDocumentAsync(document);

                    /* var todo = new Todo()
                    {
                        Id = document.GetPropertyValue<string>("id"),
                        CreatedTime = document.GetPropertyValue<DateTime>("CreatedTime"),
                        TaskDescription = document.GetPropertyValue<string>("TaskDescription"),
                        IsCompleted = document.GetPropertyValue<bool>("IsCompleted")
                    };*/

                    // an easier way to deserialize a Document
                    ToDoItem todo2 = (dynamic)document;

                    return new OkObjectResult(todo2);

        }    

        [FunctionName("DeleteTodoItem")]
       /*  public static async Task<IActionResult> DeleteTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo4/{id}")]HttpRequest req,
            [CosmosDB(ConnectionStringSetting = "CosmosDBConnectionString")] DocumentClient client,
            ILogger log, string id)
        {
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("testtodo", "abiolatest");
            var document = client.CreateDocumentQuery(collectionUri).Where(t => t.Id == id)
                    .AsEnumerable().FirstOrDefault();
            if (document == null)
            {
                return new NotFoundResult();
            }
           // await client.DeleteDocumentAsync(document.SelfLink);

            await client.DeleteDocumentAsync(document, new RequestOptions { PartitionKey = new Microsoft.Azure.Documents.PartitionKey("Id") });
            return new OkResult();
        }      */  

        public static async Task<IActionResult> DeleteTodo(
        HttpRequest req, 
        [CosmosDB(ConnectionStringSetting = "CosmosDBConnectionString")] DocumentClient client, 
        ILogger log, string Id)
        {
        var option = new FeedOptions { EnableCrossPartitionQuery = true };
        var collectionUri = UriFactory.CreateDocumentCollectionUri("testtodo", "abiolatest");
        
        var document = client.CreateDocumentQuery(collectionUri, option).Where(t => t.Id == Id)
                .AsEnumerable().FirstOrDefault();
        
        if (document == null)
        {
            return new NotFoundResult();
        }
        await client.DeleteDocumentAsync(document.SelfLink, new RequestOptions
        {
            PartitionKey = new Microsoft.Azure.Documents.PartitionKey("id")
        });
        
        return new OkResult();
        }
         public class ToDoItem{

        public string Id {get;set;} = Guid.NewGuid().ToString("n");
        public string Name{get; set;}
        public DateTime createdTime {get;set;}= DateTime.UtcNow;
        public bool isCompleted {get;set;}
        public string TaskDescription{get;set;}
        public string Category{get;set;}
        }

}}
