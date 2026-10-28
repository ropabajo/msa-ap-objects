using System.Collections.Concurrent;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Api.Realtime;

internal sealed class ImportProgressConnections
{
    internal sealed class Session(string id, ImportRequester requester, Action abort)
    {
        internal string Id => id;
        internal ImportRequester Requester => requester;
        internal Action Abort => abort;
        private readonly object _gate = new();
        private Guid[] _codes = [];
        private readonly Dictionary<Guid, long> _versions = [];

        internal void Subscribe(Guid[] codes)
        {
            lock (_gate)
            {
                _codes = [.. codes];
                _versions.Clear();
            }
        }

        internal Guid[] Codes()
        {
            lock (_gate)
                return [.. _codes];
        }

        internal ImportProgressSnapshot[] Changes(IReadOnlyList<ImportProgressSnapshot> items)
        {
            lock (_gate)
            {
                var changed = items
                    .Where(x =>
                        _codes.Contains(x.BulkLoadCode)
                        && (
                            !_versions.TryGetValue(x.BulkLoadCode, out var version)
                            || x.Version > version
                        )
                    )
                    .ToArray();
                foreach (var item in changed)
                    _versions[item.BulkLoadCode] = item.Version;
                return changed;
            }
        }
    }

    private readonly object _gate = new();
    private readonly ConcurrentDictionary<string, Session> _sessions = new();

    internal Session? Add(string id, ImportRequester requester, Action abort)
    {
        lock (_gate)
        {
            if (
                _sessions.Count >= 128
                || _sessions.Values.Count(x =>
                    x.Requester.OwnerHash == requester.OwnerHash
                    && x.Requester.ChurchCode == requester.ChurchCode
                ) >= 3
            )
                return null;
            var session = new Session(id, requester, abort);
            return _sessions.TryAdd(id, session) ? session : null;
        }
    }

    internal Session? Get(string id) => _sessions.GetValueOrDefault(id);

    internal void Remove(string id) => _sessions.TryRemove(id, out _);

    internal Session[] Snapshot() => _sessions.Values.ToArray();
}
