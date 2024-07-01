namespace CharaPara.App.Extensions
{
    public static class SearchTagStringExtensions
    {
        public static string ValidateSearchTagString(this string searchTagString)
        {
            //filter the string in title case
            var returnString = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(searchTagString);
            //split and trim the string by comma
            var returnStringList = searchTagString.Split(',').Select(x => x.Trim()).ToList();









            //remove additional spaces
            returnString = returnString.Replace("  ", " ");
            returnString = returnString.Replace(", ", ",");
            returnString = returnString.Replace(" ,", ",");
            //return the string
            return returnString;
        }

        public static string FormatSearchTagStringToHtml(this string searchTagString)
        {
            //split the string by comma
            var splitString = searchTagString.Split(',');
            //return each string enclosed in a div
            return string.Join("", splitString.Select(x => $"<div class='badge badge-secondary'>{x}</div>"));

     
        }


    }
}
