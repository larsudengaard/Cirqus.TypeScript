using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Serilog;

namespace Cirqus.TypeScript.Generation
{
    class ProxyGenerationResult
    {
        static readonly Encoding Encoding = Encoding.UTF8;

        readonly string _filename;
        readonly string _code;

        public ProxyGenerationResult(string filename, string code)
        {
            _filename = filename;
            _code = code;
        }

        public void WriteTo(string destinationDirectory)
        {
            var destinationFilePath = Path.Combine(destinationDirectory, _filename);

            if (File.Exists(destinationFilePath) && !HasChanged(destinationFilePath))
            {
                Log.Logger.Information("    No changes - skipping {0}", destinationFilePath);
                return;
            }

            Log.Logger.Information("    Writing {0}", destinationFilePath);
            var header = string.Format(HeaderTemplate, HashPrefix, GetHash());
            
            var output = new StringBuilder();
            output.Append(header);
            output.AppendLine("");
            output.AppendLine("");
            output.AppendLine(_code);

            File.WriteAllText(destinationFilePath, output.ToString(), Encoding);
        }

        string GetHash()
        {
            return Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.GetBytes(_code)));
        }

        bool HasChanged(string destinationFilePath)
        {
            return GetHash() != GetHashFromFile(destinationFilePath);
        }

        string GetHashFromFile(string destinationFilePath)
        {
            using (var file = File.OpenRead(destinationFilePath))
            using (var reader = new StreamReader(file, Encoding))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    var trimmedLine = line.TrimStart();
                    
                    if (string.IsNullOrWhiteSpace(trimmedLine)) break;

                    if (!trimmedLine.StartsWith(HashPrefix)) continue;
                    
                    var hash = trimmedLine.Substring(HashPrefix.Length);

                    return hash;
                }
            }

            return "";
        }

        const string HeaderTemplate = @"/* 
    Generated with d60.Cirqus.TsClient.exe
    Should not be edited directly - should probably be regenerated instead :)
    {0}{1}
*/";
        const string HashPrefix = "Hash: ";
    }
}