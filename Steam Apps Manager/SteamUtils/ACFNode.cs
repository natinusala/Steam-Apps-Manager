using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Adapted from https://github.com/Grumbel/steamtools/blob/master/acf.py

namespace Steam_Apps_Manager.SteamUtils
{
    class ACFNode : Dictionary<String, Object>
    {
        public static ACFNode ParseACF(string file)
        {
            return new ACFNode(new StringReader(File.ReadAllText(file, new UTF8Encoding(false))));
        }

        private string InternalToString(string str, int level)
        {
            string tabulations = new string('\t', level);
            foreach (KeyValuePair<string, Object> entry in this)
            {
                str += tabulations + "\"" + entry.Key + "\"";

                if (entry.Value.GetType() == typeof(ACFNode))
                {
                    str += tabulations + "\n{\n";
                    str = tabulations + ((ACFNode)entry.Value).InternalToString(str, level + 1);
                    str += tabulations + "\n}\n";
                }
                else
                {
                    str += tabulations + "\t\t" + "\"" + (string)entry.Value + "\"\n";
                }
            }

            return str;
        }

        public override string ToString()
        {
            return InternalToString("", 0);
        }

        public string GetValue(params string[] keys)
        {
            ACFNode root = this;
            for (int i = 0; i < keys.Length-1; i++)
            {
                string key = keys[i];

                if (root.ContainsKey(key))
                    root = (ACFNode)root[key];
                else
                    return null;
            }
            return (string) root[keys[keys.Length - 1]];
        }

        public ACFNode(StringReader content)
        {
            while (true)
            {
                char tokenType;
                
                tokenType = ScanForNextToken(content);
                
                if (tokenType == '}' || tokenType == '\uffff')
                    return;

                if (tokenType != '"')
                {
                    throw new Exception("Invalid token : '" + tokenType + "'");
                }
                                                
                string name = ParseQuotedToken(content);

                tokenType = ScanForNextToken(content);

                if (tokenType == '"')
                    this.Add(name, ParseQuotedToken(content));
                else if (tokenType == '{')
                    this.Add(name, new ACFNode(content));
                else
                {
                    throw new Exception("Invalid token : '" + tokenType + "'");
                }
            }
        }


        private char ScanForNextToken(StringReader content)
        {
            char c = (char) content.Read();

            if (c == '\uffff')
            {
                return c;
            }

            while (c == ' ' || c == '\n' || c == '\r' || c == '\t')
            {
                if (c == '\uffff')
                {
                    return c;
                }

                c = (char) content.Read();
            }
            return c;
        }

        private string ParseQuotedToken(StringReader content)
        {
            string ret = "";

            char c = (char) content.Read();

            while (c != '"')
            {
                ret += c;
                c = (char) content.Read();
            }
            return ret;
        }
    }
}
