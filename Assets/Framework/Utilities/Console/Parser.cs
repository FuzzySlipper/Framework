﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using task = System.Threading.Tasks;

namespace PixelComrades.Debugging
{
    [Serializable]
    public class Parser
    {
        private static Dictionary<string, Owner> idToOwner = new Dictionary<string, Owner>();

        public static List<Owner> Owners
        {
            get
            {
                List<Owner> owners = new List<Owner>();

                foreach (var owner in idToOwner)
                {
                    owners.Add(owner.Value);
                }

                return owners;
            }
        }

        public static void Register(object owner, int id)
        {
            Register(owner, id.ToString());
        }

        public static void Register(object owner, string id)
        {
            if (!idToOwner.TryGetValue(id, out Owner ownerValue))
            {
                ownerValue = new Owner();
                idToOwner.Add(id, ownerValue);
            }
            
            ownerValue.owner = owner;
            ownerValue.id = id;

            ownerValue.FindMethods();
            ownerValue.FindProperties();
            ownerValue.FindFields();
        }

        public static void Initialize()
        {
            Library.FindCategories();
            Library.FindCommands();
            Converter.FindConverters();
        }

        public static void Unregister(object owner)
        {
            List<Owner> owners = Owners;
            for (int i = 0; i < owners.Count; i++)
            {
                if (owners[i].owner == owner)
                {
                    Unregister(owners[i].id);
                    return;
                }
            }
        }

        public static void Unregister(int id)
        {
            Unregister(id.ToString());
        }

        public static void Unregister(string id)
        {
            //remove this object as an owner for any methods
            if (idToOwner.ContainsKey(id))
            {
                idToOwner.Remove(id);
            }
        }

        public static async Task<object> Run(string input)
        {
            //if input starts with id flag
            //remove the id flag and store it separately
            string id = null;
            if (input.StartsWith("@"))
            {
                id = input.Substring(1, input.IndexOf(' ') - 1);
                input = input.Replace("@" + id + " ", "");
            }

            foreach (ConsoleCommand command in Library.Commands)
            {
                bool nameMatch = false;
                string commandInput = null;
                foreach (var name in command.Names)
                {
                    if (input.StartsWith(name))
                    {
                        nameMatch = true;
                        commandInput = name;
                        break;
                    }
                }

                //check
                if (nameMatch)
                {
                    string text = input.Replace(commandInput, "");
                    List<string> parameters = GetParameters(text);
                    if (command.Matches(parameters, out object[] converted))
                    {
                        object owner = FindOwner(command, id);
                        if (owner == null && !command.IsStatic)
                        {
                            //this was an instance method that didnt have an id
                            return new NullReferenceException("Couldn't find owner with ID " + id);
                        }

                        //try to exec
                        try
                        {
                            object result = command.Invoke(owner, converted);
                            if (result is System.Threading.Tasks.Task)
                            {
                                System.Threading.Tasks.Task task = result as System.Threading.Tasks.Task;
                                await task.ConfigureAwait(false);
                                return task.GetType().GetProperty("Result").GetValue(task);
                            }
                            else
                            {
                                return result;
                            }
                        }
                        catch (Exception exception)
                        {
                            return exception;
                        }
                    }
                }
            }

            return "Command not found";
        }

        public static object FindOwner(ConsoleCommand consoleCommand, string id)
        {
            if (id == null) return null;
            if (consoleCommand.IsStatic) return null;

            string memberName = consoleCommand.Name;
            if (idToOwner.TryGetValue(id, out Owner owner))
            {
                for (int i = 0; i < owner.methods.Count; i++)
                {
                    if (owner.methods[i].name == memberName)
                    {
                        return owner.owner;
                    }
                }
                for (int i = 0; i < owner.properties.Count; i++)
                {
                    if (owner.properties[i].name == memberName)
                    {
                        return owner.owner;
                    }
                }
                for (int i = 0; i < owner.fields.Count; i++)
                {
                    if (owner.fields[i].name == memberName)
                    {
                        return owner.owner;
                    }
                }
            }

            return null;
        }

        private static List<string> GetParameters(string input)
        {
            List<string> parameters = Regex.Matches(input, @"[\""].+?[\""]|[^ ]+").Cast<Match>().Select(x => x.Value).ToList();
            for (int i = 0; i < parameters.Count; i++)
            {
                if (parameters[i].StartsWith("\"") && parameters[i].EndsWith("\""))
                {
                    parameters[i] = parameters[i].TrimStart('\"').TrimEnd('\"');
                }
            }
            return parameters;
        }
    }
}
