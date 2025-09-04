using DrugiProjekt.Models;
using MongoDB.Driver;

namespace DrugiProjekt.Repositories
{
    public class PatientGeneExpressionRepository : IPatientGeneExpressionRepository
    {
        private readonly IMongoCollection<PatientGeneExpression> _collection;
        private readonly ILogger<PatientGeneExpressionRepository> _logger;

        public PatientGeneExpressionRepository(IMongoDatabase database, ILogger<PatientGeneExpressionRepository> logger)
        {
            _collection = database.GetCollection<PatientGeneExpression>("patient_gene_expressions");
            _logger = logger;
        }

        public async Task<PatientGeneExpression?> GetByIdAsync(string id)
        {
            try
            {
                return await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving patient gene expression by ID: {id}");
                return null;
            }
        }

        public async Task<IEnumerable<PatientGeneExpression>> GetAllAsync()
        {
            try
            {
                return await _collection.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all patient gene expressions");
                return Enumerable.Empty<PatientGeneExpression>();
            }
        }

        public async Task<IEnumerable<PatientGeneExpression>> GetByCohortAsync(string cohort)
        {
            try
            {
                var filter = Builders<PatientGeneExpression>.Filter.Eq(p => p.CancerCohort, cohort);
                return await _collection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving patient gene expressions for cohort: {cohort}");
                return Enumerable.Empty<PatientGeneExpression>();
            }
        }

        public async Task<IEnumerable<PatientGeneExpression>> GetByPatientIdAsync(string patientId)
        {
            try
            {
                var filter = Builders<PatientGeneExpression>.Filter.Eq(p => p.PatientId, patientId);
                return await _collection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving gene expressions for patient: {patientId}");
                return Enumerable.Empty<PatientGeneExpression>();
            }
        }

        public async Task<PatientGeneExpression> InsertAsync(PatientGeneExpression patient)
        {
            try
            {
                await _collection.InsertOneAsync(patient);
                return patient;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inserting patient gene expression for patient: {patient.PatientId}");
                throw;
            }
        }

