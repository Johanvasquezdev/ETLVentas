using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ETLVentas.Data.Context;
using ETLVentas.Data.Interfaces;
using ETLVentas.Data.Result;
using ETLVentas.Data.Validators;
using ETLVentas.Data.Helpers;

namespace ETLVentas.Data.Services
{
    public abstract class EtlServiceBase<TCsv>
    {
        protected readonly ICSVReaderService _csvReader;
        protected readonly AnalisisDeVentasContext _context;
        protected readonly CsvValidator _csvValidator;
        protected readonly ILogger _logger;

        protected abstract string EntityName { get; }

        protected EtlServiceBase(
            ICSVReaderService csvReader,
            AnalisisDeVentasContext context,
            CsvValidator csvValidator,
            ILogger logger = null)
        {
            _csvReader = csvReader;
            _context = context;
            _csvValidator = csvValidator;
            _logger = logger;
        }

        public virtual async Task<EntityProcessResult> ProcessAsync(string filePath)
        {
            var sw = Stopwatch.StartNew();
            var startTime = DateTime.Now;
            var result = new EntityProcessResult { EntityName = EntityName, StartTime = startTime };

            _logger?.LogInformation($"Starting ETL for {EntityName}");

            if (!_csvValidator.ValidateFile(filePath, out string error))
            {
                _logger?.LogError(error);
                result.Errors++;
                result.ErrorMessages.Add(ValidationHelper.FormatError("N/A", "File Error", error));
                result.EndTime = DateTime.Now;
                result.Duration = sw.Elapsed;
                return result;
            }

            var readResult = await _csvReader.ReadCsvAsync<TCsv>(filePath);
            if (!readResult.Success || readResult.Data == null)
            {
                _logger?.LogError(readResult.ErrorMessage);
                result.Errors++;
                result.ErrorMessages.Add(ValidationHelper.FormatError("N/A", "Read Error", readResult.ErrorMessage));
                result.EndTime = DateTime.Now;
                result.Duration = sw.Elapsed;
                return result;
            }

            var records = readResult.Data;
            result.TotalRecords = records.Count;

            var uniqueSet = new HashSet<string>();
            var procedures = _context.GetProcedures();

            int row = 1;
            foreach (var record in records)
            {
                row++;
                try
                {
                    await ProcessRecordAsync(record, row, result, uniqueSet, procedures);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"Error processing record {row}");
                    result.Errors++;
                    result.ErrorMessages.Add(ValidationHelper.FormatError(row.ToString(), "System Error", ex.Message));
                }
            }

            sw.Stop();
            result.EndTime = DateTime.Now;
            result.Duration = sw.Elapsed;
            _logger?.LogInformation($"Finished ETL for {EntityName}");

            return result;
        }

        protected abstract Task ProcessRecordAsync(
            TCsv record, 
            int rowNumber, 
            EntityProcessResult result, 
            HashSet<string> uniqueSet, 
            IAnalisisDeVentasContextProcedures procedures);
    }
}

