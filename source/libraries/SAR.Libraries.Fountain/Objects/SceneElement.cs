using System;
using System.Text.RegularExpressions;

namespace SAR.Libraries.Fountain.Objects
{
    public class SceneElement : Element
    {
        private const string scene_number_pattern = @"(\#([0-9A-Za-z\.\)-]+)\#)";

        public SceneElement(string rawData)
            : base(rawData)
        {
            string temp = rawData;

            //Extract Scene Number
            var match = Regex.Match(temp, scene_number_pattern);
            if (match.Success)
            {
                SceneNumber = match.Groups[1].Value;

                temp = temp.Replace(match.Groups[0].Value, "").Trim();
            }

            if (temp.StartsWith("."))
            {
                //Forced Scene Heading
                Location = temp;
                return;
            }

            //Extract Time of Day
            int dashPosition = temp.LastIndexOf("-");
            if (dashPosition > 0)
            {
                TimeOfDay = temp.Substring(dashPosition + 1).Trim();

                temp = temp.Substring(0, dashPosition - 1);
            }

            int spacePosition = temp.IndexOf(" ");
            if (spacePosition > 0)
            {
                InteriorExterior = temp.Substring(0, spacePosition).Trim();
                temp = temp.Substring(spacePosition + 1).Trim();
            }

            Location = temp;
        }

        public String InteriorExterior { get; set; }
        public string Location { get; set; }
        public string TimeOfDay { get; set; }
        public string SceneNumber { get; set; }
    }
}
