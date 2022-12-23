using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Handy.TableInteractions
{
    public class TableProperties : IEnumerable<KeyValuePair<PropertyInfo, ColumnAttribute>>
    {
        private readonly Type _TableType;
        private readonly TableAttribute _TableAttribute;
        private readonly KeyValuePair<PropertyInfo, ColumnAttribute> _PrimaryKeyProperty;
        private readonly KeyValuePair<PropertyInfo, ColumnAttribute>[] _Properties;

        public TableProperties(Type tableType, TableAttribute tableAttribute)
        {
            if (tableType == null)
            {
                throw new ArgumentNullException(nameof(tableType));
            }

            if (tableAttribute == null)
            {
                throw new ArgumentNullException(nameof(tableAttribute));
            }

            _TableType = tableType;
            _TableAttribute = tableAttribute;
            _Properties = GetProperties().ToArray();

            KeyValuePair<PropertyInfo, ColumnAttribute> primaryKeyProperty = _Properties
                .FirstOrDefault(currentPropertyValuePair => currentPropertyValuePair.Value.IsPrimaryKey);

            if (primaryKeyProperty.Key == null)
            {
                throw new NullReferenceException($"В таблице {_TableAttribute.Name} не существует первичного ключа!");
            }

            _PrimaryKeyProperty = primaryKeyProperty;
        }

        public KeyValuePair<PropertyInfo, ColumnAttribute> PrimaryKey => _PrimaryKeyProperty;

        private IEnumerable<KeyValuePair<PropertyInfo, ColumnAttribute>> GetProperties()
        {
            IEnumerable<PropertyInfo> properties = _TableType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(currentProperty => currentProperty.IsDefined(typeof(ColumnAttribute)));

            foreach (PropertyInfo currentProperty in properties)
            {
                ColumnAttribute currentAttribute = currentProperty.GetCustomAttribute<ColumnAttribute>();

                KeyValuePair<PropertyInfo, ColumnAttribute> newPropertyKeyValuePair =
                    new KeyValuePair<PropertyInfo, ColumnAttribute>(currentProperty, currentAttribute);

                yield return newPropertyKeyValuePair;
            }
        }

        public string GetTableName()
        {
            StringBuilder tableName = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(_TableAttribute.Schema))
            {
                tableName.Append($"[{_TableAttribute.Schema}].");
            }

            tableName.Append($"[{_TableAttribute.Name}]");

            return tableName.ToString();
        }

        public KeyValuePair<PropertyInfo, ColumnAttribute> GetProperty(int propertyIndex) => _Properties[propertyIndex];

        public KeyValuePair<PropertyInfo, ColumnAttribute> GetProperty(string propertyColumnName)
        {
            if (string.IsNullOrWhiteSpace(propertyColumnName))
            {
                throw new ArgumentNullException(nameof(propertyColumnName));
            }

            KeyValuePair<PropertyInfo, ColumnAttribute> selectedProperty = _Properties
                .FirstOrDefault(currentProperty => currentProperty.Value.Name == propertyColumnName);

            if (selectedProperty.Key == null)
            {
                throw new KeyNotFoundException($"Поле {propertyColumnName} не найдено");
            }

            return selectedProperty;
        }

        public KeyValuePair<PropertyInfo, ColumnAttribute> GetProperty(PropertyInfo property)
        {
            KeyValuePair<PropertyInfo, ColumnAttribute> selectedProperty = _Properties
                .FirstOrDefault(currentProperty => currentProperty.Key == property);

            if (selectedProperty.Key == null)
            {
                throw new KeyNotFoundException($"Поле {property.Name} не найдено");
            }

            return selectedProperty;
        }

        public string GetPropertyName(in KeyValuePair<PropertyInfo, ColumnAttribute> property)
        {
            ColumnAttribute propertyColumn = property.Value;

            if (propertyColumn.IsForeignColumn && propertyColumn.ForeignTable != null)
            {
                TableQueryCreator tableQueryCreator = TableQueryCreator
                    .GetInstance(propertyColumn.ForeignTable);

                KeyValuePair<PropertyInfo, ColumnAttribute> foreignProperty = tableQueryCreator.Properties
                    .GetProperty(propertyColumn.Name);

                return tableQueryCreator.Properties.GetPropertyName(foreignProperty);
            }

            StringBuilder propertyName = new StringBuilder(GetTableName());

            propertyName.Append($".[{property.Value.Name}]");

            return propertyName.ToString();
        }

        public string GetForeignKeyName(ColumnAttribute propertyColumn)
        {
            if (string.IsNullOrWhiteSpace(propertyColumn.ForeignKeyName))
            {
                throw new ArgumentNullException($"Получение имени внешнего ключа в {propertyColumn.Name} невозможно! Поле внешнего ключа является пустым");
            }

            StringBuilder foreignKeyName = new StringBuilder(GetTableName());

            foreignKeyName.Append($".[{propertyColumn.ForeignKeyName}]");

            return foreignKeyName.ToString();
        }

        public string GetTableProperties()
        {
            StringBuilder stringProperties = new StringBuilder("(");

            foreach (KeyValuePair<PropertyInfo, ColumnAttribute> currentKeyValuePair in _Properties)
            {
                ColumnAttribute columnAttribute = currentKeyValuePair.Value;

                if (!columnAttribute.IsValid)
                {
                    continue;
                }

                stringProperties.Append($"[{columnAttribute.Name}], ");
            }

            stringProperties[stringProperties.Length - 2] = ')';

            return stringProperties.ToString();
        }

        public string GetTablePropertiesValue(object table)
        {
            StringBuilder stringPropertiesValue = new StringBuilder("(");

            foreach (KeyValuePair<PropertyInfo, ColumnAttribute> currentKeyValuePair in _Properties)
            {
                ColumnAttribute columnAttribute = currentKeyValuePair.Value;

                if (!columnAttribute.IsValid)
                {
                    continue;
                }

                stringPropertiesValue.Append($"{ConvertFieldQuery(currentKeyValuePair.Key.GetValue(table))},");
            }

            stringPropertiesValue[stringPropertiesValue.Length - 1] = ')';

            return stringPropertiesValue.ToString();
        }

        public string GetTablePropertiesNameAndValue(object table)
        {
            StringBuilder stringProperties = new StringBuilder();

            foreach (KeyValuePair<PropertyInfo, ColumnAttribute> currentKeyValuePair in _Properties)
            {
                ColumnAttribute columnAttribute = currentKeyValuePair.Value;

                if (!columnAttribute.IsValid)
                {
                    continue;
                }

                stringProperties
                    .Append($"[{columnAttribute.Name}] = {ConvertFieldQuery(currentKeyValuePair.Key.GetValue(table))}, ");
            }

            stringProperties.Remove(stringProperties.Length - 2, 2);

            return stringProperties.ToString();
        }

        public static string ConvertFieldQuery(object value)
        {
            if (value == null)
            {
                return "NULL";
            }

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                return value.ToString();
                case TypeCode.Single:
                case TypeCode.Decimal:
                case TypeCode.Double:
                return value.ToString().Replace(',', '.');
                case TypeCode.Boolean:
                case TypeCode.String:
                case TypeCode.DateTime:
                case TypeCode.Object:
                return $"'{value}'";
                default:
                throw new NotSupportedException($"The constant for '{value}' is not supported");
            }
        }

        public IEnumerator<KeyValuePair<PropertyInfo, ColumnAttribute>> GetEnumerator() => _Properties.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _Properties.GetEnumerator();
    }
}