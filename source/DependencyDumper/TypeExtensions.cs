// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeExtensions.cs" company="Appccelerate">
//   Copyright (c) 2008-2014
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DependecyDumper
{
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Contains extension methods for Type.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Correctly formats the FullName of the specified type by taking generics into consideration.
        /// </summary>
        /// <param name="type">The type whose full name is formatted.</param>
        /// <returns>A correctly formatted full name.</returns>
        public static string NameToString(this Type type)
        {
            var index = type.Name.IndexOf('`');

            if (index < 0)
            {
                return type.Name;
            }

            var partName = type.Name.Substring(0, index);
            var genericArgumentNames = type.GetTypeInfo().GenericTypeArguments.Select(arg => NameToString(arg));
            return string.Concat(partName, "<", string.Join(",", genericArgumentNames), ">");
        }
    }
}