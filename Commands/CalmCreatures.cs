using System.Collections.Generic;
using System.Linq;
using System.Text;
using ValheimRcon.Commands.Search;
using ValheimRcon.ZDOInfo;

namespace ValheimRcon.Commands
{
    internal class CalmCreatures : RconCommand
    {
        public override string Command => "calmCreatures";

        public override string Description => "Clears the anger of aggravated creatures such as dvergr, in the whole world or near a point. " +
            "Usage (with optional arguments): calmCreatures " +
            "-near <x> <y> <z> <radius>";

        // Registered by BaseAI.Awake; only the owner of the creature applies it and writes the flag back.
        private const string SetAggravatedRpc = "SetAggravated";

        private const int MaxListed = 20;

        private static readonly Dictionary<int, bool> _aggravatable = new Dictionary<int, bool>();

        protected override string OnHandle(CommandArgs args)
        {
            var criterias = new List<ISearchCriteria>();
            foreach (var (index, argument) in args.GetOptionalArguments())
            {
                switch (argument.ToLower())
                {
                    case "-near":
                        criterias.Add(new NearCriteria(args.GetVector3(index + 1), args.GetFloat(index + 4)));
                        break;
                    default:
                        return $"Unknown argument: {argument}";
                }
            }

            var angry = ZDOMan.instance.m_objectsByID.Values
                .Where(zdo => criterias.All(c => c.IsMatch(zdo)) && IsAggravatable(zdo) && zdo.GetBool(ZDOVars.s_aggravated))
                .ToArray();

            if (angry.Length == 0)
            {
                return "No angry creatures found.";
            }

            var peers = new HashSet<long>(ZNet.instance.m_peers.Select(p => p.m_uid));
            var sb = new StringBuilder();
            sb.AppendLine($"Calming {angry.Length} creatures:");
            var listed = 0;
            foreach (var zdo in angry)
            {
                var owner = zdo.GetOwner();
                var ownerOnline = peers.Contains(owner);
                if (ownerOnline)
                {
                    // The owner simulates the creature, so a server-side write would race with its own updates.
                    ZRoutedRpc.instance.InvokeRoutedRPC(owner, zdo.m_uid, SetAggravatedRpc, false, 0);
                }
                else
                {
                    zdo.Set(ZDOVars.s_aggravated, false);
                }

                if (listed++ < MaxListed)
                {
                    sb.Append('-');
                    ZDOInfoUtil.AppendInfo(zdo, sb, false);
                    sb.AppendLine(ownerOnline ? " [sent to owner]" : " [cleared]");
                }
            }

            if (angry.Length > MaxListed)
            {
                sb.AppendLine($"...and {angry.Length - MaxListed} more");
            }
            return sb.ToString().TrimEnd();
        }

        private static bool IsAggravatable(ZDO zdo)
        {
            var prefabHash = zdo.GetPrefab();
            if (_aggravatable.TryGetValue(prefabHash, out var aggravatable))
                return aggravatable;

            var prefab = ZNetScene.instance.GetPrefab(prefabHash);
            aggravatable = prefab != null && prefab.TryGetComponent<BaseAI>(out var ai) && ai.m_aggravatable;
            _aggravatable[prefabHash] = aggravatable;
            return aggravatable;
        }
    }
}
