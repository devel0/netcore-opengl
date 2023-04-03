namespace test;

public static partial class Toolkit
{

    public static string? TestProjectFolder() => ParentPath(AppDomain.CurrentDomain.BaseDirectory, "test");

    public static void AssertEqualsTol(float tol, float expected, float actual, string userMessage = "")
    {
        if (!expected.EqualsTol(tol, actual))
        {
            if (userMessage.Length == 0)
                userMessage = $"Δ {(expected - actual)}";
            throw new Xunit.Sdk.AssertActualExpectedException(expected, actual, userMessage);
        }
    }

    public static void AssertEqualsTol(float tol, in Vector2 expected, in Vector2 actual, string userMessage = "")
    {
        if (!expected.EqualsTol(tol, actual))
        {
            if (userMessage.Length == 0)
                userMessage = $"Δ {(expected - actual)}";
            throw new Xunit.Sdk.AssertActualExpectedException(expected, actual, userMessage);
        }
    }

    public static void AssertEqualsTol(float tol, in Vector3 expected, in Vector3 actual, string userMessage = "")
    {
        if (!expected.EqualsTol(tol, actual))
        {
            if (userMessage.Length == 0)
                userMessage = $"Δ {(expected - actual)}";
            throw new Xunit.Sdk.AssertActualExpectedException(expected, actual, userMessage);
        }
    }

    public static void AssertEqualsTol(float tol, in Vector4 expected, in Vector4 actual, string userMessage = "")
    {
        if (!expected.EqualsTol(tol, actual))
        {
            if (userMessage.Length == 0)
                userMessage = $"Δ {(expected - actual)}";
            throw new Xunit.Sdk.AssertActualExpectedException(expected, actual, userMessage);
        }
    }

}