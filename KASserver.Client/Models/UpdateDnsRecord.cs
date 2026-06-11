using System.Globalization;

namespace KASserver;

/// <summary>
/// Parameters for editing a DNS resource record (<c>update_dns_settings</c>). Only properties that
/// are set (non-null) are sent; everything left <c>null</c> stays unchanged on the server. The record
/// is identified by its <c>record_id</c>, passed separately to
/// <see cref="IKasClient.UpdateDnsRecordAsync"/>. At least one field must be set. The record type
/// cannot be changed via this action (the KAS API does not accept it here).
/// </summary>
public sealed class UpdateDnsRecord
{
    /// <summary>The record name (<c>record_name</c>). Optional.</summary>
    public string? RecordName { get; set; }

    /// <summary>The record data (<c>record_data</c>). Optional.</summary>
    public string? RecordData { get; set; }

    /// <summary>The AUX value (<c>record_aux</c>), the priority for <c>MX</c>/<c>SRV</c> records. Optional.</summary>
    public int? Aux { get; set; }

    internal IReadOnlyDictionary<string, object?> ToParameters(string recordId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recordId);

        var parameters = new Dictionary<string, object?> { ["record_id"] = recordId };

        if (RecordName is not null)
        {
            // Empty record_name is the zone apex; whitespace-only is rejected.
            if (RecordName.Length > 0 && string.IsNullOrWhiteSpace(RecordName))
                throw new ArgumentException("RecordName must be empty (zone apex) or a non-whitespace label.", nameof(RecordName));
            parameters["record_name"] = RecordName;
        }

        if (RecordData is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(RecordData);
            parameters["record_data"] = RecordData;
        }

        if (Aux is not null)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(Aux.Value, nameof(Aux));
            parameters["record_aux"] = Aux.Value.ToString(CultureInfo.InvariantCulture);
        }

        // record_id alone is a no-op that KAS rejects with "nothing_to_do" — fail fast on the client.
        if (parameters.Count == 1)
            throw new ArgumentException($"At least one field on {nameof(UpdateDnsRecord)} must be set.");

        return parameters;
    }
}
