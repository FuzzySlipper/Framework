using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PixelComrades.Debugging
{
    [Serializable]
    public sealed class Library
    {
        private static List<ConsoleCommand> commands = null;
        private static List<Category> categories = null;

        public static List<ConsoleCommand> Commands
        {
            get
            {
                if (commands == null)
                {
                    FindCommands();
                }

                return commands;
            }
        }

        public static List<Category> Categories
        {
            get
            {
                if (categories == null)
                {
                    FindCategories();
                }

                return categories;
            }
        }

        public static void FindCategories()
        {
            if (categories == null)
            {
                categories = new List<Category>();
                HashSet<Type> typesWithoutCategories = new HashSet<Type>();
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    Type[] types = assembly.GetTypes();
                    foreach (Type type in types)
                    {
                        Category category = Category.Create(type);
                        if (category != null)
                        {
                            if (TryGetCategory(category.Name, out Category existingCategory))
                            {
                                existingCategory.Commands.AddRange(category.Commands);
                            }
                            else
                            {
                                categories.Add(category);
                            }
                        }
                        else
                        {
                            typesWithoutCategories.Add(type);
                        }
                    }
                }

                //sort alphabetically
                categories = categories.OrderBy(x => x.Name).ToList();

                if (typesWithoutCategories.Count > 0)
                {
                    Category uncategorized = Category.CreateUncategorized();
                    List<ConsoleCommand> commands = Commands;
                    foreach (ConsoleCommand command in commands)
                    {
                        if (typesWithoutCategories.Contains(command.Owner))
                        {
                            uncategorized.Commands.Add(command);
                        }
                    }

                    if (uncategorized.Commands.Count > 0)
                    {
                        categories.Add(uncategorized);
                    }
                }
            }
        }

        public static bool TryGetCategory(string name, out Category category)
        {
            category = null;
            if (categories == null)
            {
                return false;
            }

            for (int i = 0; i < categories.Count; i++)
            {
                if (categories[i].Name == name)
                {
                    category = categories[i];
                    return true;
                }
            }

            return false;
        }

        public static void FindCommands()
        {
            if (commands == null)
            {
                commands = new List<ConsoleCommand>();
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        var methods = type.GetMethods();
                        foreach (var method in methods)
                        {
                            ConsoleCommand consoleCommand = ConsoleCommand.Create(method, type);
                            if (consoleCommand != null)
                            {
                                commands.Add(consoleCommand);
                            }
                        }

                        var properties = type.GetProperties();
                        foreach (var property in properties)
                        {
                            ConsoleCommand consoleCommand = ConsoleCommand.Create(property, type);
                            if (consoleCommand != null)
                            {
                                commands.Add(consoleCommand);
                            }
                        }

                        var fields = type.GetFields();
                        foreach (var field in fields)
                        {
                            ConsoleCommand consoleCommand = ConsoleCommand.Create(field, type);
                            if (consoleCommand != null)
                            {
                                commands.Add(consoleCommand);
                            }
                        }
                    }
                }
            }
        }
    }
}
