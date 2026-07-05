using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ETLVentas.Data.Context;
using ETLVentas.Data.Interfaces;
using ETLVentas.Data.Models.Csv;
using ETLVentas.Data.Result;
using ETLVentas.Data.Validators;
using ETLVentas.Data.Helpers;

namespace ETLVentas.Data.Services
{
    public class CategoryService : EtlServiceBase<CategoryCsv>, ICategoryService
    {
        private readonly DataCleaner _dataCleaner;
        private readonly DuplicateValidator _duplicateValidator;

        protected override string EntityName => "Categories";

        public CategoryService(
            ICSVReaderService csvReader,
            AnalisisDeVentasContext context,
            ILogger<CategoryService> logger,
            CsvValidator csvValidator,
            DataCleaner dataCleaner,
            DuplicateValidator duplicateValidator)
            : base(csvReader, context, csvValidator, logger)
        {
            _dataCleaner = dataCleaner;
            _duplicateValidator = duplicateValidator;
        }

        protected override async Task ProcessRecordAsync(
            CategoryCsv record, 
            int rowNumber, 
            EntityProcessResult result, 
            HashSet<string> uniqueSet, 
            IAnalisisDeVentasContextProcedures procedures)
        {
            var cleanName = _dataCleaner.CleanString(record.CategoryName);
            if (string.IsNullOrEmpty(cleanName))
            {
                result.RejectedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Required Fields", "Nombre de categoria vacio"));
                return;
            }

            if (_duplicateValidator.IsDuplicate(cleanName.ToLower(), uniqueSet))
            {
                result.DuplicatedRecords++;
                return;
            }

            var exists = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(_context.Categories, c => c.CategoryName == cleanName);
            if (exists)
            {
                result.DuplicatedRecords++;
                return;
            }

            await procedures.sp_InsertCategoryAsync(cleanName);
            result.InsertedRecords++;
        }
    }
}
