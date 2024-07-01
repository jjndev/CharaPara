namespace CharaPara.Data.Model
{
    public class SearchTag
    {
        public short Id { get; set; }

        public string Name { get; set; } = "<Error>";

        public string Description { get; set; } = "";

        public string SearchAliases { get; set; } = "";

        public SearchTag? ParentSearchTag { get; set; }
        public short ParentSearchTagId { get; set; }

        public SensitiveStatus SensitiveStatus { get; set; }





    }

}
