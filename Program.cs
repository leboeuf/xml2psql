﻿using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace xml2psql
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: xml2psql ./path-to-xml-file");
                return;
            }

            // Validate arguments
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Error: first argument must be a path to an XML file.");
                return;
            }

            // Load XML file
            var xml = TryLoadXml(args[0]);
            if (xml == null)
            {
                return;
            }

            // Validate XML file starts with <xml2psql>
            if (xml.Root.Name.LocalName != "xml2psql")
            {
                Console.WriteLine("Error: file must start with a <xml2psql> node.");
                return;
            }

            // Validate XML file contains connection string
            var connectionStringNode = xml.XPathSelectElement("/xml2psql/ConnectionString[1]");
            if (connectionStringNode == null || string.IsNullOrWhiteSpace(connectionStringNode.Value))
            {
                Console.WriteLine("Error: file must contain a non-empty <ConnectionString> node.");
                return;
            }

            // Parse XML file
            var tables = XmlParser.ParseXml(xml);

            // Write to database
            var connectionString = xml.XPathSelectElement("/xml2psql/ConnectionString[1]").Value;
            await DbSchemaWriter.Write(connectionString, tables);
        }

        static XDocument TryLoadXml(string path)
        {
            try
            {
                return XDocument.Load(path);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error loading XML document: {e.Message}");
                return null;
            }
        }
    }
}
