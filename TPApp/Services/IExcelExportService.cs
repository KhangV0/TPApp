using Microsoft.AspNetCore.Mvc;

namespace TPApp.Services
{
    public interface IExcelExportService
    {
        /// <summary>
        /// Exports a list of items to an Excel file and returns FileContentResult.
        /// Column headers are read from [Display(Name)] attribute, or property name.
        /// </summary>
        FileContentResult Export<T>(IEnumerable<T> data, string fileName);
    }
}
