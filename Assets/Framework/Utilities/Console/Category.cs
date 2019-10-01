using System;
using System.Collections.Generic;

namespace PixelComrades.Debugging
{
    [Serializable]
    public class Category
    {
        private string name = "";
        private List<ConsoleCommand> commands = new List<ConsoleCommand>();

        public string Name
        {
            get
            {
                return name;
            }
        }

        public List<ConsoleCommand> Commands
        {
            get
            {
                return commands;
            }
        }

        private Category(string name)
        {
            this.name = name;
        }

        public static Category CreateUncategorized()
        {
            Category category = new Category("Uncategorized");
            return category;
        }

        public static Category Create(Type type)
        {
            CategoryAttribute attribute = type.GetCategoryAttribute();
            if (attribute == null) return null;

            Category category = new Category(attribute.name);
            List<ConsoleCommand> commands = Library.Commands;
            foreach (ConsoleCommand command in commands)
            {
                if (command.Owner == type)
                {
                    category.commands.Add(command);
                }
            }

            return category;
        }
    }
}