        public async Task<IEnumerable<PatientGeneExpression>> InsertManyAsync(IEnumerable<PatientGeneExpression> patients)
        {
            try
            {
                var patientsList = patients.ToList();
                if (patientsList.Any())
                {
                    await _collection.InsertManyAsync(patientsList);
                }
                return patientsList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting multiple patient gene expressions");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(string id, PatientGeneExpression patient)
        {
            try
            {
                patient.UpdatedAt = DateTime.UtcNow;
                var result = await _collection.ReplaceOneAsync(p => p.Id == id, patient);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating patient gene expression: {id}");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var result = await _collection.DeleteOneAsync(p => p.Id == id);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting patient gene expression: {id}");
                return false;
            }
        }

        public async Task<long> CountAsync()
        {
            try
            {
                return await _collection.CountDocumentsAsync(_ => true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting patient gene expressions");
                return 0;
            }
        }

        public async Task<long> CountByCohortAsync(string cohort)
        {
            try
            {
                var filter = Builders<PatientGeneExpression>.Filter.Eq(p => p.CancerCohort, cohort);
                return await _collection.CountDocumentsAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error counting patient gene expressions for cohort: {cohort}");
                return 0;
            }
        }

        public async Task<IEnumerable<PatientGeneExpression>> GetByGeneExpressionRangeAsync(string geneName, double minValue, double maxValue)
        {
            try
            {
                var filter = Builders<PatientGeneExpression>.Filter.And(
                    Builders<PatientGeneExpression>.Filter.Exists($"gene_expressions.{geneName}"),
                    Builders<PatientGeneExpression>.Filter.Gte($"gene_expressions.{geneName}", minValue),
                    Builders<PatientGeneExpression>.Filter.Lte($"gene_expressions.{geneName}", maxValue)
                );

                return await _collection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving patients by gene expression range for gene: {geneName}");
                return Enumerable.Empty<PatientGeneExpression>();
            }
        }

        public async Task<IEnumerable<PatientGeneExpression>> GetcGAS_STING_DataAsync(string? cohort = null)
        {
            try
            {
                var filterBuilder = Builders<PatientGeneExpression>.Filter;
                var filter = filterBuilder.Ne(p => p.cGAS_STING_Genes, null);

                if (!string.IsNullOrEmpty(cohort))
                {
                    filter = filterBuilder.And(filter, filterBuilder.Eq(p => p.CancerCohort, cohort));
                }

                return await _collection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving cGAS-STING data for cohort: {cohort}");
                return Enumerable.Empty<PatientGeneExpression>();
            }
        }

        public async Task<Dictionary<string, double>> GetGeneExpressionStatisticsAsync(string geneName, string? cohort = null)
        {
            try
            {
                var filterBuilder = Builders<PatientGeneExpression>.Filter;
                var filter = filterBuilder.Exists($"gene_expressions.{geneName}");

                if (!string.IsNullOrEmpty(cohort))
                {
                    filter = filterBuilder.And(filter, filterBuilder.Eq(p => p.CancerCohort, cohort));
                }

                var patients = await _collection.Find(filter).ToListAsync();
                var expressionValues = patients
                    .Where(p => p.GeneExpressions.ContainsKey(geneName))
                    .Select(p => p.GeneExpressions[geneName])
                    .Where(val => !double.IsNaN(val) && !double.IsInfinity(val))
                    .ToList();

                if (!expressionValues.Any())
                {
                    return new Dictionary<string, double>();
                }

                var sortedValues = expressionValues.OrderBy(x => x).ToList();

                return new Dictionary<string, double>
                {
                    ["Count"] = expressionValues.Count,
                    ["Min"] = sortedValues.First(),
                    ["Max"] = sortedValues.Last(),
                    ["Mean"] = expressionValues.Average(),
                    ["Median"] = GetMedian(sortedValues),
                    ["Q1"] = GetPercentile(sortedValues, 0.25),
                    ["Q3"] = GetPercentile(sortedValues, 0.75),
                    ["StdDev"] = GetStandardDeviation(expressionValues)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calculating gene expression statistics for gene: {geneName}");
                return new Dictionary<string, double>();
            }
        }

        public async Task<IEnumerable<string>> GetAvailableCohortsAsync()
        {
            try
            {
                var cohorts = await _collection.Distinct<string>("cancer_cohort", _ => true).ToListAsync();
                return cohorts.OrderBy(c => c);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available cohorts");
                return Enumerable.Empty<string>();
            }
        }

        public async Task<IEnumerable<string>> GetPatientIdsByCohortAsync(string cohort)
        {
            try
            {
                var filter = Builders<PatientGeneExpression>.Filter.Eq(p => p.CancerCohort, cohort);
                var patientIds = await _collection.Distinct<string>("patient_id", filter).ToListAsync();
                return patientIds.OrderBy(p => p);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving patient IDs for cohort: {cohort}");
                return Enumerable.Empty<string>();
            }
        }

        private static double GetMedian(List<double> sortedValues)
        {
            int count = sortedValues.Count;
            if (count % 2 == 0)
            {
                return (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2.0;
            }
            return sortedValues[count / 2];
        }

        private static double GetPercentile(List<double> sortedValues, double percentile)
        {
            int count = sortedValues.Count;
            double index = percentile * (count - 1);
            int lowerIndex = (int)Math.Floor(index);
            int upperIndex = (int)Math.Ceiling(index);

            if (lowerIndex == upperIndex)
            {
                return sortedValues[lowerIndex];
            }

            double weight = index - lowerIndex;
            return sortedValues[lowerIndex] * (1 - weight) + sortedValues[upperIndex] * weight;
        }

        private static double GetStandardDeviation(List<double> values)
        {
            double mean = values.Average();
            double sumOfSquares = values.Sum(x => Math.Pow(x - mean, 2));
            return Math.Sqrt(sumOfSquares / values.Count);
        }
    }
}
