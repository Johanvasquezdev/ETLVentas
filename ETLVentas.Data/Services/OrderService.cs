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
    public class OrderService : EtlServiceBase<OrderCsv>, IOrderService
    {
        private readonly DataCleaner _dataCleaner;
        private readonly DuplicateValidator _duplicateValidator;
        private readonly ReferentialIntegrityValidator _refValidator;
        private readonly TypeValidator _typeValidator;

        protected override string EntityName => "Orders";

        public OrderService(
            ICSVReaderService csvReader,
            AnalisisDeVentasContext context,
            ILogger<OrderService> logger,
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
            OrderCsv record, 
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

            if (_duplicateValidator.IsDuplicate(orderId.ToString(), uniqueSet))
            {
                result.DuplicatedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Duplicate", "Orden duplicada"));
                return;
            }

            if (!_typeValidator.IsValidDateTime(record.OrderDate, out DateTime orderDateDT))
            {
                result.RejectedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Type Validation", "OrderDate invalido"));
                return;
            }
            DateOnly orderDate = new DateOnly(orderDateDT.Year, orderDateDT.Month, orderDateDT.Day);

            if (string.IsNullOrWhiteSpace(record.CustomerID) || !int.TryParse(record.CustomerID, out int customerId))
            {
                result.RejectedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Foreign Key", "CustomerID invalido"));
                return;
            }

            var cleanStatus = _dataCleaner.CleanString(record.Status);
            if (string.IsNullOrEmpty(cleanStatus))
            {
                result.RejectedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Required Fields", "Status vacio"));
                return;
            }
            
            await procedures.sp_InsertOrderAsync(orderId, customerId, cleanStatus, orderDate);
            result.InsertedRecords++;
        }
    }
}
