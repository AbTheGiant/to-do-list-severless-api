// Copyright 2022 abiolaolajide
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// modeel for to do list

using System;
namespace SeverlessApi{
    public class ToDoItem{

        public string Id {get;set;} = Guid.NewGuid().ToString("n");
        public string Name{get; set;}
        public DateTime createdTime {get;set;}= DateTime.UtcNow;
        public bool isCompleted {get;set;}
        public string TaskDescription{get;set;}
        
        }

    public class TodoCreateModel{
        public string TaskDescription {get;set;}
    }

    public class TodoUpdateModel{
        public bool isCompleted {get;set;}
        public string TaskDescription {get;set;}

    }
        
}
