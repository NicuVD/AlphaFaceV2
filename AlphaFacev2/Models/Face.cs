namespace AlphaFacev2.Models
{
    public partial class Face
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public byte[] ProfileImage { get; set; }
        public byte[] ComparisonImage { get; set; }
        public bool IsIdentical { get; set; }
        public double Confidence { get; set; }
        public string Gender { get; set; }
        public string FaceGuid { get; set; }
        //public bool IsComparisonSucces { get; set; }

    }
}
