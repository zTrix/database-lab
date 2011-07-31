using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using System.Data.SqlClient;

namespace WindowsFormsApplication1{
    static class Program{
        static int num = 0;
        static int height = 0;
        static String saveFile = "save.txt";
        static public String disk = "r:";
        static String dbName = "final";
        static String tableName = "Tree";
        static public int type = 1;
        static public String xml_name = "";
        static public Form1 mainForm;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(){
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            mainForm = new Form1();
            Application.Run(mainForm);
        }

        static StreamWriter streamwriter;
        
        public static void start(){
            if (xml_name.Equals(""))
            {
                return;
            }
            mainForm.clearLog();

            DateTime startTime = DateTime.Now;

            mainForm.printLog("Loading XML " + xml_name + " ...");
            XmlDocument xml_data = new XmlDocument();
            xml_data.Load(xml_name);
            mainForm.printResult(true);
            

            char c = 'c';
            while (!Directory.Exists(disk)){
                disk = c+":";
                c++;
            }
            streamwriter = new StreamWriter(disk+"\\"+saveFile, false);
            num = 0;
            mainForm.printLog("Preparing SQL Server insert data ... ");
            save(xml_data);
            mainForm.printResult(true);
            

            streamwriter.Flush();
            streamwriter.Dispose();

            mainForm.printLog("Connecting SQL Server, DataBase = "+dbName+" ... ");
            System.Data.SqlClient.SqlConnection mySqlconnection;
            try
            {
                mySqlconnection = new SqlConnection("integrated security = SSPI;" + "DataBase =" + dbName + ";" + "Server = (local)\\SQLExpress");
                mySqlconnection.Open();
            }catch(Exception e){
                mainForm.fail("cannot connect SQL Server or database "+dbName+" not existed");
                return;
            }
            mainForm.printResult(true);
            

            SqlCommand myCommand = new SqlCommand();
            myCommand.Connection = mySqlconnection;
            SqlDataReader myReader;

            StreamReader streamreader = new StreamReader(disk+"\\"+saveFile);

            mainForm.printLog("Creating Table "+tableName+" ...");
            myCommand.CommandText = "if object_id('dbo." + tableName + "') is not null drop table dbo." + tableName + ";create table " + tableName + "(name char(20),low int,high int,height int);delete from " + tableName + "";
            try
            {
                myReader = myCommand.ExecuteReader();
                myReader.Close();
            }catch(Exception e){
                mainForm.fail("cannot create table " + tableName);
                return;
            }
            mainForm.printResult(true);

            mainForm.printLog("Bulk insert data into DataBase ...");
            myCommand.CommandText = "bulk insert " + tableName + " from '" + disk + "\\\\" + saveFile + "' with ( FIELDTERMINATOR=',', ROWTERMINATOR=';')";
            try
            {
                myReader = myCommand.ExecuteReader();
                myReader.Close();
            }catch(Exception e){
                mainForm.fail("cannot insert data into " + dbName);
                return;
            }
            mainForm.printResult(true);

            mainForm.printLog("Collecting distinct names ...");
            ArrayList names = new ArrayList();
            myCommand.CommandText = "SELECT DISTINCT name from " + tableName + " where(name <> '#document')";
            myReader = myCommand.ExecuteReader();
            while (myReader.Read())
            {
                names.Add(myReader[0]);
            }
            mainForm.setList(names.ToArray());
            myReader.Close();
            streamreader.Close();
            mySqlconnection.Close();
            mainForm.printResult(true);

            DateTime stopTime = DateTime.Now;
            TimeSpan duration = stopTime - startTime;

            mainForm.printLog(Environment.NewLine + "Finished!\t\tExecution time: " + duration.ToString() + Environment.NewLine);
        }


