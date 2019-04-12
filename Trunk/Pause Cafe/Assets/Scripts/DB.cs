using Mono.Data.Sqlite;
using System.Data;
using System.Collections;
using System;
using System.IO;
using UnityEngine;
using Classifiers;

public class DB
{
    /// <summary>
    /// get connection to the Rule database.
    /// </summary>
    /// <returns>IDbConnection object that contain the databese instance </returns>
    public static IDbConnection connectBDD()
    {

        //Connection à la base de données
        //string conn = "URI=file:" + Application.dataPath + "/Classifiers.db"; //Path to database
		//string conn = "URI=file:" + "Data/Classifiers/Classifiers.db"; //Path to database
		string conn = "URI=file:" + Application.dataPath + "/../Data/Classifiers/Classifiers.db"; //Path to database
		
        IDbConnection dbconn;
        dbconn = (IDbConnection)new SqliteConnection(conn);
        dbconn.Open(); //Open connection to the database.
        return dbconn;
    }

    public static void addRule(Classifier c)
    {
		if (c.id == 0)
        {
			IDbConnection dbconn = connectBDD();
			IDbCommand dbCommand = dbconn.CreateCommand();
			dbCommand.CommandText = "INSERT INTO Rules (SITUATION, ACTION, FITNESS) values ('" + c.RuleToString() + "','" + c.action.ToString() + "'," + c.fitness + ")";
			dbCommand.ExecuteNonQuery();
			dbCommand.Dispose();
			c.modified = false;
			dbconn.Close();
		}
    }

    public static void addRule(Classifier c, float tempFitness)
    {
        if (c.id == 0)
        {
            IDbConnection dbconn = connectBDD();
            IDbCommand dbCommand = dbconn.CreateCommand();
            dbCommand.CommandText = "INSERT INTO Rules (SITUATION, ACTION) values ('" + c.RuleToString() + "','" + c.action.ToString() + "'," + tempFitness + ")";
            dbCommand.ExecuteNonQuery();
            dbCommand.Dispose();
            c.modified = false;
            dbconn.Close();
        }
    }




    public static void UpdateRuleFitness(Classifier c)
    {
        if (c.id > 0)
        {
            IDbConnection dbconn = connectBDD();
            IDbCommand dbCommand = dbconn.CreateCommand();
            dbCommand.CommandText = "UPDATE Rules SET FITNESS = " + c.fitness + " WHERE ID='" + c.id + "';";
            dbCommand.ExecuteNonQuery();
            dbCommand.Dispose();
            dbconn.Close();
            c.modified = false;
        }
    }



    public static void UpdateRuleAction(Classifier c, Action action)
    {
        IDbConnection dbconn = connectBDD();
        IDbCommand dbCommand = dbconn.CreateCommand();
        dbCommand.CommandText = "UPDATE Rules SET ACTION = " + action + " WHERE SITUATION='" + c.RuleToString() + "';";
        dbCommand.ExecuteNonQuery();
        dbCommand.Dispose();
        dbconn.Close();
    }

}