using System.Globalization;
using System.Text;

namespace Modules.AudioEvents
{
    internal static class InternalUtils
    {
        public static string ToTitleCase(string input)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(char.ToUpper(input[0]));

            for (int index = 1; index < input.Length; ++index)
            {
                char ch = input[index];
                if (ch == '_' && index + 1 < input.Length)
                {
                    char upper = input[index + 1];
                    if (char.IsLower(upper))
                        upper = char.ToUpper(upper, CultureInfo.InvariantCulture);
                    stringBuilder.Append(upper);
                    ++index;
                }
                else
                    stringBuilder.Append(ch);
            }

            return stringBuilder.ToString();
        }
    }
}