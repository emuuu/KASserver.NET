namespace KASserver;

/// <summary>
/// Parameters for changing ownership of a path (<c>update_chown</c>). Requires a superuser login.
/// </summary>
public sealed class UpdateChown
{
    /// <summary>The path whose ownership is changed (<c>chown_path</c>). Required.</summary>
    public required string Path { get; set; }

    /// <summary>The new owner (<c>chown_user</c>), e.g. a PHP user or a KAS login. Required.</summary>
    public required string User { get; set; }

    /// <summary>Apply the change recursively (<c>recursive</c>). Default: <c>false</c> (<c>N</c>).</summary>
    public bool Recursive { get; set; }

    internal IReadOnlyDictionary<string, object?> ToParameters()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Path);
        ArgumentException.ThrowIfNullOrWhiteSpace(User);

        return new Dictionary<string, object?>
        {
            ["chown_path"] = Path,
            ["chown_user"] = User,
            ["recursive"] = Recursive ? "Y" : "N",
        };
    }
}
