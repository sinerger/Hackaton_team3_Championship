﻿using System;
using System.Data.SqlClient;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Hackaton_team3
{
    public class Core
    {
        private static Core _core;
        private string _connectionPath = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=F:\C#\Hackaton_team3\Hackaton_team3\TournamtsDb.mdf;Integrated Security=True";
        private SqlConnection _sqlConnection;
        private LoggingLevelSwitch _loggerSwitch;
        public Logger DbLogger { get; private set; }

        public static Core GetCore()
        {
            if (_core == null)
            {
                _core = new Core();
            }

            return _core;
        }

        public bool ConnectDataBase()
        {
            if (_connectionPath != null)
            {
                bool result = true;
                InitDbLogger();
                _sqlConnection = new SqlConnection(_connectionPath);

                try
                {
                    _sqlConnection.Open();
                    DbLogger.Information("Connect to database");
                }
                catch (Exception e)
                {
                    result = false;
                    DbLogger.Error(e.Message);
                }

                return result;
            }

            throw new ArgumentNullException("String connection path is null");
        }

        public string[] GetParticipantFromDb(string matchId)
        {
            if (matchId != null)
            {
                string[] result = new string[2];

                try
                {
                    string query = $"select * from [dbo].[Participants] where id = (select [ParticipantOne] from [dbo].[Matches] where [dbo].[Matches].[ID]={matchId}) ";
                    SqlCommand command = new SqlCommand(query, _sqlConnection);
                    SqlDataReader reader = command.ExecuteReader();
                    
                        if (reader.HasRows)
                        {
                            reader.Read();
                            result[0] = $"{reader[1]},{reader[2]}";
                        }
                    
                    DbLogger.Information($"Query is done : {query}");
                }
                catch (Exception e)
                {
                    result[0] = string.Empty;
                    DbLogger.Error(e.ToString());
                }

                try
                {
                    string query = $"select * from [dbo].[Participants] where id = (select [ParticipantTwo] from [dbo].[Matches] where [dbo].[Matches].[ID]={matchId})";
                    SqlCommand command = new SqlCommand(query, _sqlConnection);
                    SqlDataReader reader = command.ExecuteReader();
                    
                        if (reader.HasRows)
                        {
                            reader.Read();
                            result[1] = $"{reader[1]},{reader[2]}";
                        }
                    
                    DbLogger.Information($"Query is done : {query}");
                }
                catch (Exception e)
                {
                    result[1] = string.Empty;
                    DbLogger.Error(e.ToString());
                }

                return result;
            }

            throw new ArgumentNullException("Value is null");
        }

        public bool InsertParticipantInToDb(string value)
        {
            if (value != null)
            {
                bool result = true;
                try
                {
                    string query = $"insert dbo.[Participants] values({value})";
                    SqlCommand command = new SqlCommand(query, _sqlConnection);
                    int i = command.ExecuteNonQuery();
                    DbLogger.Information($"{i}Query is done : {query}");
                }
                catch (Exception e)
                {
                    result = false;
                    DbLogger.Error(e.ToString());
                }

                return result;
            }

            throw new ArgumentNullException("Value is null");
        }

        private void InitDbLogger()
        {
            _loggerSwitch = new LoggingLevelSwitch();
            _loggerSwitch.MinimumLevel = LogEventLevel.Debug;

            DbLogger = new LoggerConfiguration()
                .WriteTo.File($"DbLog_{DateTime.Now.ToString("dd_MM__hh_mm_ss")}.txt")
                .MinimumLevel.ControlledBy(_loggerSwitch)
                .CreateLogger();
        }
    }
}
