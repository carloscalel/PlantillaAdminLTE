using ASP.NET.MVC_NETFramework.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace ASP.NET.MVC_NETFramework.Services
{
    public static class UserRoleService
    {
        private static readonly Lazy<Dictionary<string, HashSet<string>>> RoleMap =
            new Lazy<Dictionary<string, HashSet<string>>>(BuildRoleMap);

        public static bool IsUserInAnyRole(string identityName, IEnumerable<string> requestedRoles)
        {
            if (string.IsNullOrWhiteSpace(identityName) || requestedRoles == null)
            {
                return false;
            }

            var requested = requestedRoles.Select(r => (r ?? string.Empty).Trim())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToList();

            if (!requested.Any()) return false;

            var dbRoles = GetRolesFromDatabase(identityName);
            if (dbRoles.Any())
            {
                return requested.Any(r => dbRoles.Contains(r, StringComparer.OrdinalIgnoreCase));
            }

            var normalizedUser = Normalize(identityName);
            if (!RoleMap.Value.TryGetValue(normalizedUser, out var assignedRoles))
            {
                return false;
            }

            return requested.Any(role => assignedRoles.Contains(role));
        }

        private static IList<string> GetRolesFromDatabase(string identityName)
        {
            try
            {
                return new AccessRepository().GetRoleCodesForIdentity(identityName);
            }
            catch
            {
                return new List<string>();
            }
        }

        private static Dictionary<string, HashSet<string>> BuildRoleMap()
        {
            var rawConfig = ConfigurationManager.AppSettings["InternalRoleMappings"] ?? string.Empty;
            var map = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

            var entries = rawConfig.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var entry in entries)
            {
                var parts = entry.Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    continue;
                }

                var user = Normalize(parts[0]);
                var roles = parts[1]
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x));

                map[user] = new HashSet<string>(roles, StringComparer.OrdinalIgnoreCase);
            }

            return map;
        }

        private static string Normalize(string identityName)
        {
            return identityName?.Trim().ToUpperInvariant() ?? string.Empty;
        }
    }
}
