using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terminal.Color;
using Terminal.String;

namespace Terminal.DataBase
{
    public enum Type
    {
        String, Int, Bool
    }

    public enum Lock
    {
        True, False
    }

    public enum Option
    {
        Create, Use
    }
   
    public class DataBase
    {
        public static string Path { get; set; }
        private static bool CREATE = false;

        public DataBase(Option option)
        {
            if (Path == null) throw new ArgumentNullException("Path");

            if (File.Exists(Path))
            {
                if (option == Option.Use)
                {
                    CREATE = false;
                }
            }
            else if (!File.Exists(Path) && option != Option.Create)
            {
                throw new FileNotFoundException();
            }
            else if (option == Option.Create)
            {
                using (File.Create(Path)) { }
                CREATE = true;
            }
        }
        public void ReadBase()
        {
            string[] text = File.ReadAllLines(Path);
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i].Contains("DataGroup ::"))
                {
                    string s = text[i];
                    string name = null;
                    int space = 0;
                    for (int j = 0; j < s.Length; j++)
                    {
                        if (s[j] == ' ')
                        {
                            space++;
                        }
                        if (s[j] == '[') break;
                        if (space == 2)
                        {
                            name += s[j];
                        }
                    }
                    s = s.Replace(name, name.MakeColored("78,201,176"));
                    s = s.Replace("DataGroup", "DataGroup".MakeColored("19,131,248"));
                    s = s.Replace("::", "::".MakeColored(Color.Color.SlateGray));
                    s = s.Replace("out", "out".MakeColored("19,131,248"));
                    s = s.Replace("true", "true".MakeColored("19,131,248"));
                    s = s.Replace("false", "false".MakeColored("19,131,248"));
                    s = s.Replace("=>", "=>".MakeColored(Color.Color.SlateGray));
                    s = s.Replace("lock", "lock".MakeColored("132, 220, 254"));
                    s = s.Replace("String", "String".MakeColored("220, 219, 153"));
                    s = s.Replace("Bool", "Bool".MakeColored("220, 219, 153"));
                    s = s.Replace("Int", "Int".MakeColored("220, 219, 153"));
                    text[i] = s;
                }
                else if (text[i].StartsWith("++"))
                {
                    text[i] = text[i].Replace(text[i], text[i].MakeColored("87, 166, 74"));
                }
                else
                {
                    text[i] = text[i].Replace("'", "'".MakeColored(Color.Color.SlateGray));
                    text[i] = text[i].Replace(":", ":".MakeColored(Color.Color.SlateGray));
                }
            }
            text.WriteLine();
        }
        public void AddComment(string comment)
        {
            try
            {
                string[] content = File.ReadAllLines(Path);
                using (StreamWriter writer = new StreamWriter(Path))
                {
                    writer.WriteLine("++ " + comment);
                    foreach (var item in content)
                    {
                        writer.WriteLine(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding comment: {ex.Message}");
            }
        }

        public void AddRow(string dataGroupName)
        {
            try
            {
                string[] content = File.ReadAllLines(Path);
                int start = Array.FindIndex(content, line => line.Contains($"DataGroup :: {dataGroupName}"));
                int end = Array.FindIndex(content, start, line => line.Trim() == "};");
                List<string> rows = new List<string>();

                for (int i = start + 1; i < end; i++)
                {
                    if (!string.IsNullOrWhiteSpace(content[i]))
                    {
                        if (content[i] == "{") rows.Add(content[i].Trim());
                        else rows.Add("\t" + content[i].Trim());

                    }
                }

                int rowCount = rows.Count;
                rows.Add($"\tR{rowCount} : {{}}");

                content = content.Take(start + 1)
                                 .Concat(rows)
                                 .Concat(content.Skip(end))
                                 .ToArray();

                File.WriteAllLines(Path, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding row: {ex.Message}");
            }
        }
        public void SetData(string dataGroupName, int rowIndex, params string[] data)
        {
            try
            {
                string[] content = File.ReadAllLines(Path);
                int start = Array.FindIndex(content, line => line.Contains(dataGroupName));
                int end = Array.FindIndex(content, start, line => line.Trim() == "};");

                string formattedData = string.Join(" ' ", data);

                for (int i = start + 1; i < end; i++)
                {
                    if (content[i].Trim().StartsWith($"R{rowIndex}"))
                    {
                        content[i] = $"\tR{rowIndex} : {{{formattedData}}}";
                        break;
                    }
                }

                File.WriteAllLines(Path, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding data to row: {ex.Message}");
            }
        }
        public void AddData(string dataGroupName, int rowIndex, params string[] data)
        {
            try
            {
                string ss = GetDataFromRow(dataGroupName,rowIndex);
                if(!System.String.IsNullOrWhiteSpace(ss))
                {
                    ss += " ' ";
                }
                ss += string.Join(" ' ",data);
                SetData(dataGroupName,rowIndex,ss);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding data to row: {ex.Message}");
            }
        }
        public string GetDataFromRowFromIndex(string groupName,int rowIndex, int dataIndex)
        {
            try
            {
                string[] data = GetDataFromRow(groupName, rowIndex).Split('\'');
                return data[dataIndex];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting data from row: {ex.Message}");
            }
            return null;
        }
        public void SetLockToGroup(string groupName, Lock type)
        {
            string[] content = File.ReadAllLines(Path);
            int index = Array.FindIndex(content, line => line.Contains($"DataGroup :: {groupName}"));
            if(type == Lock.True && content[index].Contains("false"))
            {
                content[index] = content[index].Replace("false","true");
            }else if(type == Lock.False && content[index].Contains("true")) content[index] = content[index].Replace("true", "false");
            File.WriteAllLines(Path, content);
        }
        public string GetDataFromRow(string groupName, int rowIndex)
        {
            try
            {
                string[] content = File.ReadAllLines(Path);
                int start = Array.FindIndex(content, line => line.Contains($"DataGroup :: {groupName}"));
                int end = Array.FindIndex(content, start, line => line.Trim() == "};");
                for (int i = start + 1; i < end; i++)
                {
                    if (content[i].Trim().StartsWith($"R{rowIndex}"))
                    {
                        string rowData = content[i].Trim();
                        int startIndex = rowData.IndexOf("{") + 1;
                        int endIndex = rowData.LastIndexOf("}");
                        string data = rowData.Substring(startIndex, endIndex - startIndex);
                        return data;
                    }
                }
                Console.WriteLine($"Row with index R{rowIndex} not found in group {groupName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting data from row: {ex.Message}");
            }
            return null;
        }
        public void ReplaceDataAtIndex(string groupName, int rowIndex, int dataIndex, string newData)
        {
            try
            {
                string[] content = File.ReadAllLines(Path);
                int start = Array.FindIndex(content, line => line.Contains($"DataGroup :: {groupName}"));
                int end = Array.FindIndex(content, start, line => line.Trim() == "};");
                for (int i = start + 1; i < end; i++)
                {
                    if (content[i].Trim().StartsWith($"R{rowIndex}"))
                    {
                        string rowData = content[i].Trim();
                        int startIndex = rowData.IndexOf("{") + 1;
                        int endIndex = rowData.LastIndexOf("}");
                        string[] dataArray = rowData.Substring(startIndex, endIndex - startIndex).Split(new[] { " '", "' " }, StringSplitOptions.RemoveEmptyEntries);
                        if (dataIndex >= 0 && dataIndex < dataArray.Length)
                        {
                            string trimmedNewData = newData.Trim(); 
                            string updatedData = $" {trimmedNewData}";
                            dataArray[dataIndex] = updatedData;
                            string newDataString = string.Join(" '", dataArray);
                            content[i] = $"\tR{rowIndex} : {{{newDataString}}}";
                            File.WriteAllLines(Path, content);
                            return;
                        }
                        else
                        {
                            Console.WriteLine($"Data index {dataIndex} is out of range for row R{rowIndex} in group {groupName}. Valid range is 0 to {dataArray.Length - 1}.");
                            return;
                        }
                    }
                }
                Console.WriteLine($"Row with index R{rowIndex} not found in group {groupName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error replacing data at index {dataIndex} in row R{rowIndex}: {ex.Message}");
            }
        }
        public void RemoveDataAtIndex(string groupName, int rowIndex, int dataIndex)
        {
            try
            {
                string[] content = File.ReadAllLines(Path);
                int start = Array.FindIndex(content, line => line.Contains($"DataGroup :: {groupName}"));
                int end = Array.FindIndex(content, start, line => line.Trim() == "};");

                for (int i = start + 1; i < end; i++)
                {
                    if (content[i].Trim().StartsWith($"R{rowIndex}"))
                    {
                        string rowData = content[i].Trim();
                        int startIndex = rowData.IndexOf("{") + 1;
                        int endIndex = rowData.LastIndexOf("}");
                        string[] dataArray = rowData.Substring(startIndex, endIndex - startIndex).Split(new[] { " '", "' " }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
                        if (dataIndex >= 0 && dataIndex < dataArray.Length)
                        {
                            List<string> updatedDataList = new List<string>(dataArray);
                            updatedDataList.RemoveAt(dataIndex);
                            string newDataString = string.Join(" ' ", updatedDataList);
                            content[i] = $"\tR{rowIndex} : {{{newDataString}}}";
                            File.WriteAllLines(Path, content);
                            return;
                        }
                        else
                        {
                            Console.WriteLine($"Data index {dataIndex} is out of range for row R{rowIndex} in group {groupName}. Valid range is 0 to {dataArray.Length - 1}.");
                            return;
                        }
                    }
                }

                Console.WriteLine($"Row with index R{rowIndex} not found in group {groupName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing data at index {dataIndex} in row R{rowIndex}: {ex.Message}");
            }
        }
        public void CreateDataGroup(string groupName, Lock value, Type[] type)
        {
            if (CREATE)
            {
                try
                {
                    string comment = "++ \"Created by Terminal.DataBase.dll\" " + DateTime.Now + "\n";
                    string types = string.Join(",", type.Select(t => t.ToString()));
                    string locks = value == Lock.False ? "lock:false" : "lock:true";
                    string dataGroup = $"DataGroup :: {groupName}[{locks}] out => {types}\n" + "{\n\n};\n";
                    using (StreamWriter writer = new StreamWriter(Path, true))
                    {
                        writer.WriteLine(comment);
                        writer.WriteLine(dataGroup);
                    }
                    CREATE = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating data group: {ex.Message}");
                }
            }
            else
            {
                string types = string.Join(",", type.Select(t => t.ToString()));
                string locks = value == Lock.False ? "lock:false" : "lock:true";
                string dataGroup = $"DataGroup :: {groupName}[{locks}] out => {types}\n" + "{\n\n};\n";
                using (StreamWriter writer = new StreamWriter(Path, true))
                {
                    writer.WriteLine(dataGroup);
                }
            }
        }
    }
}
