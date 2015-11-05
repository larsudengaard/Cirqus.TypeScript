using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Cirqus.TypeScript.Generation
{
    class ProxyGenerationResult
    {
        static readonly Encoding Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        readonly string _code;

        public ProxyGenerationResult(string code)
        {
            _code = code;
        }

        public void WriteTo(string destinationFilePath)
        {
            Console.WriteLine("    Writing {0}", destinationFilePath);
            
            var output = new StringBuilder();
            output.Append(HeaderTemplate);
            output.AppendLine("");
            output.AppendLine("");
            output.AppendLine(_code);

            File.WriteAllText(destinationFilePath, output.ToString(), Encoding);
        }

        const string HeaderTemplate = @"/* 
    Generated with Cirqus.TypeScript.exe
    Should not be edited directly - should probably be regenerated instead :)
*/";
    }
}