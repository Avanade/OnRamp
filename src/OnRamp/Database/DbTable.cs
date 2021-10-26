// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using OnRamp.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace OnRamp.Database
{
    /// <summary>
    /// Represents the Database <b>Table</b> schema definition.
    /// </summary>
    public class DbTable
    {
        private string? _name;

        /// <summary>
        /// Create an alias from the name.
        /// </summary>
        /// <param name="name">The source name.</param>
        /// <returns>The alias.</returns>
        public static string CreateAlias(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            return new string(StringConverter.ToSentenceCase(name.Replace(" ", "").Replace("_", "").Replace("-", ""))!.Split(' ').Select(x => x.Substring(0, 1).ToLower(System.Globalization.CultureInfo.InvariantCulture).ToCharArray()[0]).ToArray());
        }

        /// <summary>
        /// Gets or sets the table name.
        /// </summary>
        public string? Name
        {
            get { return _name; }

            set
            {
                _name = value;
                if (!string.IsNullOrEmpty(_name) && string.IsNullOrEmpty(Alias))
                    Alias = CreateAlias(_name);
            }
        }

        /// <summary>
        /// Gets or sets the schema.
        /// </summary>
        public string? Schema { get; set; }

        /// <summary>
        /// Gets or sets the alias (automatically updated when the <see cref="Name"/> is set and the current alias value is <c>null</c>).
        /// </summary>
        public string? Alias { get; set; }

        /// <summary>
        /// Gets the fully qualified name schema.table name.
        /// </summary>
        public string? QualifiedName => $"[{Schema}].[{Name}]";

        /// <summary>
        /// Indicates whether the Table is actually a View.
        /// </summary>
        public bool IsAView { get; set; }

        /// <summary>
        /// Indicates whether the Table is considered reference data.
        /// </summary>
        public bool IsRefData { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DbColumn"/> list.
        /// </summary>
        public List<DbColumn> Columns { get; private set; } = new List<DbColumn>();

        /// <summary>
        /// Gets the primary key <see cref="DbColumn"/> list.
        /// </summary>
        public List<DbColumn> PrimaryKeyColumns { get; private set; } = new List<DbColumn>();
    }
}