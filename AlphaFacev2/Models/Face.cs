namespace AlphaFacev2.Models
{
    public partial class Face
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public double Accuracy { get; set; }
        public string Gender { get; set; }
        public string FaceGuid { get; set; }
    }
}
