using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace AdlistCollecting
{
    internal class Program
    {
        private static readonly List<string> _whitelist = new List<string>();
        private static List<string> _domains = new List<string>();
        private static readonly List<string> _adlist = new List<string>();

        public static void Main(string[] args)
        {
            Console.Title = "Adlist Collecting";
            LoadingFile("adlist.txt", _adlist, "Adlist");
            LoadingFile("whitelist.txt", _whitelist, "Whitelist");
            Console.WriteLine("\nProgram Starting....\n");
            _adlist.ForEach(Processing);
            var lines = _domains.Count;
            _domains = _domains.Distinct().ToList();
            _domains.Sort();
            Console.WriteLine(
                $"\n[+] Added {_domains.Count} unique domains! {lines - _domains.Count} duplicated domains!");
            if (File.Exists("output.txt"))
                File.Delete("output.txt");
            // died brain code gg
            File.WriteAllText("output.txt",
                $"# ================================================{Environment.NewLine}# {_domains.Count} unique domains! {lines - _domains.Count} duplicated domains!{Environment.NewLine}# Adlist: {_adlist.Count} list{Environment.NewLine}# Whitelist: {_whitelist.Count} domains{Environment.NewLine}# Last update: {DateTime.Now:yyyy-MM-dd HH:mm:ss \"GMT\"zzz}{Environment.NewLine}# ================================================{Environment.NewLine}{Environment.NewLine}{string.Join(Environment.NewLine, _domains)}");
            Console.WriteLine("\nDone!");
            Thread.Sleep(-1);
        }

        private static void LoadingFile(string path, List<string> list, string type)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"[-] Not found file: '{path}'");
                Thread.Sleep(-1);
            }

            list.AddRange(File.ReadAllLines(path).Where(s => !s.StartsWith("#") && !string.IsNullOrEmpty(s))
                .Distinct());
            Console.WriteLine($"[+] Loaded {type}: {list.Count} lines");
        }

        private static void Processing(string uri)
        {
            Console.WriteLine($"[+] Processing: {uri}");
            var request = (HttpWebRequest) WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36 Edg/87.0.664.66";
            using var response = (HttpWebResponse) request.GetResponse();
            using var stream = response.GetResponseStream();
            if (stream == null) return;
            using var reader = new StreamReader(stream);
            string str;
            while ((str = reader.ReadLine()) != null)
            {
                // ignore comment & ipv6
                if (str.StartsWith("#") || str.Contains(":")) continue;
                str = Regex.Replace(str, "\\d+\\.\\d+\\.\\d+\\.\\d+", ""); //remove all ip address
                str = string.Concat(str.Where(c => !char.IsWhiteSpace(c))); //remove all white space
                if (_whitelist.Any(domain => string.Equals(str, domain, StringComparison.OrdinalIgnoreCase))) continue;
                if (string.IsNullOrEmpty(str)) continue;
                _domains.Add($"127.0.0.1 {str}");
            }
        }
    }
}