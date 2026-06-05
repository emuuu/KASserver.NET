namespace KASserver;

/// <summary>
/// Parameters for creating a mail forward (<c>add_mailforward</c>).
/// </summary>
public sealed class AddMailForward
{
    /// <summary>The local part of the forwarding address (before the <c>@</c>). Required.</summary>
    public required string LocalPart { get; set; }

    /// <summary>The domain part of the forwarding address (an existing domain on the account). Required.</summary>
    public required string DomainPart { get; set; }

    /// <summary>The target addresses. At least one, at most ten (KAS <c>target_0</c>..<c>target_9</c>).</summary>
    public required IReadOnlyList<string> Targets { get; set; }

    internal IReadOnlyDictionary<string, object?> ToParameters()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(LocalPart);
        ArgumentException.ThrowIfNullOrWhiteSpace(DomainPart);

        if (Targets is null || Targets.Count == 0)
            throw new ArgumentException("At least one target address is required.", nameof(Targets));

        if (Targets.Count > 10)
            throw new ArgumentException("The KAS API accepts at most ten target addresses.", nameof(Targets));

        var parameters = new Dictionary<string, object?>
        {
            ["local_part"] = LocalPart,
            ["domain_part"] = DomainPart,
        };

        for (var i = 0; i < Targets.Count; i++)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(Targets[i], $"{nameof(Targets)}[{i}]");
            parameters[$"target_{i}"] = Targets[i];
        }

        return parameters;
    }
}
