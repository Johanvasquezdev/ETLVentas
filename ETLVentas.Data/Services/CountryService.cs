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
    public class CountryService : EtlServiceBase<CountryCsv>, ICountryService
    {
        private readonly DataCleaner _dataCleaner;
        private readonly DuplicateValidator _duplicateValidator;

        protected override string EntityName => "Country";

        public CountryService(
            ICSVReaderService csvReader,
            AnalisisDeVentasContext context,
            ILogger<CountryService> logger,
            CsvValidator csvValidator,
            DataCleaner dataCleaner,
            DuplicateValidator duplicateValidator)
            : base(csvReader, context, csvValidator, logger)
        {
            _dataCleaner = dataCleaner;
            _duplicateValidator = duplicateValidator;
        }

        protected override async Task ProcessRecordAsync(
            CountryCsv record, 
            int rowNumber, 
            EntityProcessResult result, 
            HashSet<string> uniqueSet, 
            IAnalisisDeVentasContextProcedures procedures)
        {
            var cleanName = _dataCleaner.CleanString(record.CountryName);
            if (string.IsNullOrEmpty(cleanName))
            {
                result.RejectedRecords++;
                return;
            }

            if (_duplicateValidator.IsDuplicate(cleanName.ToLower(), uniqueSet))
            {
                result.DuplicatedRecords++;
                return;
            }

            // Let the Database or Cache handle the existence check later, 
            // but for Clientes we only insert if not exists. 
            // Actually, for Country we also check DB.
            // Wait, we can use EF Core AnyAsync here but it's better to avoid N+1 if possible, 
            // but wait, since there are only 7 countries it's fast. Let's keep it simple.
            var exists = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(_context.Countries, c => c.CountryName == cleanName);
            if (exists)
            {
                result.DuplicatedRecords++;
                return;
            }

            await procedures.sp_InsertCountryAsync(cleanName);
            result.InsertedRecords++;
        }
    }
}
