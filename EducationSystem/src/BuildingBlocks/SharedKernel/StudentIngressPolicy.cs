namespace SharedKernel;

public static class StudentIngressPolicy
{
    public static bool IsAllowedPath(string path)
    {
        if (string.Equals(path, "/api/auth/student/login", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return path.StartsWith("/api/student/me/", StringComparison.OrdinalIgnoreCase)
            && !path.Contains("..", StringComparison.Ordinal);
    }
}
