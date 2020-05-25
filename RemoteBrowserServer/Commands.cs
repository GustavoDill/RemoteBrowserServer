using CSharpExtendedCommands.Web.Communication;
using System.Collections.Generic;
using System.IO;

namespace RemoteBrowserServer
{
    public static class Commands
    {
        public static void LISTDIRECTORY(TCPClient requester, string dir)
        {
            var dirinfo = new DirectoryInfo(dir);
            FileInfo[] files = null;
            DirectoryInfo[] dirs = null;
            List<string> dnames = new List<string>();
            try
            {
                files = dirinfo.GetFiles();
                dirs = dirinfo.GetDirectories();
            }
            catch { requester.SendPackage(new TcpPackage("ACCESS DENIED")); }
            string data = "Files:";
            if (new DirectoryInfo(dir).Parent != null)
                dnames.Add("..");
            foreach (var d in dirs)
                dnames.Add(d.Name);
            for (int i = 0; i < files.Length; i++)
                data += "\n\t" + files[i].Name + ";";
            data += "\nDirectories:";
            for (int i = 0; i < dnames.Count; i++)
                data += "\n\t" + dnames[i] + ";";
            var package = new TcpPackage(data);
            requester.SendPackage(package);
        }
        public static void RETRIEVEFILE(TCPClient requester, string file)
        {
            requester.SendFile(file);
        }
        public static void DIREXISTS(TCPClient requester, string dir)
        {
            var flag = Directory.Exists(dir);
            if (flag)
                requester.SendPackage("1");
            else
                requester.SendPackage("0");
        }
    }
}
