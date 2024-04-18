namespace AnEoT.Tools.MarkdownChecker.Models;

public record struct CheckResult(int ErrorCount, int WarningCount)
{
    public static CheckResult operator +(CheckResult a, CheckResult b)
    {
        return new CheckResult(a.ErrorCount + b.ErrorCount, a.WarningCount + b.WarningCount);
    }

    public readonly bool HasError => ErrorCount != 0;

    public readonly bool HasWarning => WarningCount != 0;

    public readonly bool IsNoErrorOrWarning => ErrorCount == 0 && WarningCount == 0;
}
