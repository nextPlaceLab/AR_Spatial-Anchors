using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using Npgsql.Logging;
using System.Collections.Generic;
using System.Linq;
using NetworkCommon;
using extensionsClass;

/// <summary>
/// Summary description for DBCon
/// Class for connecting to database
/// </summary>
/// 


namespace DBCon
{
    public class DataBaseConnector
    {
        private NpgsqlConnection conn = null;
        DataManager dataManager;
        private readonly Timer mTimer;

        public DataBaseConnector(DataManager datamanager)
        {
            this.dataManager = datamanager;
            int intervalInSeconds = 1800;
            mTimer = new Timer((a) => PeriodicSave(a), true, intervalInSeconds * 1000, intervalInSeconds * 1000);
        }

        public void ConnectToDB()
        {
            Console.WriteLine("[{0}] Connecting to Database...", DateTime.Now.ToString());

            // Set database credentials
            var connString = "Host=db_host;Username=db_user;Password=db_password;Database=db_name";

            conn = new NpgsqlConnection(connString);
            conn.Open();

            Console.WriteLine("[{0}] Connecting to Database...finished", DateTime.Now.ToString());
        }

        private void PeriodicSave(object a)
        {
            Thread ctThread = new Thread(Save2DB);
            ctThread.Start();
        }

        private void Save2DB()
        {
            Console.WriteLine("[{0}] Saving data to database...", DateTime.Now.ToString());

            Dictionary<string, HashSet<string>> contexts = dataManager.GetContextAnchors();

            foreach (KeyValuePair<string, HashSet<string>> kvp in contexts)
            {
                string context = kvp.Key;
                HashSet<string> anchors = kvp.Value;
                HashSet<string> dbanchors = new HashSet<string>(GetAnchorsByContext(context));

                foreach (string anchor in anchors)
                {
                    if (!dbanchors.Contains(anchor))
                    {
                        InsertAnchor(context,anchor);
                    }
                }
                foreach (string anchor in dbanchors)
                {
                    if (!anchors.Contains(anchor))
                    {
                        DeleteAnchor(context, anchor);
                    }
                }
            }

            ModelData[] models = dataManager.GetModels("", "", 66);
            HashSet<long> dbids = GetModelsColumnValues("arid");
            HashSet<long> ids = new HashSet<long>();

            InsertModels(models);

            foreach (ModelData model in models)
                ids.Add(model.Id.GetHashCode());

            foreach (long dbid in dbids)
                if (!ids.Contains(dbid))
                    DeleteModel(dbid);

            Console.WriteLine("[{0}] Saving data to database...finished.", DateTime.Now.ToString());
        }

        public Dictionary<string, HashSet<string>> GetContextAnchors()
        {
            Dictionary<string, HashSet<string>> anchors = new Dictionary<string, HashSet<string>>();

            using (var cmd = new NpgsqlCommand("SELECT * FROM contextanchors", conn))
            {
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string context = reader[1].ToString();
                    string anchorid = reader[0].ToString();

                    if (!anchors.ContainsKey(context))
                        anchors.Add(context,new HashSet<string>());

                    anchors[context].Add(anchorid);
                }
            }
            return anchors;
        }

        public string[] GetAnchorsByContext(string context, string columnnames = "*")
        {
            HashSet<string> dbAnchors = new HashSet<string>();

            using (var cmd = new NpgsqlCommand($"SELECT {columnnames} FROM contextanchors WHERE context=@c", conn))
            {
                cmd.Parameters.AddWithValue("c", context);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    dbAnchors.Add(reader[0].ToString());
            }
            return dbAnchors.ToArray();
        }

