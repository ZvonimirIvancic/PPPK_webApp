using MongoDB.Bson.Serialization.Attributes;

namespace DrugiProjekt.Models
{
    public class cGAS_STING_GenesExpression
    {
        [BsonElement("C6orf150_cGAS")]
        public double C6orf150_cGAS { get; set; }

        [BsonElement("CCL5")]
        public double CCL5 { get; set; }

        [BsonElement("CXCL10")]
        public double CXCL10 { get; set; }

        [BsonElement("TMEM173_STING")]
        public double TMEM173_STING { get; set; }

        [BsonElement("CXCL9")]
        public double CXCL9 { get; set; }

        [BsonElement("CXCL11")]
        public double CXCL11 { get; set; }

        [BsonElement("NFKB1")]
        public double NFKB1 { get; set; }

        [BsonElement("IKBKE")]
        public double IKBKE { get; set; }

        [BsonElement("IRF3")]
        public double IRF3 { get; set; }

        [BsonElement("TREX1")]
        public double TREX1 { get; set; }

        [BsonElement("ATM")]
        public double ATM { get; set; }

        [BsonElement("IL6")]
        public double IL6 { get; set; }

        [BsonElement("IL8_CXCL8")]
        public double IL8_CXCL8 { get; set; }
    }
}
