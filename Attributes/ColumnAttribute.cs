﻿using System;

namespace Handy
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
        }

        /// <summary>
        /// Имя поля в таблице базы данных
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        /// Указывает внешний ключ текущей таблицы для связи с первичным ключем другой таблицы
        /// </summary>
        public string ForeignKeyName { get; set; }

        /// <summary>
        /// Указывает ключ для поиска по внешней таблицы (Работает только вместе с ForeignKeyName)
        /// </summary>
        /// <remarks> Если ключ не указан, то поиск выполняется по первичному ключу</remarks>
        public string ForeignTableKeyName { get; set; }

        /// <summary>
        /// Указывает тип внешней таблицы
        /// </summary>
        public Type ForeignTable { get; set; }

        /// <summary>
        /// Указывает, что колонка самозаполняющейся
        /// </summary>
        public bool IsAutoGenerated { get; set; }

        /// <summary>
        /// Указывает, что колонка является первичным ключем
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Указывает, что колонка является внешним ключем
        /// </summary>
        public bool IsForeignKey { get; set; }

        /// <summary>
        /// Указывает, что колонка является внешней колонкой из другой таблицы
        /// </summary>
        public bool IsForeignColumn { get; set; }

        /// <summary>
        /// Указывает, что колонка является таблицей
        /// </summary>
        public bool IsTable { get; set; }

        /// <summary>
        /// Првоеряет, не является ли поле внешним или таблице, или с автогенерацией
        /// </summary>
        internal bool IsValid => !(IsAutoGenerated || IsForeignColumn || IsTable);
    }
}