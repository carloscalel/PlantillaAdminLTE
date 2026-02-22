using ASP.NET.MVC_NETFramework.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace ASP.NET.MVC_NETFramework.Data
{
    public class AccessRepository
    {
        private readonly string _connectionString;

        public AccessRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["AdventureWorks"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                throw new ConfigurationErrorsException("No se encontró la cadena de conexión 'AdventureWorks'.");
            }
        }

        public IList<AccessRole> GetRoles()
        {
            const string query = "SELECT RoleId, RoleCode, RoleName, IsActive FROM Security.Roles ORDER BY RoleName";
            var roles = new List<AccessRole>();
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        roles.Add(new AccessRole
                        {
                            RoleId = rd.GetInt32(0),
                            RoleCode = rd.GetString(1),
                            RoleName = rd.GetString(2),
                            IsActive = rd.GetBoolean(3)
                        });
                    }
                }
            }
            return roles;
        }

        public IList<AccessUser> GetUsers()
        {
            const string query = @"
SELECT u.UserId, u.UserName, u.DisplayName, u.Email, u.IsActive,
       STUFF((SELECT ', ' + r.RoleCode
              FROM Security.UserRoles ur2
              JOIN Security.Roles r ON r.RoleId = ur2.RoleId
              WHERE ur2.UserId = u.UserId
              FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS RolesSummary
FROM Security.Users u
ORDER BY u.UserName";
            var users = new List<AccessUser>();
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        users.Add(new AccessUser
                        {
                            UserId = rd.GetInt32(0),
                            UserName = rd.GetString(1),
                            DisplayName = rd.IsDBNull(2) ? null : rd.GetString(2),
                            Email = rd.IsDBNull(3) ? null : rd.GetString(3),
                            IsActive = rd.GetBoolean(4),
                            RolesSummary = rd.IsDBNull(5) ? "(sin roles)" : rd.GetString(5)
                        });
                    }
                }
            }
            return users;
        }

        public AccessUser GetUserById(int userId)
        {
            const string userQuery = "SELECT UserId, UserName, DisplayName, Email, IsActive FROM Security.Users WHERE UserId = @UserId";
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(userQuery, cn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return null;
                    var user = new AccessUser
                    {
                        UserId = rd.GetInt32(0),
                        UserName = rd.GetString(1),
                        DisplayName = rd.IsDBNull(2) ? null : rd.GetString(2),
                        Email = rd.IsDBNull(3) ? null : rd.GetString(3),
                        IsActive = rd.GetBoolean(4)
                    };

                    user.SelectedRoleIds = GetRoleIdsByUser(userId);
                    return user;
                }
            }
        }

        public IList<int> GetRoleIdsByUser(int userId)
        {
            const string query = "SELECT RoleId FROM Security.UserRoles WHERE UserId = @UserId";
            var result = new List<int>();
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read()) result.Add(rd.GetInt32(0));
                }
            }
            return result;
        }

        public void CreateUser(AccessUser user, string assignedBy)
        {
            const string query = @"INSERT INTO Security.Users (UserName, DisplayName, Email, IsActive, UpdatedAt)
VALUES (@UserName, @DisplayName, @Email, @IsActive, SYSDATETIME()); SELECT SCOPE_IDENTITY();";
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {
                cmd.Parameters.AddWithValue("@UserName", user.UserName.Trim());
                cmd.Parameters.AddWithValue("@DisplayName", (object)user.DisplayName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object)user.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
                cn.Open();
                user.UserId = Convert.ToInt32(cmd.ExecuteScalar());
            }
            ReplaceUserRoles(user.UserId, user.SelectedRoleIds, assignedBy);
        }

        public void UpdateUser(AccessUser user, string assignedBy)
        {
            const string query = @"UPDATE Security.Users
SET UserName = @UserName, DisplayName = @DisplayName, Email = @Email, IsActive = @IsActive, UpdatedAt = SYSDATETIME()
WHERE UserId = @UserId";
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {
                cmd.Parameters.AddWithValue("@UserId", user.UserId);
                cmd.Parameters.AddWithValue("@UserName", user.UserName.Trim());
                cmd.Parameters.AddWithValue("@DisplayName", (object)user.DisplayName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object)user.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
            ReplaceUserRoles(user.UserId, user.SelectedRoleIds, assignedBy);
        }

        public void DeleteUser(int userId)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    var deleteRoles = new SqlCommand("DELETE FROM Security.UserRoles WHERE UserId = @UserId", cn, tx);
                    deleteRoles.Parameters.AddWithValue("@UserId", userId);
                    deleteRoles.ExecuteNonQuery();

                    var deleteUser = new SqlCommand("DELETE FROM Security.Users WHERE UserId = @UserId", cn, tx);
                    deleteUser.Parameters.AddWithValue("@UserId", userId);
                    deleteUser.ExecuteNonQuery();

                    tx.Commit();
                }
            }
        }

        public IList<string> GetRoleCodesForIdentity(string identityName)
        {
            const string query = @"
SELECT DISTINCT r.RoleCode
FROM Security.Users u
JOIN Security.UserRoles ur ON ur.UserId = u.UserId
JOIN Security.Roles r ON r.RoleId = ur.RoleId
WHERE u.IsActive = 1
  AND r.IsActive = 1
  AND (
        UPPER(u.UserName) = UPPER(@UserName)
        OR (CHARINDEX('\\', u.UserName) > 0 AND UPPER(SUBSTRING(u.UserName, CHARINDEX('\\', u.UserName) + 1, LEN(u.UserName))) = UPPER(@UserNameOnly))
      )";

            var normalized = (identityName ?? string.Empty).Trim();
            var nameOnly = normalized.Contains("\\") ? normalized.Split('\\').Last() : normalized;

            var roles = new List<string>();
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, cn))
            {
                cmd.Parameters.AddWithValue("@UserName", normalized);
                cmd.Parameters.AddWithValue("@UserNameOnly", nameOnly);
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read()) roles.Add(rd.GetString(0));
                }
            }
            return roles;
        }

        private void ReplaceUserRoles(int userId, IEnumerable<int> roleIds, string assignedBy)
        {
            var list = (roleIds ?? Enumerable.Empty<int>()).Distinct().ToList();
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    var clearRoles = new SqlCommand("DELETE FROM Security.UserRoles WHERE UserId = @UserId", cn, tx);
                    clearRoles.Parameters.AddWithValue("@UserId", userId);
                    clearRoles.ExecuteNonQuery();

                    foreach (var roleId in list)
                    {
                        var insert = new SqlCommand(@"INSERT INTO Security.UserRoles (UserId, RoleId, AssignedBy)
VALUES (@UserId, @RoleId, @AssignedBy)", cn, tx);
                        insert.Parameters.AddWithValue("@UserId", userId);
                        insert.Parameters.AddWithValue("@RoleId", roleId);
                        insert.Parameters.AddWithValue("@AssignedBy", (object)assignedBy ?? DBNull.Value);
                        insert.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
            }
        }
    }
}
