using System.Globalization;
using System.Windows.Controls;

namespace AnEoT.Tools.WordToMarkdown.ValidationRules;

public sealed class StringInInt32ValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is string str && int.TryParse(str, out _))
        {
            return ValidationResult.ValidResult;
        }
        else
        {
            return new ValidationResult(false, "输入的内容不是整数");
        }
    }
}
