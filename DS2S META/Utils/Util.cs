﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace DS2S_META.Utils
{
    internal static class Util
    {

        public static readonly string ExeDir = Environment.CurrentDirectory;

        public static int DeleteFromEnd(int num, int n)
        {
            for (int i = 1; num != 0; i++)
            {
                num = num / 10;

                if (i == n)
                    return num;
            }

            return 0;
        }

        public static string GetEmbededResource(string item)
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            string resourceName = $"Erd_Tools.{item}";

            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new NullReferenceException($"Could not find embedded resource: {item} in the {Assembly.GetCallingAssembly().GetName()} assembly");

                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            
        }


        public static string GetTxtResource(string filePath)
        {
            //Get local directory + file path, read file, return string contents of file

            //Path.Combine(Environment.CurrentDirectory, filePath);
            if (!File.Exists($@"{ExeDir}/{filePath}"))
                return "";

            string fileString = File.ReadAllText($@"{ExeDir}/{filePath}");

            return fileString;
        }

        public static string[] GetListResource(string filePath)
        {
            //Get local directory + file path, read file, return string contents of file
            if (!File.Exists($@"{ExeDir}/{filePath}"))
                return Array.Empty<string>();

            string[] stringArray = File.ReadAllLines($@"{ExeDir}/{filePath}");

            return stringArray;
        }

        public static bool IsValidTxtResource(string txtLine)
        {
            //see if txt resource line is valid and should be accepted 
            //(bare bones, only checks for a couple obvious things)

            if (txtLine.Contains("//"))
            {
                txtLine = txtLine.Substring(0, txtLine.IndexOf("//")); // remove everything after "//" comments
            };

            if (string.IsNullOrWhiteSpace(txtLine) == true || txtLine.Contains('#')) //empty line check
            {
                return false; //resource line invalid
            };

            return true; //resource line valid
        }

        /// <summary>
        /// Removes everything after // in a string and returns the new string with .Trim()
        /// </summary>
        /// <param name="txtLine"></param>
        /// <returns>txtLine.Trim() with everything after // removed</returns>
        public static string TrimComment(this string txtLine)
        {
            //Repurposing Kingborehahas code for checking valid resource to trim hashes
            if (txtLine.Contains("//"))
            {
                txtLine = txtLine.Substring(0, txtLine.IndexOf("//")); // remove everything after "//" comments
            };

            return txtLine.Trim();
        }


        public static string[] RegexSplit(string source, string pattern)
        {
            return Regex.Split(source, pattern);
        }

        public static T? DeserializeXml<T>(string filePath)
        {
            var xml = new XmlDocument();
            TextReader textReader = new StreamReader(@$"{ExeDir}/{filePath}");
            XmlSerializer serializer = new(typeof(T));
            return (T?)serializer.Deserialize(textReader);
        }

        public static IEnumerable<T> CollateCalls<T>(Func<T> f, int count)
        {
            // Call a function (no args) N times and return results as enumerable

            List<T> results = new(); // preallocate empty
            for (var i = 0; i < count; i++)
                results.Add(f());
            return results;
        }

        // Technically doesn't belong here, but w/e
        public static void ExecuteAsAdmin(string fileName)
        {
            Process proc = new();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Verb = "runas";
            proc.Start();
        }
    }
}
