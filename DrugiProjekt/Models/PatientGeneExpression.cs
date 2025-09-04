using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DrugiProjekt.Models
{
    public class PatientGeneExpression
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("patient_id")]
        public string PatientId { get; set; } = string.Empty;

        [BsonElement("cancer_cohort")]
        public string CancerCohort { get; set; } = string.Empty;

        [BsonElement("gene_expressions")]
        public Dictionary<string, double> GeneExpressions { get; set; } = new();

        [BsonElement("cgas_sting_genes")]
        public cGAS_STING_GenesExpression cGAS_STING_Genes { get; set; } = new();

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("source_file")]
        public string SourceFile { get; set; } = string.Empty;

        [BsonElement("processing_status")]
        public ProcessingStatus ProcessingStatus { get; set; } = ProcessingStatus.Pending;
    }
}
