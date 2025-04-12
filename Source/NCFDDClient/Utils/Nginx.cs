using System.Text.RegularExpressions;

namespace NCFDDClient.Utils
{
    internal static class Nginx
    {
        public static List<string> GetSitesEnabledDomains()
        {
            const string sitesEnabledPath = "/etc/nginx/sites-enabled";

            if (Directory.Exists(sitesEnabledPath))
            {
                var files = Directory.GetFiles(sitesEnabledPath);
                if (files.Length > 0)
                {
                    var domainList = new HashSet<string>();
                    Regex serverNameRegex = new Regex(@"^\s*server_name\s+(.+?);", RegexOptions.IgnoreCase);

                    foreach (var file in files)
                    {
                        if (File.Exists(file))
                        {
                            string[] configLines = File.ReadAllLines(file);
                            foreach (var line in configLines)
                            {
                                if (line.Trim().StartsWith('#')) continue;

                                var match = serverNameRegex.Match(line);
                                if (match.Success)
                                {
                                    var domain = match.Groups[1].Value.Trim().ToLower();

                                    if (!domainList.Contains(domain))
                                    {
                                        domainList.Add(domain);
                                    }
                                }
                            }
                        }
                    }

                    if (domainList.Count > 0)
                    {
                        var sortedList = domainList.ToList();
                        sortedList.Sort();
                        return sortedList;
                    }
                }
            }

            return [];
        }
    }
}