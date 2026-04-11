using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace TPApp.Services
{
    public class ExcelExportService : IExcelExportService
    {
        public ExcelExportService()
        {
            // EPPlus 5+ requires license context to be set
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public FileContentResult Export<T>(IEnumerable<T> data, string fileName)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Sheet1");

            // ── Headers ──
            for (int col = 0; col < properties.Length; col++)
            {
                var prop = properties[col];
                var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
                var header = displayAttr?.Name ?? prop.Name;

                var cell = ws.Cells[1, col + 1];
                cell.Value = header;

                // Header styling
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(68, 114, 196)); // Blue header
                cell.Style.Font.Color.SetColor(Color.White);
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            // ── Data rows ──
            int row = 2;
            foreach (var item in data)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    var value = properties[col].GetValue(item);
                    var cell = ws.Cells[row, col + 1];

                    // Format DateTime nicely
                    if (value is DateTime dt)
                        cell.Value = dt.ToString("dd/MM/yyyy HH:mm");
                    else
                        cell.Value = value;

                    // Stripe even rows
                    if (row % 2 == 0)
                    {
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(242, 242, 242));
                    }

                    // Render URLs as clickable hyperlinks
                    if (value is string strVal
                        && (strVal.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                            || strVal.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
                    {
                        if (Uri.TryCreate(strVal, UriKind.Absolute, out var uri))
                        {
                            cell.Hyperlink = uri;
                            cell.Style.Font.Color.SetColor(Color.FromArgb(0, 70, 127));
                            cell.Style.Font.UnderLine = true;
                        }
                    }
                }
                row++;
            }

            // ── Auto-fit columns ──
            ws.Cells[ws.Dimension.Address].AutoFitColumns();

            // ── Freeze header row ──
            ws.View.FreezePanes(2, 1);

            var bytes = package.GetAsByteArray();
            return new FileContentResult(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = fileName.EndsWith(".xlsx") ? fileName : fileName + ".xlsx"
            };
        }
    }
}
