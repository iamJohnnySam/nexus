using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Dapper;
using ProjectManager.Models;
using System.Collections.ObjectModel;

namespace ProjectManager
{
	public class Manager : IManager
	{
		private readonly string dbPath = "NexusDB.sqlite";
		private readonly string connectionString;

		private Project currentProject;
		public Project CurrentProject
		{
			get { return currentProject; }
			set { currentProject = value; }
		}

		public List<Project> Projects
		{
			get
			{
				using var connection = new SQLiteConnection(connectionString);
				connection.Open();
				return connection.Query<Project>("SELECT * FROM Project;").AsList();
			}
		}


		public Manager()
		{
			connectionString = $"Data Source={dbPath};Version=3;";

			bool databaseNotExist = false;
			if (!File.Exists(dbPath))
			{
				SQLiteConnection.CreateFile(dbPath);
				databaseNotExist = true;
			}
			using var connection = new SQLiteConnection(connectionString);
			connection.Open();

			if (databaseNotExist)
			{
				string sqlScript = File.ReadAllText("DB/schema.sql");
				connection.Execute(sqlScript);

				var newProject = new Project
				{
					ProjectName = "General",
					CustomerID = 1,
					DesignCode = "GENERAL",
					Priority = ProjectPriority.Normal,
					POStatus = SalesStatus.Concept,
					ProductId = 1
				};
				InsertProject(connection, newProject);
			}

			currentProject = connection.QueryFirstOrDefault<Project>("SELECT * FROM Project ORDER BY ProjectId ASC LIMIT 1;")!;
		}


		static void InsertProject(SQLiteConnection conn, Project p)
		{
			string sql = @"INSERT INTO Project (ProjectName, CustomerID, DesignCode, Priority, POStatus, ProductId)
                       VALUES (@ProjectName, @CustomerID, @DesignCode, @Priority, @POStatus, @ProductId);";
			conn.Execute(sql, p);
		}

		static void UpdateProject(SQLiteConnection conn, Project p)
		{
			string sql = @"UPDATE Project
                       SET ProjectName = @ProjectName,
                           CustomerID = @CustomerID,
                           DesignCode = @DesignCode,
                           Priority = @Priority,
                           POStatus = @POStatus,
                           ProductId = @ProductId
                       WHERE ProjectId = @ProjectId;";
			conn.Execute(sql, p);
		}

		static void DeleteProject(SQLiteConnection conn, int projectId)
		{
			string sql = "DELETE FROM Project WHERE ProjectId = @ProjectId;";
			conn.Execute(sql, new { ProjectId = projectId });
		}

	}
}
