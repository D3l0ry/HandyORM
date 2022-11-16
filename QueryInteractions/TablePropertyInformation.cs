using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Handy.QueryInteractions
{
    public class TablePropertyInformation
    {
        private readonly Type mr_TableType;
        private readonly TableAttribute mr_TableAttribute;
        private readonly KeyValuePair<PropertyInfo, ColumnAttribute> mr_PrimaryKeyProperty;
        private readonly KeyValuePair<PropertyInfo, ColumnAttribute>[] mr_Properties;

        public TablePropertyInformation(Type tableType, TableAttribute tableAttribute)
        {
            if (tableType is null)
            {
                throw new ArgumentNullException(nameof(tableType));
            }

            if (tableAttribute is null)
            {
                throw new ArgumentNullException(nameof(tableAttribute));
            }

            mr_TableType = tableType;

            mr_TableAttribute = tableAttribute;

            mr_Properties = GetProperties().ToArray();

            KeyValuePair<PropertyInfo, ColumnAttribute> primaryKeyProperty = mr_Properties
                .FirstOrDefault(currentPropertyValuePair => currentPropertyValuePair.Value.IsPrimaryKey);

            if (primaryKeyProperty.Key == null)
            {
                throw new NullReferenceException($"В таблице {mr_TableAttribute.Name} не существует первичного ключа!");
            }

            mr_PrimaryKeyProperty = primaryKeyProperty;
        }

        public KeyValuePair<PropertyInfo, ColumnAttribute> PrimaryKey => mr_PrimaryKeyProperty;

        public KeyValuePair<PropertyInfo, ColumnAttribute>[] Properties => mr_Properties;

        private IEnumerable<KeyValuePair<PropertyInfo, ColumnAttribute>> GetProperties()
        {
            IEnumerable<PropertyInfo> properties = mr_TableType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(currentProperty => currentProperty.CustomAttributes
                    .Any(currentPropertyAttribute => currentPropertyAttribute.AttributeType == typeof(ColumnAttribute)));

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

            if (!string.IsNullOrWhiteSpace(mr_TableAttribute.Schema))
            {
                tableName.Append($"[{mr_TableAttribute.Schema}].");
            }

            tableName.Append($"[{mr_TableAttribute.Name}]");

            return tableName.ToString();
        }

        public KeyValuePair<PropertyInfo, ColumnAttribute> GetProperty(int propertyIndex) => mr_Properties[propertyIndex];

        public KeyValuePair<PropertyInfo, ColumnAttribute> GetProperty(string propertyColumnName)
        {
            if (string.IsNullOrWhiteSpace(propertyColumnName))
            {
                throw new ArgumentNullException(nameof(propertyColumnName));
            }

            KeyValuePair<PropertyInfo, ColumnAttribute> selectedProperty = mr_Properties
                .FirstOrDefault(currentProperty => currentProperty.Value.Name == propertyColumnName);

            if (selectedProperty.Key is null)
            {
                throw new KeyNotFoundException($"Поле {propertyColumnName} не найдено");
            }

            return selectedProperty;
        }

        public KeyValuePair<PropertyInfo, ColumnAttribute> GetProperty(PropertyInfo property)
        {
            KeyValuePair<PropertyInfo, ColumnAttribute> selectedProperty = mr_Properties
                .FirstOrDefault(currentProperty => currentProperty.Key == property);

            if (selectedProperty.Key is null)
            {
                throw new KeyNotFoundException($"Поле {property.Name} не найдено");
            }

            return selectedProperty;
        }

        public ColumnAttribute GetPropertyColumn(int propertyIndex) => GetProperty(propertyIndex).Value;

        public ColumnAttribute GetPropertyColumn(string propertyColumnName) => GetProperty(propertyColumnName).Value;

        public ColumnAttribute GetPropertyColumn(PropertyInfo property) => GetProperty(property).Value;

        public string GetPropertyName(in KeyValuePair<PropertyInfo, ColumnAttribute> property)
        {
            StringBuilder propertyName = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(mr_TableAttribute.Schema))
            {
                propertyName.Append($"[{mr_TableAttribute.Schema}].");
            }

            propertyName.Append($"[{mr_TableAttribute.Name}].[{property.Value.Name}]");

            return propertyName.ToString();
        }

        public string GetForeignKeyName(in KeyValuePair<PropertyInfo, ColumnAttribute> property)
        {
            if (string.IsNullOrWhiteSpace(property.Value.ForeignKeyName))
            {
                throw new ArgumentNullException($"Получение имени внешнего ключа в {property.Key.Name} невозможно! Поле внешнего ключа является пустым");
            }

            StringBuilder foreignKeyName = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(mr_TableAttribute.Schema))
            {
                foreignKeyName.Append($"[{mr_TableAttribute.Schema}].");
            }

            foreignKeyName
                .Append($"[{mr_TableAttribute.Name}].[{property.Value.ForeignKeyName}]");

            return foreignKeyName.ToString();
        }

        public string GetForeignKeyName(ColumnAttribute propertyColumn)
        {
            if (string.IsNullOrWhiteSpace(propertyColumn.ForeignKeyName))
            {
                throw new ArgumentNullException($"Получение имени внешнего ключа в {propertyColumn.Name} невозможно! Поле внешнего ключа является пустым");
            }

            StringBuilder foreignKeyName = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(mr_TableAttribute.Schema))
            {
                foreignKeyName.Append($"[{mr_TableAttribute.Schema}].");
            }

            foreignKeyName
                .Append($"[{mr_TableAttribute.Name}].[{propertyColumn.ForeignKeyName}]");

            return foreignKeyName.ToString();
        }

        public string GetTableProperties()
        {
            StringBuilder stringProperties = new StringBuilder("(");

            foreach (KeyValuePair<PropertyInfo, ColumnAttribute> currentKeyValuePair in mr_Properties)
            {
                ColumnAttribute columnAttribute = currentKeyValuePair.Value;

                if (!columnAttribute.IsValid)
                {
                    continue;
                }

                stringProperties.Append($"{GetPropertyName(currentKeyValuePair)}, ");
            }

            stringProperties[stringProperties.Length - 2] = ')';

            return stringProperties.ToString();
        }

        public string GetTablePropertiesValue(object table)
        {
            StringBuilder stringPropertiesValue = new StringBuilder("(");

            foreach (KeyValuePair<PropertyInfo, ColumnAttribute> currentKeyValuePair in mr_Properties)
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

        public string GetTablesPropertiesValue(IEnumerable<object> tables)
        {
            StringBuilder stringPropertiesValue = new StringBuilder();

            foreach (object table in tables)
            {
                string newTablePropertiesValue = GetTablePropertiesValue(table);

                stringPropertiesValue.Append(newTablePropertiesValue);
                stringPropertiesValue.Append(',');
            }

            stringPropertiesValue[stringPropertiesValue.Length - 1] = ';';

            return stringPropertiesValue.ToString();
        }

        public string GetTablePropertiesNameAndValue(object table)
        {
            StringBuilder stringProperties = new StringBuilder("");

            foreach (KeyValuePair<PropertyInfo, ColumnAttribute> currentKeyValuePair in mr_Properties)
            {
                ColumnAttribute columnAttribute = currentKeyValuePair.Value;

                if (!columnAttribute.IsValid)
                {
                    continue;
                }

                stringProperties
                    .Append($"{GetPropertyName(currentKeyValuePair)} = {ConvertFieldQuery(currentKeyValuePair.Key.GetValue(table))}, ");
            }

            stringProperties.Remove(stringProperties.Length - 2, 2);

            return stringProperties.ToString();
        }

        public static string ConvertFieldQuery(object value)
        {
            if (value is null)
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
    }
}