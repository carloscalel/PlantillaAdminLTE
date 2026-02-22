using ASP.NET.MVC_NETFramework.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace ASP.NET.MVC_NETFramework.Data
{
    public class DepartmentRepository
    {
        private readonly string _connectionString;

        public DepartmentRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["AdventureWorks"]?.ConnectionString;

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                throw new ConfigurationErrorsException("No se encontró la cadena de conexión 'AdventureWorks'.");
            }
        }

        public IList<Department> GetAll()
        {
            var departments = new List<Department>();

            const string query = @"
                SELECT DepartmentID, Name, GroupName, ModifiedDate
                FROM HumanResources.Department
                ORDER BY Name";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        departments.Add(MapDepartment(reader));
                    }
                }
            }

            return departments;
        }

        public Department GetById(short id)
        {
            const string query = @"
                SELECT DepartmentID, Name, GroupName, ModifiedDate
                FROM HumanResources.Department
                WHERE DepartmentID = @DepartmentID";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@DepartmentID", id);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapDepartment(reader);
                    }
                }
            }

            return null;
        }

        public void Create(Department department)
        {
            const string query = @"
                INSERT INTO HumanResources.Department (Name, GroupName, ModifiedDate)
                VALUES (@Name, @GroupName, @ModifiedDate)";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", department.Name);
                command.Parameters.AddWithValue("@GroupName", department.GroupName);
                command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Update(Department department)
        {
            const string query = @"
                UPDATE HumanResources.Department
                SET Name = @Name,
                    GroupName = @GroupName,
                    ModifiedDate = @ModifiedDate
                WHERE DepartmentID = @DepartmentID";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@DepartmentID", department.DepartmentID);
                command.Parameters.AddWithValue("@Name", department.Name);
                command.Parameters.AddWithValue("@GroupName", department.GroupName);
                command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Delete(short id)
        {
            const string query = @"DELETE FROM HumanResources.Department WHERE DepartmentID = @DepartmentID";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@DepartmentID", id);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static Department MapDepartment(SqlDataReader reader)
        {
            return new Department
            {
                DepartmentID = reader.GetInt16(reader.GetOrdinal("DepartmentID")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                GroupName = reader.GetString(reader.GetOrdinal("GroupName")),
                ModifiedDate = reader.GetDateTime(reader.GetOrdinal("ModifiedDate"))
            };
        }
    }
}
