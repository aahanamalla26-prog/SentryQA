using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace SentryQA.Core;

/// <summary>
/// Retries a failing test up to <paramref name="maxAttempts"/> times before
/// marking it failed — but unlike a plain retry, a test that only passes on
/// a later attempt is reported as <c>Passed (Flaky, attempt N)</c> rather
/// than a clean pass. The goal isn't to hide flakiness behind a retry, it's
/// to stop flakiness from blocking CI while still keeping it visible in the
/// report so it gets fixed instead of forgotten.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RetryTestAttribute : NUnitAttribute, IWrapTestMethod
{
    private readonly int _maxAttempts;

    public RetryTestAttribute(int maxAttempts = 2) => _maxAttempts = maxAttempts;

    public TestCommand Wrap(TestCommand command) => new RetryCommand(command, _maxAttempts);

    private class RetryCommand : DelegatingTestCommand
    {
        private readonly int _maxAttempts;

        public RetryCommand(TestCommand innerCommand, int maxAttempts) : base(innerCommand)
            => _maxAttempts = maxAttempts;

        public override TestResult Execute(TestExecutionContext context)
        {
            TestResult result = innerCommand.Execute(context);

            int attempt = 1;
            while (result.ResultState != ResultState.Success && attempt < _maxAttempts)
            {
                attempt++;
                TestLogger.Warn($"[RETRY] '{context.CurrentTest.Name}' failed on attempt {attempt - 1}, retrying ({attempt}/{_maxAttempts})...");
                context.CurrentResult = context.CurrentTest.MakeTestResult();
                result = innerCommand.Execute(context);
            }

            if (result.ResultState == ResultState.Success && attempt > 1)
            {
                TestLogger.Warn($"[FLAKY] '{context.CurrentTest.Name}' passed on attempt {attempt}/{_maxAttempts} — investigate before it starts failing outright.");
                result.SetResult(ResultState.Success, $"Passed (Flaky, attempt {attempt}/{_maxAttempts})");
            }

            return result;
        }
    }
}
