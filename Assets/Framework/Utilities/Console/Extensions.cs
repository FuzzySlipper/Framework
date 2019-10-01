﻿using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PixelComrades.Debugging
{
    internal static class Extensions
    {
        public static CommandAttribute GetCommand(this MemberInfo member)
        {
            try
            {
                CommandAttribute attribute = null;
                object[] attributes = member.GetCustomAttributes(typeof(CommandAttribute), false);
                for (int a = 0; a < attributes.Length; a++)
                {
                    if (attributes[a].GetType() == typeof(CommandAttribute))
                    {
                        attribute = attributes[a] as CommandAttribute;
                        return attribute;
                    }
                }
            }
            catch
            {
                //this could happen due to a TypeLoadException in builds
            }

            return null;
        }

        public static AliasAttribute[] GetAliases(this MemberInfo member)
        {
            try
            {
                List<AliasAttribute> aliases = new List<AliasAttribute>();
                object[] attributes = member.GetCustomAttributes(typeof(AliasAttribute), false);
                for (int a = 0; a < attributes.Length; a++)
                {
                    if (attributes[a].GetType() == typeof(AliasAttribute))
                    {
                        AliasAttribute attribute = attributes[a] as AliasAttribute;
                        aliases.Add(attribute);
                    }
                }

                return aliases.ToArray();
            }
            catch
            {
                //this could happen due to a TypeLoadException in builds
            }

            return new AliasAttribute[] { };
        }

        public static CategoryAttribute GetCategoryAttribute(this Type type)
        {
            try
            {
                CategoryAttribute attribute = null;
                object[] attributes = type.GetCustomAttributes(typeof(CategoryAttribute), false);
                for (int a = 0; a < attributes.Length; a++)
                {
                    if (attributes[a].GetType() == typeof(CategoryAttribute))
                    {
                        attribute = attributes[a] as CategoryAttribute;
                        return attribute;
                    }
                }
            }
            catch
            {
                //this could happen due to a TypeLoadException in builds
            }

            return null;
        }

        public static char GetCharFromKeyCode(this KeyCode keyCode)
        {
            int index = (int)keyCode;
            return (char)index;
        }
    }
}
