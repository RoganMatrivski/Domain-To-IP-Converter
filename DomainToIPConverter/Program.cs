using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace DomainToIPConverter
{
    class Program
    {
        static HttpClient client = new HttpClient();

        static void Main(string[] args)
        {
            //string raws = await client.GetStringAsync(@"https://github.com/gvoze32/unblockhostid/raw/master/raw.txt");
            var task = client.GetStringAsync(@"https://github.com/gvoze32/unblockhostid/raw/master/raw.txt");
            task.Wait();
            string raws = task.Result;

            Dictionary<string, List<string>> group_collection = new Dictionary<string, List<string>>();

            List<string> current_host_group = new List<string>();
            string current_group_name = "";

            foreach (string line in raws.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
            {
                //var test = line.Split(' ').Skip(1).ToArray();
                var matches = Regex.Matches(line, @"^\[(.+)\]");
                if (matches.Count != 0)
                {
                    var text = matches.OfType<Match>().First().Value;
                    Console.WriteLine(text.Remove(0, 1).Remove(text.Length - 2, 1));
                    current_group_name = text.Remove(0, 1).Remove(text.Length - 2, 1);

                    continue;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    if (group_collection.ContainsKey(current_group_name))
                        group_collection[current_group_name].AddRange(current_host_group);
                    else
                        group_collection.Add(current_group_name, current_host_group);

                    current_host_group = new List<string>();

                    continue;
                }

                //current_host_group.Add(Regex.Match(line, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}").Value);
                current_host_group.AddRange(line.Split(' ').Skip(1).ToArray());
            }

            Dictionary<string, Dictionary<string, List<string>>> host_groups = new Dictionary<string, Dictionary<string, List<string>>>();

            //Iteration for domain group
            foreach (var host_group in group_collection)
            {
                Dictionary<string, List<string>> host_group_dict = new Dictionary<string, List<string>>();

                Console.WriteLine(host_group.Key);
                foreach (var host in host_group.Value)
                {
                    if (string.IsNullOrWhiteSpace(host))
                        continue;

                    var ip = "";
                    try
                    {
                        //Dns.GetHostAddresses(host).ToList().ForEach(ip => Console.WriteLine(ip.ToString()));
                        ip = Dns.GetHostAddresses(host).First().ToString();
                        Console.WriteLine(ip);
                    }
                    catch (System.Net.Sockets.SocketException ex)
                    {
                        Console.WriteLine($"Can't find {host}");
                    }

                    if (host_group_dict.ContainsKey(ip))
                        host_group_dict[ip].Add(host);
                    else
                        host_group_dict.Add(ip, new List<string>() { host });
                }

                host_groups.Add(host_group.Key, host_group_dict);
            }

            Console.Clear();

            Console.WriteLine(@"
# Copyright (c) 1993-2009 Microsoft Corp.
#
# This is a sample HOSTS file used by Microsoft TCP/IP for Windows.
#
# This file contains the mappings of IP addresses to host names. Each
# entry should be kept on an individual line. The IP address should
# be placed in the first column followed by the corresponding host name.
# The IP address and the host name should be separated by at least one
# space.
#
# Additionally, comments (such as these) may be inserted on individual
# lines or following the machine name denoted by a '#' symbol.
#
# For example:
#
#      102.54.94.97     rhino.acme.com          # source server
#       38.25.63.10     x.acme.com              # x client host

# localhost name resolution is handled within DNS itself.
#	127.0.0.1       localhost
#	::1             localhost

#=========================================
#     Custom hosts by RoganMatrivski
#=========================================


");

            foreach (var original_hosts in File.ReadAllLines(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\System32\drivers\etc\hosts"))
                if (!string.IsNullOrWhiteSpace(original_hosts) && original_hosts[0] != '#')
                    Console.WriteLine(original_hosts);

            foreach (var hosts_group in host_groups)
            {
                Console.WriteLine($"# {hosts_group.Key}");

                foreach (var hosts in hosts_group.Value)
                {
                    if (string.IsNullOrWhiteSpace(hosts.Key))
                    {
                        Console.Write("# Can't find an IP for these hosts :" + " " + string.Join(" ", hosts.Value) + Environment.NewLine);
                        continue;
                    }

                    Console.Write(hosts.Key + " " + string.Join(" ", hosts.Value) + Environment.NewLine);
                }

                Console.WriteLine(" ");
            }
        }
    }
}
