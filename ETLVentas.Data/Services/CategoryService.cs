using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
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
        private HashSet<string>? _existingCategories;

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

        public override async Task<EtlReport> ExecuteAsync()
        {
            // Cargar categorías existentes en memoria para evitar N+1 queries
            var dbCategories = await _context.Categories.AsNoTracking().ToListAsync();
            _existingCategories = new HashSet<string>();
            foreach(var c in dbCategories)
            {
                _existingCategories.Add(c.CategoryName.ToLower());
            }

            return await base.ExecuteAsync();
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

            var cleanNameLower = cleanName.ToLower();

            if (_duplicateValidator.IsDuplicate(cleanNameLower, uniqueSet))
            {
                result.DuplicatedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Duplicate", "Categoria duplicada en archivo"));
                return;
            }

            if (_existingCategories != null && _existingCategories.Contains(cleanNameLower))
            {
                result.DuplicatedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Duplicate", "Categoria ya existe en BD"));
                return;
            }

            await procedures.sp_InsertCategoryAsync(cleanName);
            if (_existingCategories != null) _existingCategories.Add(cleanNameLower);
            result.InsertedRecords++;
        }
    }
}
