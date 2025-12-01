using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using evidence_timeline.Models;

namespace evidence_timeline.Utilities
{
    public static class PathHelper
    {
        private static readonly HashSet<char> InvalidFileNameChars = new(Path.GetInvalidFileNameChars());

        public static string Slug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            foreach (var ch in text.Trim().ToLowerInvariant())
            {
                if (char.IsWhiteSpace(ch))
                {
                    builder.Append('_');
                    continue;
                }

                if (InvalidFileNameChars.Contains(ch))
                {
                    continue;
                }

                builder.Append(ch);
            }

            var slug = builder.ToString().Trim('_');
            while (slug.Contains("__", StringComparison.Ordinal))
            {
                slug = slug.Replace("__", "_", StringComparison.Ordinal);
            }

            return slug;
        }

        public static string GetCaseFolderName(CaseInfo caseInfo)
        {
            var parts = new[]
            {
                Slug(caseInfo.CaseNumber),
                Slug(caseInfo.Name)
            }.Where(p => !string.IsNullOrWhiteSpace(p));

            return string.Join("_", parts);
        }

        public static string GetEvidenceFolderName(int evidenceNumber, DateOnly sortDate, string title, string typeName)
        {
            var parts = new List<string>
            {
                evidenceNumber.ToString("D4"),
                sortDate.ToString("yyyyMMdd"),
                Slug(title),
                Slug(typeName)
            }.Where(p => !string.IsNullOrWhiteSpace(p));

            return string.Join("_", parts);
        }

        public static DirectoryInfo EnsureDirectory(string path)
        {
            return Directory.CreateDirectory(path);
        }
    }
}
