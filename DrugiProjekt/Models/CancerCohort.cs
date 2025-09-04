using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DrugiProjekt.Models
{
    public class CancerCohort
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("cohort_name")]
        public string CohortName { get; set; } = string.Empty;

        [BsonElement("cancer_type")]
        public string CancerType { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("total_patients")]
        public int TotalPatients { get; set; }

        [BsonElement("processed_patients")]
        public int ProcessedPatients { get; set; }

        [BsonElement("xena_url")]
        public string XenaUrl { get; set; } = string.Empty;

        [BsonElement("illumina_file_url")]
        public string IlluminaFileUrl { get; set; } = string.Empty;

        [BsonElement("download_date")]
        public DateTime? DownloadDate { get; set; }

        [BsonElement("processing_date")]
        public DateTime? ProcessingDate { get; set; }

        [BsonElement("file_size_bytes")]
        public long FileSizeBytes { get; set; }

        [BsonElement("status")]
        public CohortStatus Status { get; set; } = CohortStatus.Discovered;

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
