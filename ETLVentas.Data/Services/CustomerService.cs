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
    public class CustomerService : EtlServiceBase<CustomerCsv>, ICustomerService
    {
        private readonly DataCleaner _dataCleaner;
        private readonly DuplicateValidator _duplicateValidator;
        private readonly ReferentialIntegrityValidator _refValidator;

        protected override string EntityName => "Customers";

        public CustomerService(
            ICSVReaderService csvReader,
            AnalisisDeVentasContext context,
            ILogger<CustomerService> logger,
            CsvValidator csvValidator,
            DataCleaner dataCleaner,
            DuplicateValidator duplicateValidator,
            ReferentialIntegrityValidator refValidator)
            : base(csvReader, context, csvValidator, logger)
        {
            _dataCleaner = dataCleaner;
            _duplicateValidator = duplicateValidator;
            _refValidator = refValidator;
        }

        protected override async Task ProcessRecordAsync(
            CustomerCsv record, 
            int rowNumber, 
            EntityProcessResult result, 
            HashSet<string> uniqueSet, 
            IAnalisisDeVentasContextProcedures procedures)
        {
            if (string.IsNullOrWhiteSpace(record.CustomerID) || !int.TryParse(record.CustomerID, out int customerId))
            {
                result.RejectedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Required Fields", "CustomerID invalido"));
                return;
            }

            var cleanFirstName = _dataCleaner.CleanString(record.FirstName);
            var cleanLastName = _dataCleaner.CleanString(record.LastName);
            if (string.IsNullOrEmpty(cleanFirstName) || string.IsNullOrEmpty(cleanLastName))
            {
                result.RejectedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Required Fields", "Nombre vacio"));
                return;
            }

            var cleanEmail = _dataCleaner.CleanString(record.Email);
            if (string.IsNullOrEmpty(cleanEmail))
            {
                result.RejectedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Required Fields", "Email vacio"));
                return;
            }

            if (_duplicateValidator.IsDuplicate(customerId.ToString(), uniqueSet))
            {
                result.DuplicatedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Duplicate", "Cliente duplicado"));
                return;
            }

            var countryId = await _refValidator.GetCountryIdAsync(_dataCleaner.CleanString(record.Country));
            if (!countryId.HasValue)
            {
                result.RejectedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Foreign Key", "Pais no encontrado"));
                return;
            }

            var cityId = await _refValidator.GetCityIdAsync(_dataCleaner.CleanString(record.City), countryId.Value);
            if (!cityId.HasValue)
            {
                result.RejectedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Foreign Key", "Ciudad no encontrada"));
                return;
            }

            await procedures.sp_InsertCustomerAsync(customerId, cleanFirstName, cleanLastName, cleanEmail, _dataCleaner.CleanString(record.Phone), cityId.Value);
            result.InsertedRecords++;
        }
    }
}
