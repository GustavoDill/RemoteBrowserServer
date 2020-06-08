using CSharpExtendedCommands.Data;
using CSharpExtendedCommands.Data.SimpleJSON;
using CSharpExtendedCommands.UI;
using CSharpExtendedCommands.Web.Communication;
using EverythingApi;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace RemoteBrowserServer
{
    public static class Commands
    {
        public static void Log(string msg)
        {
            Log(msg, console.DefaultLogColor);
        }
        public static void Log(string msg, Color color, bool doNewLine = true)
        {
            try
            {
                console.Log("[" + DateTime.Now.Day.ToString().PadLeft(2, '0') + "/" + DateTime.Now.Day.ToString().PadLeft(2, '0') + "/" + DateTime.Now.Day.ToString().PadLeft(2, '0') + " - " + DateTime.Now.Hour.ToString().PadLeft(2, '0') + ":" + DateTime.Now.Minute.ToString().PadLeft(2, '0') + ":" + DateTime.Now.Second.ToString().PadLeft(2, '0') + "] " + msg, color, doNewLine);
            }
            catch { }
        }
        public static ConsoleTextBox console;
        public static void CLOSECONNECTION(TCPClient client)
        {
            Log($"Client has ended the connection {{Host: {client.Ip} Port: {client.Port}}}");
        }
        private static string GetJson(string name, string path, Type type)
        {
            return $"{{\"Name\" : \"{name}\", \"FullPath\" : \"{path}\", \"Type\" : \"{type.Name}\"}}";
        }
        public static void SEARCHARCHIVE(TCPClient requester, string request)
        {
            if (!File.Exists("Everything32.dll"))
                Resource.Export("RemoteBrowserServer.SearchEngine.Everything32.dll", "Everything32.dll");
            Everything.RequestFlags = EverythingFlags.RequestFlags.REQUEST_FULL_PATH_AND_FILE_NAME | EverythingFlags.RequestFlags.REQUEST_FILE_NAME;
            Everything.Search = request;
            Everything.QueryW(true);
            var tot = Everything.GetNumResults();
            JSONNode json = JSON.Parse($"{{\"Items\" : []}}");
            for (uint i = 0; i < tot; i++)
            {
                StringBuilder fpath = null;
                try
                {
                    fpath = new StringBuilder(Everything.MaxPathSize);
                    Everything.GetResultFullPathName(i, fpath, (uint)Everything.MaxPathSize);
                }
                catch { }
                Type type;
                if (Directory.Exists(fpath.ToString()))
                    type = typeof(DirectoryInfo);
                else
                    type = typeof(FileInfo);
                json["Items"].Add(JSON.Parse(GetJson(Marshal.PtrToStringUni(Everything.GetResultFileName(i)), fpath.ToString().Replace("\\", ";"), type)));
            }
            requester.SendPackage(json.ToString());
            Log($"Sent package to client {{Host: {requester.Ip} Port: {requester.Port}}} - Pacakge of System.byte[{json.ToString().Length}]", Color.Cyan);
        }
        public static void LISTDIRECTORY(TCPClient requester, string dir)
        {
            DirectoryInfo dirinfo;
            if (dir.Length != 2)
                dirinfo = new DirectoryInfo(dir);
            else
                dirinfo = new DirectoryInfo(dir + "\\");
            FileInfo[] files = null;
            DirectoryInfo[] dirs = null;
            List<string> dnames = new List<string>();
            try
            {
                files = dirinfo.GetFiles();
                dirs = dirinfo.GetDirectories();
            }
            catch
            {
                Log($"Access to directory '" + dir + "' was denied!", Color.Red);
                var pkg = new TcpPackage("ACCESS DENIED");
                Log($"Begin send command response to client {{Host: {requester.Ip} Port: {requester.Port}}} - Length: {pkg.Size} byte(s)", Color.FromArgb(0x0072ff));
                requester.SendPackage(pkg);
                Log($"Sent package to client {{Host: {requester.Ip} Port: {requester.Port}}} - Pacakge of System.byte[{pkg.Size}]", Color.Cyan);
                console.Log("");
                return;
            }
            string data = "Files:";
            if (new DirectoryInfo(dir).Parent != null && dir.Length != 2)
                dnames.Add("..");
            foreach (var d in dirs)
                if (!Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(d.Name)).Contains("?"))
                    dnames.Add(d.Name);
            for (int i = 0; i < files.Length; i++)
                data += "\n\t" + files[i].Name + ";";
            data += "\nDirectories:";
            for (int i = 0; i < dnames.Count; i++)
                data += "\n\t" + dnames[i] + ";";
            var package = new TcpPackage(data);
            Log($"Begin send command response to client {{Host: {requester.Ip} Port: {requester.Port}}} - Length: {package.Size} byte(s)", Color.FromArgb(0x0072ff));
            requester.SendPackage(package);
            Log($"Sent package to client {{Host: {requester.Ip} Port: {requester.Port}}} - Pacakge of System.byte[{package.Size}]", Color.Cyan);
            console.Log("");
        }
        public static void FILEDETAILS(TCPClient requester, string file)
        {
            if (File.Exists(file))
            {
                var i = new FileInfo(file);
                var json = $"{{" +
                    $"\"Type\" : \"File\"," +
                    $"\"Name\" : \"{i.Name}\"," +
                    $"\"Size\" : {i.Length}," +
                    $"\"CreationTime\" : \"{i.CreationTime}\"," +
                    $"\"Attributes\" : {(int)i.Attributes}," +
                    $"\"DirectoryName\" : \"{i.DirectoryName.Replace("\\", ";")}\"," +
                    $"\"Extension\" : \"{i.Extension}\"," +
                    $"\"LastWriteTime\" : \"{i.LastWriteTime}\"," +
                    $"\"LastAccessTime\" : \"{i.LastAccessTime}\"" +
                    $"}}";
                requester.SendPackage(json);
            }
            else if (Directory.Exists(file))
            {
                var i = new DirectoryInfo(file);
                var json = $"{{" +
                    $"\"Type\" : \"Directory\"," +
                    $"\"Name\" : \"{i.Name}\"," +
                    $"\"CreationTime\" : \"{i.CreationTime}\"," +
                    $"\"Attributes\" : {(int)i.Attributes}," +
                    $"\"DirectoryName\" : \"{i.Parent.FullName.Replace("\\", ";")}\"," +
                    $"\"Extension\" : \"{i.Extension}\"," +
                    $"\"LastWriteTime\" : \"{i.LastWriteTime}\"," +
                    $"\"LastAccessTime\" : \"{i.LastAccessTime}\"" +
                    $"}}";
                requester.SendPackage(json);
            }
            else
                requester.SendPackage("ERROR_FILE_INEXISTENT");
        }
        public static void GETFILESIZE(TCPClient requester, string file)
        {
            if (File.Exists(file))
            {
                var info = new FileInfo(file);
                var size = info.Length.ToString("X16");
                requester.SendPackage(size);
            }
            else
                requester.SendPackage("ERROR_FILE_INEXISTENT");
        }
        public static void RETRIEVEFILE(TCPClient requester, string file)
        {
            try
            {
                var reader = new BinaryReader(File.OpenRead(file));
                var buffer = reader.ReadBytes((int)reader.BaseStream.Length);
                reader.Close();
                Log($"Begin send file to client {{Host: {requester.Ip} Port: {requester.Port}}} - Length: {buffer.Length} byte(s)", Color.FromArgb(0x0072ff));
                requester.SendPackage(new TcpPackage(buffer));
                Log($"Sent package to client {{Host: {requester.Ip} Port: {requester.Port}}} - Package of System.byte[{buffer.Length}]", Color.Cyan);
                console.Log("");
            }
            catch
            {
                Log($"Access to file '" + file + "' was denied!", Color.Red);
                var pkg = new TcpPackage("ACCESS DENIED");
                Log($"Begin send command response to client {{Host: {requester.Ip} Port: {requester.Port}}} - Length: {pkg.Size} byte(s)", Color.FromArgb(0x0072ff));
                requester.SendPackage(pkg);
                Log($"Sent package to client {{Host: {requester.Ip} Port: {requester.Port}}} - Package of System.byte[{pkg.Size}]", Color.Cyan);
                console.Log("");
            }
        }
        public static void DIREXISTS(TCPClient requester, string dir)
        {
            var flag = Directory.Exists(dir);
            Log($"Begin sending command response to client {{Host: {requester.Ip} Port: {requester.Port}}} - Length: 1 byte(s)", Color.FromArgb(0x0072ff));
            if (flag)
                requester.SendPackage("1");
            else
                requester.SendPackage("0");
            Log($"Sent package to client {{Host: {requester.Ip} Port: {requester.Ip}}} package of System.bool[", Color.Green, false);
            Log($"{flag}", flag ? Color.Blue : Color.Red, false);
            Log("]", Color.Green);
            console.Log("");
        }
    }
}
