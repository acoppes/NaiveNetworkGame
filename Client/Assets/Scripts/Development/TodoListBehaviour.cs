using System;
using System.Collections.Generic;
using UnityEngine;

namespace Development
{
    [Serializable]
    public class TodoChecklistEntry
    {
        [TextArea(3, 15)]
        public string description;
    }
    
    [Serializable]
    public class TodoEntry
    {
        public string name;
        [TextArea(3, 15)]
        public string description;
        public List<TodoChecklistEntry> checklist = new List<TodoChecklistEntry>();
    }
    
    public class TodoListBehaviour : MonoBehaviour
    {
        public List<TodoEntry> todoList;
    }
}
