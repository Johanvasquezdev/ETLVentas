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
    public class CityService : EtlServiceBase<CityCsv>, ICityService
    {
        private readonly DataCleaner _dataCleaner;
        private readonly DuplicateValidator _duplicateValidator;
        private readonly ReferentialIntegrityValidator _refValidator;
        private HashSet<string>? _existingCities;

        protected override string EntityName => "Cities";

        public CityService(
            ICSVReaderService csvReader,
            AnalisisDeVentasContext context,
            ILogger<CityService> logger,
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

        public override async Task<EtlReport> ExecuteAsync()
        {
            // Cargar ciudades existentes en memoria para evitar N+1 queries
            var dbCities = await _context.Cities.AsNoTracking().ToListAsync();
            _existingCities = new HashSet<string>();
            foreach(var c in dbCities)
            {
                _existingCities.Add($"{c.CityName.ToLower()}_{c.CountryId}");
            }

            return await base.ExecuteAsync();
        }

        protected override async Task ProcessRecordAsync(
            CityCsv record, 
            int rowNumber, 
            EntityProcessResult result, 
            HashSet<string> uniqueSet, 
            IAnalisisDeVentasContextProcedures procedures)
        {
            var cleanCityName = _dataCleaner.CleanString(record.CityName);
            if (string.IsNullOrEmpty(cleanCityName))
            {
                result.RejectedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Required Fields", "Nombre de ciudad vacio"));
                return;
            }

            var countryId = await _refValidator.GetCountryIdAsync(_dataCleaner.CleanString(record.CountryName));
            if (!countryId.HasValue)
            {
                result.RejectedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Foreign Key", "Pais no encontrado"));
                return;
            }

            var uniqueKey = $"{cleanCityName.ToLower()}_{countryId.Value}";

            if (_duplicateValidator.IsDuplicate(uniqueKey, uniqueSet))
            {
                result.DuplicatedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Duplicate", "Ciudad duplicada en archivo"));
                return;
            }

            if (_existingCities != null && _existingCities.Contains(uniqueKey))
            {
                result.DuplicatedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Duplicate", "Ciudad ya existe en BD"));
                return;
            }

            await procedures.sp_InsertCityAsync(cleanCityName, countryId.Value);
            if (_existingCities != null) _existingCities.Add(uniqueKey);
            result.InsertedRecords++;
        }
    }
}
