using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Cirqus.TypeScript.Generation
{
    class ProxyGenerationResult
    {
        static readonly Encoding Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        public string Code { get; }

        public ProxyGenerationResult(string code)
        {
            Code = code;
        }

        public void WriteTo(string destinationFilePath)
        {
            if (File.Exists(destinationFilePath) && !HasChanged(destinationFilePath))
            {
                Console.WriteLine("    No changes - skipping {0}", destinationFilePath);
                return;
            }

            Console.WriteLine("    Writing {0}", destinationFilePath);
            var header = string.Format(HeaderTemplate, HashPrefix, GetHash());
            
            var output = new StringBuilder();
            output.Append(header);
            output.AppendLine("");
            output.AppendLine("");
            output.AppendLine(Code);

            File.WriteAllText(destinationFilePath, output.ToString(), Encoding);
        }

        string GetHash()
        {
            return Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.GetBytes(Code)));
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
    Generated with Cirqus.TypeScript.exe
    Should not be edited directly - should probably be regenerated instead :)
    {0}{1}
*/";
        const string HashPrefix = "Hash: ";
    }
}