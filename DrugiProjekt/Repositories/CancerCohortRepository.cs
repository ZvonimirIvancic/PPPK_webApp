using DrugiProjekt.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DrugiProjekt.Repositories
{
    public class CancerCohortRepository : ICancerCohortRepository
    {
        private readonly IMongoCollection<CancerCohort> _collection;
        private readonly ILogger<CancerCohortRepository> _logger;

        public CancerCohortRepository(IMongoDatabase database, ILogger<CancerCohortRepository> logger)
        {
            _collection = database.GetCollection<CancerCohort>("cancer_cohorts");
            _logger = logger;
        }

        public async Task<CancerCohort?> GetByIdAsync(string id)
        {
            try
            {
                return await _collection.Find(c => c.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving cancer cohort by ID: {id}");
                return null;
            }
        }

        public async Task<IEnumerable<CancerCohort>> GetAllAsync()
        {
            try
            {
                return await _collection.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all cancer cohorts");
                return Enumerable.Empty<CancerCohort>();
            }
        }

        public async Task<CancerCohort?> GetByNameAsync(string cohortName)
        {
            try
            {
                return await _collection.Find(c => c.CohortName == cohortName).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving cancer cohort by name: {cohortName}");
                return null;
            }
        }

        public async Task<IEnumerable<CancerCohort>> GetByCancerTypeAsync(string cancerType)
        {
            try
            {
                var filter = Builders<CancerCohort>.Filter.Eq(c => c.CancerType, cancerType);
                return await _collection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving cohorts by cancer type: {cancerType}");
                return Enumerable.Empty<CancerCohort>();
            }
        }

        public async Task<IEnumerable<CancerCohort>> GetByStatusAsync(CohortStatus status)
        {
            try
            {
                var filter = Builders<CancerCohort>.Filter.Eq(c => c.Status, status);
                return await _collection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving cohorts by status: {status}");
                return Enumerable.Empty<CancerCohort>();
            }
        }

        public async Task<CancerCohort> InsertAsync(CancerCohort cohort)
        {
            try
            {
                await _collection.InsertOneAsync(cohort);
                return cohort;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inserting cancer cohort: {cohort.CohortName}");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(string id, CancerCohort cohort)
        {
            try
            {
                cohort.UpdatedAt = DateTime.UtcNow;
                var result = await _collection.ReplaceOneAsync(c => c.Id == id, cohort);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating cancer cohort: {id}");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var result = await _collection.DeleteOneAsync(c => c.Id == id);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting cancer cohort: {id}");
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
                _logger.LogError(ex, "Error counting cancer cohorts");
                return 0;
            }
        }

        public async Task<long> CountByStatusAsync(CohortStatus status)
        {
            try
            {
                var filter = Builders<CancerCohort>.Filter.Eq(c => c.Status, status);
                return await _collection.CountDocumentsAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error counting cohorts by status: {status}");
                return 0;
            }
        }

        public async Task<IEnumerable<CancerCohort>> GetProcessedCohortsAsync()
        {
            try
            {
                var filter = Builders<CancerCohort>.Filter.Eq(c => c.Status, CohortStatus.Completed);
                return await _collection.Find(filter)
                    .SortByDescending(c => c.ProcessingDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving processed cohorts");
                return Enumerable.Empty<CancerCohort>();
            }
        }

        public async Task<IEnumerable<CancerCohort>> GetFailedCohortsAsync()
        {
            try
            {
                var filter = Builders<CancerCohort>.Filter.Eq(c => c.Status, CohortStatus.Failed);
                return await _collection.Find(filter)
                    .SortByDescending(c => c.UpdatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving failed cohorts");
                return Enumerable.Empty<CancerCohort>();
            }
        }

        public async Task<Dictionary<CohortStatus, int>> GetStatusStatisticsAsync()
        {
            try
            {
                var pipeline = new[]
                {
                    new BsonDocument("$group", new BsonDocument
                    {
                        ["_id"] = "$status",
                        ["count"] = new BsonDocument("$sum", 1)
                    })
                };

                var results = await _collection.AggregateAsync<BsonDocument>(pipeline);
                var stats = new Dictionary<CohortStatus, int>();

                await results.ForEachAsync(doc =>
                {
                    if (Enum.TryParse<CohortStatus>(doc["_id"].AsString, out var status))
                    {
                        stats[status] = doc["count"].AsInt32;
                    }
                });

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting status statistics");
                return new Dictionary<CohortStatus, int>();
            }
        }

        public async Task<Dictionary<string, int>> GetCancerTypeStatisticsAsync()
        {
            try
            {
                var pipeline = new[]
                {
                    new BsonDocument("$group", new BsonDocument
                    {
                        ["_id"] = "$cancer_type",
                        ["count"] = new BsonDocument("$sum", 1)
                    }),
                    new BsonDocument("$sort", new BsonDocument("count", -1))
                };

                var results = await _collection.AggregateAsync<BsonDocument>(pipeline);
                var stats = new Dictionary<string, int>();

                await results.ForEachAsync(doc =>
                {
                    var cancerType = doc["_id"].AsString;
                    var count = doc["count"].AsInt32;
                    stats[cancerType] = count;
                });

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cancer type statistics");
                return new Dictionary<string, int>();
            }
        }

        public async Task<IEnumerable<CancerCohort>> GetCohortsWithPatientCountAsync(int minPatients = 1)
        {
            try
            {
                var filter = Builders<CancerCohort>.Filter.Gte(c => c.ProcessedPatients, minPatients);
                return await _collection.Find(filter)
                    .SortByDescending(c => c.ProcessedPatients)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving cohorts with minimum {minPatients} patients");
                return Enumerable.Empty<CancerCohort>();
            }
        }

        public async Task<long> GetTotalPatientsAcrossCohortsAsync()
        {
            try
            {
                var pipeline = new[]
                {
                    new BsonDocument("$group", new BsonDocument
                    {
                        ["_id"] = BsonNull.Value,
                        ["total_patients"] = new BsonDocument("$sum", "$processed_patients")
                    })
                };

                var results = await _collection.AggregateAsync<BsonDocument>(pipeline);
                var result = await results.FirstOrDefaultAsync();

                return result?["total_patients"]?.AsInt64 ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total patients across cohorts");
                return 0;
            }
        }

        public async Task<bool> CohortExistsAsync(string cohortName)
        {
            try
            {
                var count = await _collection.CountDocumentsAsync(c => c.CohortName == cohortName);
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if cohort exists: {cohortName}");
                return false;
            }
        }
    }
}
