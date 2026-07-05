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
    public class CountryService : EtlServiceBase<CountryCsv>, ICountryService
    {
        private readonly DataCleaner _dataCleaner;
        private readonly DuplicateValidator _duplicateValidator;
        private HashSet<string>? _existingCountries;

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

        public override async Task<EtlReport> ExecuteAsync()
        {
            // Cargar países existentes en memoria para evitar N+1 queries
            var dbCountries = await _context.Countries.AsNoTracking().ToListAsync();
            _existingCountries = new HashSet<string>();
            foreach(var c in dbCountries)
            {
                _existingCountries.Add(c.CountryName.ToLower());
            }

            return await base.ExecuteAsync();
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
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Required Fields", "Nombre de pais vacio"));
                return;
            }

            var cleanNameLower = cleanName.ToLower();

            if (_duplicateValidator.IsDuplicate(cleanNameLower, uniqueSet))
            {
                result.DuplicatedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Duplicate", "Pais duplicado en archivo"));
                return;
            }

            if (_existingCountries != null && _existingCountries.Contains(cleanNameLower))
            {
                result.DuplicatedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Duplicate", "Pais ya existe en BD"));
                return;
            }

            await procedures.sp_InsertCountryAsync(cleanName);
            if (_existingCountries != null) _existingCountries.Add(cleanNameLower);
            result.InsertedRecords++;
        }
    }
}
