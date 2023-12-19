﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Studyzy.IMEWLConverter.Helpers
{
    public class DictionaryHelper
    {
        private static readonly Dictionary<char, ChineseCode> dictionary = new Dictionary<char, ChineseCode>();

        private static Dictionary<char, ChineseCode> Dict
        {
            get
            {
                if (dictionary.Count == 0)
                {
                    string allPinYin = GetResourceContent("ChineseCode.txt");
                    string[] pyList = allPinYin.Split(new[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < pyList.Length; i++)
                    {
                        string[] hzpy = pyList[i].Split('\t');
                        char hz = Convert.ToChar(hzpy[1]);

                        dictionary.Add(hz, new ChineseCode
                        {
                            Code = hzpy[0],
                            Word = hzpy[1][0],
                            Wubi86 = hzpy[2],
                            Wubi98 = (hzpy[3] == "" ? hzpy[2] : hzpy[3]),
                            Pinyins = hzpy[4],
                            Freq = Convert.ToDouble(hzpy[5])
                        });
                    }
                }
                return dictionary;
            }
        }

        public static ChineseCode GetCode(char c)
        {
            if(Dict.ContainsKey(c))
            return Dict[c];
            else
            {
                throw new Exception("给定关键字不在字典中，【"+c+"】");
            }
        }


        public static List<ChineseCode> GetAll()
        {
            return new List<ChineseCode>(Dict.Values);
        }
        public static string GetResourceContent(string fileName)
        {
            string file;
            var assembly = typeof(DictionaryHelper).GetTypeInfo().Assembly;

            using (var stream = assembly.GetManifestResourceStream("ImeWlConverterCore.Resources." + fileName))
            {
                using (var reader = new StreamReader(stream,true))
                {
                    file = reader.ReadToEnd();
                }
            }
            return file;
        }

    }

    public struct ChineseCode
    {
        public string Code { get; set; }
        public char Word { get; set; }
        public string Wubi86 { get; set; }
        public string Wubi98 { get; set; }
        public string Pinyins { get; set; }
        public double Freq { get; set; }
    }
}