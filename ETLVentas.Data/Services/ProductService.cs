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
    public class ProductService : EtlServiceBase<ProductCsv>, IProductService
    {
        private readonly DataCleaner _dataCleaner;
        private readonly DuplicateValidator _duplicateValidator;
        private readonly ReferentialIntegrityValidator _refValidator;
        private readonly TypeValidator _typeValidator;

        protected override string EntityName => "Products";

        public ProductService(
            ICSVReaderService csvReader,
            AnalisisDeVentasContext context,
            ILogger<ProductService> logger,
            CsvValidator csvValidator,
            DataCleaner dataCleaner,
            DuplicateValidator duplicateValidator,
            ReferentialIntegrityValidator refValidator,
            TypeValidator typeValidator)
            : base(csvReader, context, csvValidator, logger)
        {
            _dataCleaner = dataCleaner;
            _duplicateValidator = duplicateValidator;
            _refValidator = refValidator;
            _typeValidator = typeValidator;
        }

        protected override async Task ProcessRecordAsync(
            ProductCsv record, 
            int rowNumber, 
            EntityProcessResult result, 
            HashSet<string> uniqueSet, 
            IAnalisisDeVentasContextProcedures procedures)
        {
            if (string.IsNullOrWhiteSpace(record.ProductID) || !int.TryParse(record.ProductID, out int productId))
            {
                result.RejectedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Required Fields", "ProductID invalido"));
                return;
            }

            var cleanName = _dataCleaner.CleanString(record.ProductName);
            if (string.IsNullOrEmpty(cleanName))
            {
                result.RejectedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Required Fields", "Nombre vacio"));
                return;
            }

            if (_duplicateValidator.IsDuplicate(productId.ToString(), uniqueSet))
            {
                result.DuplicatedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Duplicate", "Producto duplicado"));
                return;
            }

            var catId = await _refValidator.GetCategoryIdAsync(_dataCleaner.CleanString(record.Category));
            if (!catId.HasValue)
            {
                result.RejectedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Foreign Key", "Categoria no encontrada"));
                return;
            }

            if (!_typeValidator.IsValidDecimal(record.Price, out decimal price))
            {
                result.RejectedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Type Validation", "Precio invalido"));
                return;
            }
            if (!_typeValidator.IsValidInt(record.Stock, out int stock))
            {
                result.RejectedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Type Validation", "Stock invalido"));
                return;
            }

            await procedures.sp_InsertProductAsync(productId, cleanName, catId.Value, price, stock);
            result.InsertedRecords++;
        }
    }
}