        private bool InsertAnchor(string context, string id)
        {
            try
            {
                HashSet<string> ids = new HashSet<string>(GetAnchorsByContext(context));

                if (!ids.Contains(id))
                {
                    Insert("copy contextanchors from STDIN (FORMAT BINARY)", new object[] { id, context });

                    ids.Add(id);
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        private bool DeleteAnchor(string context, string id)
        {
            try
            {
                HashSet<string> ids = new HashSet<string>(GetAnchorsByContext(context));

                if (ids.Contains(id))
                {
                    Delete("contextAnchors", $"anchorid='{id}' AND context='{context}'");

                    ids.Remove(id);
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        private HashSet<long> GetModelsColumnValues(string columnName = "*")
        {
            HashSet<long> colValues = new HashSet<long>();

            using (var cmd = new NpgsqlCommand($"SELECT {columnName} FROM modeldata", conn))
            {
                using var reader = cmd.ExecuteReader();
                //cmd.Parameters.AddWithValue("col", columnName);
                while (reader.Read())
                    colValues.Add(long.Parse(reader[0].ToString()));
            }

            return colValues;
        }

        public ModelData[] GetModels()
        {
            List<ModelData> models = new List<ModelData>();

            try
            {
                using (var cmd = new NpgsqlCommand("SELECT * FROM modeldata", conn))
                {
                    //cmd.Parameters.AddWithValue("c", context);
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        models.Add(new ModelData(reader[8].ToString(), reader[7].ToString()
                            , new SPose(
                                Convert.ToSingle(((double[])reader[2])[0]),
                                Convert.ToSingle(((double[])reader[2])[1]),
                                Convert.ToSingle(((double[])reader[2])[2]),
                                Convert.ToSingle(((double[])reader[3])[0]),
                                Convert.ToSingle(((double[])reader[3])[1]),
                                Convert.ToSingle(((double[])reader[3])[2]),
                                Convert.ToSingle(((double[])reader[3])[3]))
                            , new SVector3(
                                Convert.ToSingle(((double[])reader[9])[0]),
                                Convert.ToSingle(((double[])reader[9])[1]),
                                Convert.ToSingle(((double[])reader[9])[2]))
                            , (ModelData.ModelType)reader[1], reader[4].ToString(), (byte[])reader[5], reader[6].ToString()));
                    }
                }
            }
            catch (Npgsql.PostgresException pe)
            {
                Console.WriteLine($"Exception: {pe}");
            }
            return models.ToArray();
        }

        private bool[] InsertModels(ModelData[] models)
        {
            List<bool> res = new List<bool>();

            HashSet<long> dbids = GetModelsColumnValues("arid");
            HashSet<long> ids = new HashSet<long>();

            foreach (ModelData model in models)
            {
                ids.Add(model.Id.GetHashCode());
                try
                {
                    if (!dbids.Contains(model.Id.GetHashCode()))
                    {
                        object[] paramObj = model.ToObjectArray();
                        Insert("copy modeldata from STDIN (FORMAT BINARY)", paramObj);
                    }
                    else
                    {
                        UpdateModelData(model);
                    }
                    res.Add(true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    res.Add(false);
                }
            }

            foreach (long dbid in dbids)
            {
                if (!ids.Contains(dbid))
                {
                    DeleteModel(dbid);
                }
            }

            return res.ToArray();
        }

 
        private bool DeleteModel(long id)
        {
            try
            {
                Delete("modeldata", $"arid='{id}'");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        private void UpdateModelData(ModelData model)
        {
            using var cmd = new NpgsqlCommand($"UPDATE modeldata SET position=@pos,rotation=@rot,message=@msg,data=@dat WHERE arid=@id;", conn);
            cmd.Parameters.AddWithValue("pos", model.Pose.Position.ToArray());
            cmd.Parameters.AddWithValue("rot", model.Pose.Rotation.ToArray());
            cmd.Parameters.AddWithValue("msg", model.Message);
            cmd.Parameters.AddWithValue("dat", model.Data);
            cmd.Parameters.AddWithValue("id", model.Id.GetHashCode());
            cmd.ExecuteNonQuery();
        }

        private void Insert(string query, object[] parameter)
        {
            using var writer = conn.BeginBinaryImport(query);
            writer.WriteRow(parameter);
            writer.Complete();
        }

        private void Delete(string tablename, string condition)
        {
            Console.WriteLine("Delete: {condition}");
            using var cmd = new NpgsqlCommand($"DELETE FROM {tablename} WHERE {condition};", conn);
            //cmd.Parameters.Add("condition", condition);
            cmd.ExecuteNonQuery();
        }
    }
}
