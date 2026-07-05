using System;
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
    public class OrderDetailService : EtlServiceBase<OrderDetailCsv>, IOrderDetailService
    {
        private readonly DataCleaner _dataCleaner;
        private readonly DuplicateValidator _duplicateValidator;
        private readonly ReferentialIntegrityValidator _refValidator;
        private readonly TypeValidator _typeValidator;

        protected override string EntityName => "OrderDetails";

        public OrderDetailService(
            ICSVReaderService csvReader,
            AnalisisDeVentasContext context,
            ILogger<OrderDetailService> logger,
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
            OrderDetailCsv record, 
            int rowNumber, 
            EntityProcessResult result, 
            HashSet<string> uniqueSet, 
            IAnalisisDeVentasContextProcedures procedures)
        {
            if (string.IsNullOrWhiteSpace(record.OrderID) || !int.TryParse(record.OrderID, out int orderId))
            {
                result.RejectedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Required Fields", "OrderID invalido"));
                return;
            }

            if (string.IsNullOrWhiteSpace(record.ProductID) || !int.TryParse(record.ProductID, out int productId))
            {
                result.RejectedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Required Fields", "ProductID invalido"));
                return;
            }

            if (!_typeValidator.IsValidInt(record.Quantity, out int quantity))
            {
                result.RejectedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Type Validation", "Quantity invalido"));
                return;
            }

            if (!_typeValidator.IsValidDecimal(record.TotalPrice, out decimal unitPrice))
            {
                result.RejectedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Type Validation", "UnitPrice invalido"));
                return;
            }

            var key = $"{orderId}_{productId}";
            if (_duplicateValidator.IsDuplicate(key, uniqueSet))
            {
                result.DuplicatedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Duplicate", "Detalle de orden duplicado"));
                return;
            }

            await procedures.sp_InsertOrderDetailAsync(orderId, productId, quantity, unitPrice);
            result.InsertedRecords++;
        }
    }
}
