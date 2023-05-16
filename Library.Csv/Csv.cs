using System.Globalization;
using System.Text;

namespace Library.Csv
{
    public static class Csv
    {
        /// <summary>
        /// Creates a .csv file out of the passed in data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="records"></param>
        /// <returns></returns>
        public static byte[] CreateCsv<T>(IEnumerable<T> records)
        {
            if (records.Any() == false) { return Array.Empty<byte>(); }

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                var builder = new StringBuilder();
                var listSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

                var properties = records.First().GetType().GetProperties();
                for (int i = 0; i < properties.Length; i++)
                {
                    // Writes the names of all properties as top row of document.
                    var value = properties[i].Name;
                    builder.Append($"{value}{listSeparator}");
                }

                foreach (var record in records)
                {
                    builder.AppendLine(); // Line break.

                    foreach (var property in record.GetType().GetProperties())
                    {
                        // Writes all values from a record.
                        var value = property?.GetValue(record)?.ToString()?.Replace(listSeparator, "") ?? "";
                        builder.Append($"{value}{listSeparator}");
                    }
                }

                writer.Write(builder);
                writer.Flush();

                return stream.ToArray();
            }
        }

        // Example of using OnPost to download .csv using the CreateCsv method.
        //public FileResult OnPostCsv(List<VwInrapporteringar> inrapporteringar)
        //{
        //    var file = Csv.CreateCsv(inrapporteringar);
        //    return File(file, "application/csv", $"Rapport-Inrapportering-{DateTime.Now.ToShortDateString()}.csv");
        //}
    }
}