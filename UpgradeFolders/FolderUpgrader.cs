using System.Text.RegularExpressions;

namespace UpgradeFolders
{

    internal class FolderUpgrader
    {
        private double JaccardThreshold { get; set; } = 0.7;
        private double LevenshteinThreshold { get; set; } = 0.8;
        private int MinLength { get; set; } = 4;

        public void SetThresholds(double? jaccardThreshold = null, double? levenshteinThreshold = null, int? minLength = null)
        {
            if (jaccardThreshold.HasValue)
            {
                JaccardThreshold = jaccardThreshold.Value;
            }
            if (levenshteinThreshold.HasValue)
            {
                LevenshteinThreshold = levenshteinThreshold.Value;
            }
            if (minLength.HasValue)
            {
                MinLength = minLength.Value;
            }
        }

        public void UpgradeFolders(string originalRootPath)
        {
            Stack<string> stack = new();
            stack.Push(@"\\?\" + originalRootPath);

            while (stack.Count > 0)
            {
                string currentPath = stack.Pop();
                string director = Path.GetFullPath(currentPath);
                if (string.IsNullOrEmpty(currentPath) || !Directory.Exists(director))
                {
                    continue;
                }

                foreach (string subDirectory in Directory.GetDirectories(director))
                {
                    stack.Push(subDirectory);
                }

                if (UpgradeSingleFolder(currentPath))
                {
                    string? parentPath = Path.GetDirectoryName(currentPath);
                    if (!string.IsNullOrEmpty(parentPath) && currentPath != originalRootPath)
                    {
                        stack.Push(parentPath);
                    }
                }
            }
        }

        private bool UpgradeSingleFolder(string folderPath)
        {
            string[] subDirectories = Directory.GetDirectories(folderPath);
            if (subDirectories.Length != 1)
            {
                return false;
            }

            string subFolderPath = subDirectories[0];
            if (!IsSimilarEnough(Path.GetFileName(folderPath), Path.GetFileName(subFolderPath)))
            {
                return false;
            }

            if (MoveContentsToParent(subFolderPath, folderPath))
            {
                DeleteEmptyFolders(subFolderPath, folderPath);
            }
            return true;
        }

        private static bool MoveContentsToParent(string sourceFolder, string parentFolder)
        {
            bool allMoved = true;

            try
            {
                foreach (string file in Directory.EnumerateFiles(sourceFolder))
                {
                    string destFile = Path.Combine(parentFolder, Path.GetFileName(file));
                    if (!File.Exists(destFile))
                    {
                        File.Move(file, destFile);
                    }
                    else
                    {
                        allMoved = false;
                    }
                }

                foreach (string directory in Directory.EnumerateDirectories(sourceFolder))
                {
                    string destDirectory = Path.Combine(parentFolder, Path.GetFileName(directory));
                    if (!Directory.Exists(destDirectory))
                    {
                        Directory.Move(directory, destDirectory);
                    }
                    else
                    {
                        allMoved = false;
                    }
                }
            }
            catch
            {
                allMoved = false;
            }

            return allMoved && !Directory.EnumerateFileSystemEntries(sourceFolder).Any();
        }

        private static void DeleteEmptyFolders(string currentFolder, string stopFolder)
        {
            while (!string.Equals(currentFolder, stopFolder, StringComparison.OrdinalIgnoreCase))
            {
                if (!Directory.Exists(currentFolder))
                {
                    break;
                }

                if (!Directory.EnumerateFileSystemEntries(currentFolder).Any())
                {
                    string? parentFolder = Path.GetDirectoryName(currentFolder);
                    if (parentFolder == null)
                    {
                        break;
                    }

                    try
                    {
                        Directory.Delete(currentFolder);
                        currentFolder = parentFolder;
                    }
                    catch
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private bool IsSimilarEnough(string str1, string str2)
        {
            List<string> parts1 = SplitIntoParts(str1);
            List<string> parts2 = SplitIntoParts(str2);

            double jaccardSimilarity = CalculateJaccardSimilarity(parts1, parts2);

            if (jaccardSimilarity >= JaccardThreshold && str1.Length > 5 && str2.Length > 5)
            {
                return true;
            }

            if (str1.Length <= MinLength || str2.Length <= MinLength)
            {
                return str1 == str2;
            }

            double levenshteinSimilarity = CalculateLevenshteinSimilarity(string.Join(" ", parts1), string.Join(" ", parts2));
            return levenshteinSimilarity >= LevenshteinThreshold;
        }

        private static List<string> SplitIntoParts(string input)
        {
            return Regex.Split(input, @"(\[.*?\]|\S+)")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim().ToLower(System.Globalization.CultureInfo.CurrentCulture))
                .Where(p => !Regex.IsMatch(p, @"^\d+$"))
                .ToList();
        }

        private static double CalculateJaccardSimilarity(List<string> list1, List<string> list2)
        {
            HashSet<string> set1 = new(list1);
            HashSet<string> set2 = new(list2);

            int intersectionCount = set1.Intersect(set2).Count();
            int unionCount = set1.Union(set2).Count();

            return (double)intersectionCount / unionCount;
        }

        private static double CalculateLevenshteinSimilarity(string s1, string s2)
        {
            int distance = ComputeLevenshteinDistance(s1, s2);
            int maxLength = Math.Max(s1.Length, s2.Length);
            return 1 - ((double)distance / maxLength);
        }

        private static int ComputeLevenshteinDistance(string source, string target)
        {
            int m = source.Length;
            int n = target.Length;
            int[,] d = new int[m + 1, n + 1];

            for (int i = 0; i <= m; i++)
            {
                d[i, 0] = i;
            }

            for (int j = 0; j <= n; j++)
            {
                d[0, j] = j;
            }

            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    int cost = (source[i - 1] == target[j - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }

            return d[m, n];
        }
    }
}