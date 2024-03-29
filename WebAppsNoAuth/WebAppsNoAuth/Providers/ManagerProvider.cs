﻿using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.ML;
using Org.BouncyCastle.Bcpg.Sig;
using WebAppsNoAuth.Data;
using WebAppsNoAuth.Models;
using static EmpSentimentModel.ConsoleApp.EmpSentimentModel;
//using Microsoft.ML.HalLearners;
//using Microsoft.ML.LightGBM;

namespace WebAppsNoAuth.Providers
{
	public class ManagerProvider
	{
        private readonly SqlConnection _connection;
        private readonly WebAppsNoAuthDbContext _webApps;
        private readonly IConfiguration _configuration;


        public ManagerProvider(WebAppsNoAuthDbContext webApps, IConfiguration configuration)
        {
            _webApps = webApps;
            _configuration = configuration;
            // _connection = connection;
            _connection = new SqlConnection(_configuration.GetConnectionString("WebAppsNoAuthDb"));

        }

        public List<Dropdown> GetAllManagers(int userId, int institutionId)
        {
            List<Dropdown> allManagers = new List<Dropdown>();
            try
            {
                _connection.Open();
                var queryString = "SELECT Id, Name FROM [Users] WHERE Manager = @TRUE AND InstitutionId = @INSTITUTIONID"; //WHERE INSTITUTION ID = THE SAME ONE AS THE ADMIN WHO IS ON UPDATEUSERPAGE 
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@TRUE", true);
                command.Parameters.AddWithValue("@INSTITUTIONID", institutionId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())

                    {
                        Dropdown currentManager = new Dropdown();
                        currentManager.Id = dbReader.GetInt32(0);
                        currentManager.Name = dbReader.GetString(1);
                        allManagers.Add(currentManager);
                    }
                    _connection.Close();
                    return allManagers;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public List<User> GetAllManagerUsers(int managerUserId) //get all the users who the manager manages 
        {
            List<User> allUsers = new List<User>();
            try
            {
                _connection.Open();
                var queryString = "SELECT U.[Id], U.[Name] FROM Users U WHERE U.[Id] = @USERID " +
                                    "UNION SELECT U.[Id], U.[Name] FROM Users U WHERE U.[ManagerUserId] = @USERID";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", managerUserId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        User currentUser = new User();
                        currentUser.Id = dbReader.GetInt32(0);
                        currentUser.Name = dbReader.GetString(1);
                        allUsers.Add(currentUser);
                    }
                    _connection.Close();
                    return allUsers;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public List<Project> GetAllProjects(int userId)
        {
            List<Project> allProjects = new List<Project>();
            try
            {
                _connection.Open();
                var queryString = "SELECT [Project].ProjectId, [Project].Name, [Project].Description, [Project].Difficulty, " +
                                  "[Users].Name, [Project].CreatedDate, [Project].CreatedBy, [Project].EndDate, [Project].Completed FROM [Project] " +
                                  "JOIN [Users] ON [Project].[ManagerUserId] = [Users].Id WHERE CreatedBy = @MANAGERUSERID"; //WHERE INSTITUTION ID = THE SAME ONE AS THE ADMIN WHO IS ON UPDATEUSERPAGE 
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@MANAGERUSERID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        Project currentProject = new Project();
                        currentProject.ProjectId = dbReader.GetInt32(0);
                        currentProject.Name = dbReader.GetString(1);
                        currentProject.Description = dbReader.GetString(2);
                        currentProject.Difficulty = dbReader.GetInt32(3);
                        currentProject.ManagerUserName = dbReader.GetString(4);
                        currentProject.CreatedDate = dbReader.GetDateTime(5);
                        currentProject.CreatedDateStr = dbReader.GetDateTime(5).ToString("dd/MM/yyyy");
                        currentProject.CreatedBy = dbReader.GetInt32(6);
                        currentProject.EndDate = dbReader.IsDBNull(7) ? new DateTime(1) : dbReader.GetDateTime(7);
                        currentProject.EndDateStr = dbReader.IsDBNull(7) ? "" : currentProject.EndDate.ToString("dd/MM/yyyy");
                        currentProject.Completed = dbReader.IsDBNull(7) ? false : dbReader.GetBoolean(8);
                        allProjects.Add(currentProject);
                    }
                    _connection.Close();
                    return allProjects;
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public Project GetProjectById(int projectId)
        {
            Project currentProject = new Project();
            try
            {
                _connection.Open();
                var queryString = "SELECT * FROM [Project] WHERE ProjectId = @PROJECTID";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@PROJECTID", projectId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        currentProject.ProjectId = dbReader.GetInt32(0);
                        currentProject.Name = dbReader.GetString(1);
                        currentProject.Description = dbReader.GetString(2);
                        currentProject.Difficulty = dbReader.GetInt32(3);
                        currentProject.ManagerUserId = dbReader.GetInt32(4);
                        currentProject.CreatedDate = dbReader.GetDateTime(5);
                        currentProject.CreatedDateStr = dbReader.GetDateTime(5).ToString("dd/MM/yyyy");
                        currentProject.CreatedBy = dbReader.GetInt32(6);
                    }
                    _connection.Close();
                    return currentProject;
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public bool AddNewProject(string projectName, string projectDes, int projectDiff, int managerId, IEnumerable<Int32> projectPplList, DateTime projectCreated, int userId)
        {
            var addedProjectId = -1;
            try
            {
                _connection.Open();

                var queryString = "INSERT INTO [Project] VALUES (@NAME, @DESCRIPTION, @DIFFICULTY, @MANAGERID, @CREATEDATE, @CREATEDBY, NULL, @FALSE); " +
                                  "SELECT CAST(SCOPE_IDENTITY() AS INT);";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@NAME", projectName);
                command.Parameters.AddWithValue("@DESCRIPTION", projectDes);
                command.Parameters.AddWithValue("@DIFFICULTY", projectDiff);
                command.Parameters.AddWithValue("@MANAGERID", managerId);
                command.Parameters.AddWithValue("@CREATEDATE", projectCreated);
                command.Parameters.AddWithValue("@CREATEDBY", userId);
                command.Parameters.AddWithValue("@FALSE", false);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        addedProjectId = dbReader.GetInt32(0);
                    }
                }
                _connection.Close();

                for (var i = 0; i < projectPplList.Count(); i++)
                {
                    if (i == 0)
                    {
                        AddNewProjectPerson(addedProjectId, projectPplList.ElementAt(i), 1); //make it active for the host of the meeting 
                    }
                    else
                    {
                        AddNewProjectPerson(addedProjectId, projectPplList.ElementAt(i));
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }

        }

        public void AddNewProjectPerson(int projectId, int userId, int active = 1) //if the meeting is not active, on meeting page will come up with 'Accept' option 
        {
            try
            {
                _connection.Open();

                var queryString = "INSERT INTO [ProjectPerson] VALUES (@PROJECTID, @USERID); "; //set active as 0 by default 
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@PROJECTID", projectId);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@ACTIVE", active);
                command.ExecuteNonQuery();
                _connection.Close();
                return;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return;
            }

        }

        public List<Skill> GetAllSkills()
        {
            List<Skill> allSkills = new List<Skill>();
            try
            {
                _connection.Open();
                var queryString = "SELECT * FROM Skill";
                SqlCommand command = new SqlCommand(queryString, _connection);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        Skill skill = new Skill();
                        skill.SkillId = dbReader.GetInt32(0);
                        skill.SkillName = dbReader.GetString(1);
                        allSkills.Add(skill);
                    }
                    _connection.Close();
                    return allSkills;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }
        public void AddNewSkill(string skillName) 
        {
            try
            {
                _connection.Open();

                var queryString = "INSERT INTO [Skill] VALUES (@SKILLNAME); "; //set active as 0 by default 
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@SKILLNAME", skillName);
                command.ExecuteNonQuery();
                _connection.Close();
                return;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return;
            }
        }

        public List<User> GetAllUsersInProject(int projectId)
        {
            List<User> allUsers = new List<User>();
            if (projectId == -1)
            {
                User dummy = new User();
                dummy.Id = -1;
                allUsers.Add(dummy);
                return allUsers;
            }
            try
            {
                _connection.Open();
                var queryString = "SELECT U.[Id], U.[Name] FROM [Users] U JOIN [ProjectPerson] PP ON PP.[UserId] = U.[Id] WHERE PP.[ProjectId] = @PROJECTID"; //Left join to allow retrieval of users with null ManagerUserId 
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@PROJECTID", projectId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        User currentUser = new User();
                        currentUser.Id = dbReader.GetInt32(0);
                        currentUser.Name = dbReader.GetString(1);
                        allUsers.Add(currentUser);
                    }
                    _connection.Close();
                    return allUsers;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public List<User> GetAllUsersNotInProject(int projectId, int userId, int institutionId)
        {
            List<int> usersInProjectId = GetAllUsersInProject(projectId).Select(o => o.Id).ToList(); //take into consideration null lists
            List<User> allUsers = new List<User>();
            try
            {
                _connection.Open();
                var queryString = "SELECT U.[Id], U.[Name] FROM [Users] U WHERE InstitutionId = @INSTITUTIONID";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@INSTITUTIONID", institutionId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        User currentUser = new User();
                        currentUser.Id = dbReader.GetInt32(0);
                        if (usersInProjectId.Contains(currentUser.Id))
                        {
                            continue;
                        }
                        currentUser.Name = dbReader.GetString(1);
                        allUsers.Add(currentUser);
                    }
                    _connection.Close();
                    return allUsers;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public bool AddUserToProject(int userId, int projectId)
        {
            try
            {
                _connection.Open();

                var queryString = "INSERT INTO [ProjectPerson] VALUES (@PROJECTID, @USERID)";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@PROJECTID", projectId);
                command.ExecuteNonQuery();

                _connection.Close();

                return true;

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        public bool RemoveUserFromProject(int userId, int projectId)
        {
            try
            {
                _connection.Open();

                var queryString = "DELETE FROM [ProjectPerson] WHERE UserId = @USERID AND ProjectId = @PROJECTID";
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                command.Parameters.AddWithValue("@PROJECTID", projectId);
                command.ExecuteNonQuery();

                _connection.Close();

                return true;

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        public bool UpdateProjectMethod(int projectId, string projectName, string projectDescription, int projectDifficulty, int projectManager)
        {
            try
            {
                _connection.Open();

                var queryString = "UPDATE [Project] SET Name = @NAME, Description = @DESCRIPTION, Difficulty = @DIFFICULTY, ManagerUserId = @MANAGERUSERID " +
                                  "WHERE ProjectId = @PROJECTID;";

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@NAME", projectName);
                command.Parameters.AddWithValue("@DESCRIPTION", projectDescription);
                command.Parameters.AddWithValue("@DIFFICULTY", projectDifficulty);
                command.Parameters.AddWithValue("@MANAGERUSERID", projectManager);
                command.Parameters.AddWithValue("@PROJECTID", projectId);
                command.ExecuteNonQuery();

                _connection.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public bool DeleteProject(int projectId) //will have to update all tables so that instead of deleting you set Active to 0
        {
            try
            {
                _connection.Open();

                var queryString = "DELETE FROM [ProjectPerson] WHERE ProjectId = @PROJECTID;" +
                                  "DELETE FROM [ProjectTask] WHERE ProjectId = @PROJECTID;" +
                                  "DELETE FROM [EmployeeComment] WHERE ProjectId = @PROJECTID;" +
                                  "DELETE FROM [Project] WHERE ProjectId = @PROJECTID";

                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@PROJECTID", projectId);
                command.ExecuteNonQuery();

                _connection.Close();
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        public IOrderedEnumerable<KeyValuePair<string, float>> GetEmpCommentAnalysis(int userId)
        {
            Console.WriteLine(userId);
            List<ModelInput> allComments = new List<ModelInput>();
            var aggregatedComments = "";
            try
            {
                _connection.Open();
                var queryString = "SELECT Comment FROM [EmployeeComment] WHERE UserId = @USERID";
                Console.WriteLine(userId);
                SqlCommand command = new SqlCommand(queryString, _connection);
                command.Parameters.AddWithValue("@USERID", userId);
                using (SqlDataReader dbReader = command.ExecuteReader())
                {
                    while (dbReader.Read())
                    {
                        Console.WriteLine("reading.");
                        if (dbReader.IsDBNull(0))
                        {
                            _connection.Close();
                            return null;
                        }
                        aggregatedComments += dbReader.GetString(0);
                        aggregatedComments += " ";
                    }
                    _connection.Close();
                }
                Console.WriteLine(aggregatedComments);


                ModelInput aggregatedInput = new ModelInput { Feedback = aggregatedComments };
                var mlContext = new MLContext();
                DataViewSchema predictionPipelineSchema;

                ITransformer trainedModel = mlContext.Model.Load("EmpSentimentModel/EmpSentimentModel.mlnet", out var _);

                PredictionEngine<ModelInput, ModelOutput> predictionEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(trainedModel);
                Console.WriteLine("aGGRRGATED INPUT: ");
                Console.WriteLine(aggregatedComments == "");
                if (aggregatedComments == "")
                {
                    return null;
                }

                ModelOutput prediction = predictionEngine.Predict(aggregatedInput);
                var allLabels = PredictAllLabels(aggregatedInput);
                foreach (var label in allLabels)
                {
                    Console.WriteLine(label);
                }
                return allLabels; 
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }


    }
}

