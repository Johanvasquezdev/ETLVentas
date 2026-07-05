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
    public class CityService : EtlServiceBase<CityCsv>, ICityService
    {
        private readonly DataCleaner _dataCleaner;
        private readonly DuplicateValidator _duplicateValidator;
        private readonly ReferentialIntegrityValidator _refValidator;

        protected override string EntityName => "Cities";

        public CityService(
            ICSVReaderService csvReader,
            AnalisisDeVentasContext context,
            CsvValidator csvValidator,
            DataCleaner dataCleaner,
            DuplicateValidator duplicateValidator,
            ReferentialIntegrityValidator refValidator)
            : base(csvReader, context, csvValidator, null)
        {
            _dataCleaner = dataCleaner;
            _duplicateValidator = duplicateValidator;
            _refValidator = refValidator;
        }

        protected override async Task ProcessRecordAsync(
            CityCsv record, 
            int rowNumber, 
            EntityProcessResult result, 
            HashSet<string> uniqueSet, 
            IAnalisisDeVentasContextProcedures procedures)
        {
            var cleanCity = _dataCleaner.CleanString(record.CityName);
            var cleanCountry = _dataCleaner.CleanString(record.CountryName);

            if (string.IsNullOrEmpty(cleanCity) || string.IsNullOrEmpty(cleanCountry))
            {
                result.RejectedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Required Fields", "Ciudad o Pais vacio"));
                return;
            }

            var key = $"{cleanCity.ToLower()}_{cleanCountry.ToLower()}";
            if (_duplicateValidator.IsDuplicate(key, uniqueSet))
            {
                result.DuplicatedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Duplicate", "Ciudad duplicada en el archivo"));
                return;
            }

            var countryId = await _refValidator.GetCountryIdAsync(cleanCountry);
            if (!countryId.HasValue)
            {
                result.RejectedRecords++;
                result.ErrorMessages.Add(ValidationHelper.FormatError(rowNumber.ToString(), "Foreign Key", $"Pais no encontrado: {cleanCountry}"));
                return;
            }

            var cityId = await _refValidator.GetCityIdAsync(cleanCity, countryId.Value);
            if (cityId.HasValue)
            {
                result.DuplicatedRecords++;
                return;
            }

            await procedures.sp_InsertCityAsync(cleanCity, countryId.Value);
            result.InsertedRecords++;
        }
    }
}
