using System;
using System.Data;
using System.Linq;

namespace AutoModelMapping.Helpers
{
    public static class SqlHelp
    {
        public static string Convert(string sqlDataType, DataColumn dataColumn)
        {
            switch (sqlDataType)
            {
                case "System.String":
                case "System.Guid":
                    return "string";
                case "System.Boolean":
                    return dataColumn.AllowDBNull ? "bool?" : "bool";
                case "System.Byte":
                    return dataColumn.AllowDBNull ? "byte?" : "byte";
                case "System.Int16":
                case "System.Int32":
                    return dataColumn.AllowDBNull ? "int?" : "int";
                case "System.Int64":
                    return dataColumn.AllowDBNull ? "long?" : "long";
                case "System.Decimal":
                    return dataColumn.AllowDBNull ? "decimal?" : "decimal";
                case "System.Double":
                    return dataColumn.AllowDBNull ? "float?" : "float";
                case "System.DateTime":
                    return dataColumn.AllowDBNull ? "DateTime?" : "DateTime";
                default:
                    return "string";  
            }
        }

        public static string NetModel (string columnName, string dataType)
        {
            return $"\t\tpublic {dataType} {columnName.ColumnPrettify()} {{ get; set; }}{Environment.NewLine}";
        }

        public static string NetMapping(string prettyColumnName, string oldColumnName)
        {
            return
                $"\t\t\tthis.Property(x => x.{prettyColumnName}).HasColumnName(\"{oldColumnName}\");{Environment.NewLine}";
        }

        public static string TablePrettify(this string value)
        {
            if (!value.Contains("_") && value.Any(char.IsUpper))
            {
                return value;
            }

            var cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;

            var prettyString = value.Replace("_", " ");
            prettyString = cultureInfo.TextInfo.ToTitleCase(prettyString);
            prettyString = prettyString.Replace(" ", string.Empty);
            return prettyString;
        }

        public static string ColumnPrettify(this string value)
        {
            if (!value.Contains("_") && value.Any(char.IsUpper))
            {
                return value;
            }

            var cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;

            var prettyString = value.Replace("_", " ");
            prettyString = cultureInfo.TextInfo.ToTitleCase(prettyString);
            prettyString = prettyString.Replace(" ", string.Empty);
            return prettyString;
        }
    }
}
