using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoLite
{
    public static class Extensions
    {
        /// <summary>
        /// Appends a formatted string line to this string builder
        /// </summary>
        /// <param name="sb">The string builder to utilize</param>
        /// <param name="format">The formatted string to add to this string builder</param>
        /// <param name="args">the argument values referenced in the format string</param>
        public static void AppendLineFormatted(this StringBuilder sb, String format, params Object[] args)
        {
            sb.AppendLine(String.Format(format, args));
        }

        /// <summary>
        /// Applies the action to all items in the collection, but applies a different action to the last item in the enumerable
        /// </summary>
        /// <typeparam name="T">The type of items in this enumerable</typeparam>
        /// <param name="items">The items contained in this enumerable</param>
        /// <param name="allButLastAction">The action to apply to all items in the enumerable but the last one</param>
        /// <param name="lastItemAction">The action to apply to the last item in the enumerable</param>
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> allButLastAction, Action<T> lastItemAction)
        {
            var itemsCount = items.Count();
            for (int i = 0; i < itemsCount; i++)
            {
                var ithItem = items.ElementAt(i);

                if(i == itemsCount -1)
                { lastItemAction(ithItem); }
                else { allButLastAction(ithItem); }
            }
        }

        /// <summary>
        /// Creates a parameter with the specified name and value, and adds it to this command
        /// </summary>
        /// <param name="cmd">The command to add the parameter to</param>
        /// <param name="name">The name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        private static void AddParameter(this System.Data.IDbCommand cmd, String name, Object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        /// <summary>
        /// Gets the string value of the item at the named colum in this data row, handling the case if the value is null
        /// </summary>
        /// <param name="dr">The data row containing the value</param>
        /// <param name="columnName">The named column to extract the value from</param>
        /// <param name="nullValue">The optional replacement to use in the event the extracted value is null</param>
        /// <returns>The extracted string value</returns>
        public static String StringValue(this System.Data.DataRow dr, String columnName, String nullValue = "")
        {
            var columnValue = dr[columnName];
            if (columnValue == null || columnValue == DBNull.Value)
            { return nullValue; }
            else { return columnValue.ToString().Trim(); }
        }

        /// <summary>
        /// Gets the date value of the items at the named column in this data row, handling the case if the value is null
        /// </summary>
        /// <param name="dr">The data row containing the value</param>
        /// <param name="columnName">the named column to extract the value from</param>
        /// <returns>The extracted date value</returns>
        public static DateTime DateValue(this System.Data.DataRow dr, String columnName)
        {
            var columnValue = dr[columnName];

            if(columnValue == null || columnValue == DBNull.Value)
            { return DateTime.MinValue; }
            else { return DateTime.Parse(columnValue.ToString()); }
        }

        /// <summary>
        /// Gets the integer value of the item at the named column in this data row, handling the case if the value is null
        /// </summary>
        /// <param name="dr">The data row containing the value</param>
        /// <param name="columnName">The named column to extract the value from</param>
        /// <returns>The extracted integer value</returns>
        public static int IntValue(this System.Data.DataRow dr, String columnName)
        {
            var columnValue = dr[columnName];

            if (columnValue == null || columnValue == DBNull.Value)
            { return 0; }
            else { return Int32.Parse(columnValue.ToString()); }
        }

        /// <summary>
        /// Gets the boolean value of the item at the named column in this data row, handling the case if the value is null
        /// </summary>
        /// <param name="dr">The data row containing the value</param>
        /// <param name="columnName">The named column to extract the value from</param>
        /// <param name="nullValue">the value to use in the event null is extracted</param>
        /// <returns>The extracted boolean value</returns>
        public static Boolean BooleanValue(this System.Data.DataRow dr, String columnName, Boolean nullValue = false)
        {
            var columnValue = dr[columnName];
            if(columnValue == null || columnValue == DBNull.Value)
            { return nullValue; }
            else
            {
                var colStringVal = columnValue.ToString().ToUpperInvariant();
                if (colStringVal == "1" || colStringVal == "Y" || colStringVal == "YES" || colStringVal == "T" || colStringVal == "TRUE")
                { return true; }
                else
                { return false; }
            }
        }
    }
}