        private static StringBuilder ans = new StringBuilder(0x200000);
        public static void search(string str1, string str2){
            ans.Clear();

            DateTime startTime = DateTime.Now;

            int ansLen = 0;

            mainForm.printLog("Connecting SQL Server, DataBase = " + dbName + " ... ");
            System.Data.SqlClient.SqlConnection mySqlconnection;
            mySqlconnection = new SqlConnection("integrated security = SSPI;" + "DataBase ="+dbName+";" + "Server = (local)\\SQLExpress");

            mySqlconnection.Open();
            mainForm.printResult(true);


            SqlCommand myCommand = new SqlCommand();
            myCommand.Connection = mySqlconnection;

            SqlDataReader myReader;

            if (type == 0)
            {
                
                myCommand.CommandText = "select * from " + tableName + " where (name = '" + str1 + "')";
                mainForm.printLog("Executing standard query ...");
                myReader = myCommand.ExecuteReader();
                mainForm.printResult(true);

                mainForm.printLog("Getting result ...");
                while (myReader.Read())
                {
                    ans.Append("( ");
                    for (int i = 0; i < 4; i++)
                    {
                        ans.Append(myReader[i]);
                        ans.Append(" ");
                    }
                    ans.Append(" )"+Environment.NewLine);
                    ansLen++;
                }
                myReader.Close();
                mainForm.printResult(true);
            }
            else if (type == 1)
            {
                mainForm.printLog("Executing father son query ...");
                string string1 = " SELECT * ";
                string string2 = " FROM (SELECT * FROM " + tableName + " WHERE (name = '" + str1 + "')) AS table1, ";
                string string3 = " (SELECT * FROM " + tableName + " WHERE (name = '" + str2 + "')) AS table2 ";
                string string4 = " WHERE (table1.low < table2.low) and (table1.high > table2.high) and (table1.height = table2.height-1)";
                myCommand.CommandText = string1 + string2 + string3 + string4;
                myReader = myCommand.ExecuteReader();
                mainForm.printResult(true);

                mainForm.printLog("Getting result ...");
                while (myReader.Read())
                {
                    ans.Append("( ");
                    for (int i = 0; i < 8; i++)
                    {
                        ans.Append(myReader[i]);
                        if (i == 3)
                        {
                            ans.Append(" ), ( ");
                            continue;
                        }
                        ans.Append(" ");
                    }
                    ans.Append(" )");
                    ans.Append(streamwriter.NewLine);
                    ansLen++;
                }
                myReader.Close();
                mainForm.printResult(true);
            }
            else
            {
                mainForm.printLog("Executing ancestor offspring query ...");
                string string1 = " SELECT * ";
                string string2 = " FROM (SELECT * FROM " + tableName + " WHERE (name = '" + str1 + "')) AS table1, ";
                string string3 = " (SELECT * FROM " + tableName + " WHERE (name = '" + str2 + "')) AS table2 ";
                string string4 = " WHERE (table1.low < table2.low) and (table1.high > table2.high) ";
                myCommand.CommandText = string1 + string2 + string3 + string4;

                myReader = myCommand.ExecuteReader();
                mainForm.printResult(true);

                mainForm.printLog("Getting result ...");
                while (myReader.Read())
                {
                    ans.Append("( ");
                    for (int i = 0; i < 8; i++)
                    {
                        ans.Append(myReader[i]);
                        if (i == 3)
                        {
                            ans.Append(" ), ( ");
                            continue;
                        }
                        ans.Append(" ");
                    }
                    ans.Append(" )"+Environment.NewLine);
                    ansLen++;
                }
                myReader.Close();
                mainForm.printResult(true);
            }
            ans.Append(Environment.NewLine+"Result length: "+ansLen+Environment.NewLine);

            DateTime stopTime = DateTime.Now;
            TimeSpan duration = stopTime - startTime;
            mainForm.printLog(ans.ToString());
            mainForm.printLog(Environment.NewLine + "Finished!\t\tExecution time: " + duration.ToString() + Environment.NewLine);
        }

        static void save(XmlNode node){
            if (node.Name.Equals("#text")){
                return;
            }
            if (node.FirstChild == null){
                // 输出区域下界
                streamwriter.Write(node.Name + ",");
                //streamwriter.Write(" low ");
                streamwriter.Write((num + 1) + ",");
                // 输出区域上界
                //streamwriter.Write(" high ");
                streamwriter.Write((num + 2) + ",");
                // 输出高度
                //streamwriter.Write(" height ");
                streamwriter.Write(height + ";");
                num += 2;
            }else{
                num++;
                int temp = num;
                foreach (XmlNode node1 in node.ChildNodes)
                {
                    height++;
                    save(node1);
                    height--;
                }
                num++;
                // 输出区域下界
                streamwriter.Write(node.Name + ",");
                //streamwriter.Write(" low ");
                streamwriter.Write(temp + ",");
                // 输出区域上界
                //streamwriter.Write(" high ");
                streamwriter.Write(num + ",");
                // 输出高度
                //streamwriter.Write(" height ");
                streamwriter.Write(height + ";");
            }
        }

    }
}
