using Newtonsoft.Json;
using CharaPara.Data.Model;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace CharaPara.App
{
    public interface IProfileTabEncoder
    {
        public string TabToHtmlForm(Tab tab);
        //encode the tab data and tab template to html - form

        public string TabToHtmlNameValue(Tab tab);
        //encode the tab data and tab template to html - display

    }

    public class ProfileTabEncoder : IProfileTabEncoder
    {
        public string TabToHtmlForm(Tab tab)
        {
            List<FormObject> formObjects = JsonConvert.DeserializeObject<List<FormObject>>(tab.RawContent);

            if (formObjects == null)
            {
                return "";
            }

            formObjects.Sort((x, y) => x.Order.CompareTo(y.Order));

            List<string> htmlStrings = new List<string>();
            foreach (FormObject formObject in formObjects)
            {
                htmlStrings.Add(EncodeFormObjectToHtml(formObject));
            }

            return string.Join("<br>", htmlStrings);
        }

        public string TabToHtmlNameValue(Tab tab)
        {
            if (tab.RawContent == null) return "";
            List<FormObject> formObjects = JsonConvert.DeserializeObject<List<FormObject>>(tab.RawContent);

            if (formObjects == null)
            {
                return "";
            }

            formObjects.Sort((x, y) => x.Order.CompareTo(y.Order));

            List<string> htmlStrings = new List<string>();
            foreach (FormObject formObject in formObjects)
            {
                htmlStrings.Add(EncodeFormObjectToHtmlNameValue(formObject));
            }

            return string.Join("<br>", htmlStrings);
        }

        private string EncodeFormObjectToHtmlNameValue(FormObject formObject)
        {
            StringBuilder nameValueString = new StringBuilder();
            nameValueString.Append(formObject.Name + ": ");

            switch (formObject.ObjectType)
            {
                case FormObjectType.SmallText:
                case FormObjectType.MediumText:
                case FormObjectType.LargeText:
                    nameValueString.Append(formObject.Values[0]);
                    break;
                case FormObjectType.Checkbox:
                    for (int i = 0; i < formObject.Fields.Count; i++)
                    {
                        if (formObject.Values[i] == "1")
                        {
                            nameValueString.Append(formObject.Fields[i] + ": checked, ");
                        }
                        else
                        {
                            nameValueString.Append(formObject.Fields[i] + ": unchecked, ");
                        }
                    }
                    break;
                case FormObjectType.Dropdown:
                    nameValueString.Append(formObject.Fields[int.Parse(formObject.Values[0])]);
                    break;
                default:
                    break;
            }

            return nameValueString.ToString();
        }



        private string EncodeFormObjectToHtml(FormObject formObject)
        {
            StringBuilder htmlString = new StringBuilder();
            htmlString.Append("<label>" + formObject.Name + "</label><br>");

            switch (formObject.ObjectType)
            {
                case FormObjectType.SmallText:
                    htmlString.Append("<input type=\"text\" value=\"" + string.Join(",", formObject.Values) + "\" size=\"10\">");
                    break;
                case FormObjectType.MediumText:
                    htmlString.Append("<input type=\"text\" value=\"" + string.Join(",", formObject.Values) + "\" size=\"20\">");
                    break;
                case FormObjectType.LargeText:
                    htmlString.Append("<input type=\"text\" value=\"" + string.Join(",", formObject.Values) + "\" size=\"40\">");
                    break;
                case FormObjectType.Checkbox:
                    for (int i = 0; i < formObject.Fields.Count; i++)
                    {
                        htmlString.Append("<input type=\"checkbox\" id=\"" + formObject.Fields[i] + "\" name=\"" + formObject.Fields[i] + "\"");
                        if (formObject.Values[i] == "1")
                        {
                            htmlString.Append(" checked");
                        }
                        htmlString.Append("> " + formObject.Fields[i] + "<br>");
                    }
                    break;
                case FormObjectType.Dropdown:
                    htmlString.Append("<select>");
                    for (int i = 0; i < formObject.Fields.Count; i++)
                    {
                        htmlString.Append("<option value=\"" + formObject.Fields[i] + "\"");
                        if (i == int.Parse(formObject.Values[0]))
                        {
                            htmlString.Append(" selected");
                        }
                        htmlString.Append(">" + formObject.Fields[i] + "</option>");
                    }
                    htmlString.Append("</select>");
                    break;
                default:
                    break;
            }

            return htmlString.ToString();
        }


        private string ConvertFormObjectsToTabString(List<FormObject> formObjects)
        {
            return JsonConvert.SerializeObject(formObjects);
        }



        public class FormObject
        {
            public string Name { get; set; }
            public FormObjectType ObjectType { get; set; }
            public List<string> Fields { get; set; }
            public List<string> Values { get; set; }
            public string Tooltip { get; set; }
            public byte Order { get; set; }
        }

    public enum FormObjectType : byte
    {
        SmallText,
        MediumText,
        LargeText,
        Checkbox,
        Dropdown
    }
}

enum FormObjectType
        {
            SmallText,
            MediumText,
            LargeText,
            Text,
            Checkbox,
            Dropdown
        }
    }



