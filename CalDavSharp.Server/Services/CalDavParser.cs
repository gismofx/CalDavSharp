using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using CalDavSharp.Server.Models;

namespace CalDavSharp.Server.Services
{
    public class CalDavParser
    {
        /// <summary>
        /// Parse the D:PROP Node
        /// </summary>
        /// <param name="propNode"></param>
        /// <returns></returns>
        public List<string> ParseProperties(XmlNode propNode)
        {
            var myList = new List<string>();
            foreach (XmlNode node in propNode.ChildNodes)
            {
                if (node.NodeType is not XmlNodeType.Whitespace)
                {
                    myList.Add(node.Name);//prefix or local name?
                    Debug.WriteLine(node.Name);
                }
            }
            return myList;
        }

        /// <summary>
        /// Parse the C:FILTER node
        /// </summary>
        /// <param name="filterNode"></param>
        /// <returns></returns>
        public Dictionary<string,string> ParseFilter(XmlNode filterNode)
        {
            var myList = new Dictionary<string,string>();
            foreach (XmlNode node in filterNode.ChildNodes)
            {
                if (node.NodeType is not XmlNodeType.Whitespace)
                {
                    myList.Add(node.Name,node.Attributes[0].Value);//prefix or local name?
                    Debug.WriteLine(node.Name);
                }
            }
            return myList;
        }

        /// <summary>
        /// Parse a PROPFIND Request
        /// </summary>
        /// <param name="xml"></param>
        /// <returns>A ParseRequestObject</returns>
        public ParsedRequest ParsePropfind(XmlDocument xml)
        {
            if (!xml.HasChildNodes)
            {
                return null;
            }

            var result = new ParsedRequest();

            foreach (XmlNode xNode in xml.FirstChild.ChildNodes)
            {
                if (xNode.NodeType is not XmlNodeType.Whitespace)
                {
                    Debug.WriteLine(xNode.Name);
                    switch (xNode.Name.ToLower())
                    {
                        case "d:prop":
                            result.RequestedProperties = ParseProperties(xNode);
                            break;
                        case "c:filter":
                            ParseFilter(xNode);
                            break;
                        default:
                            break;

                    }
                }
            }

            return result;
        }

        public ParsedRequest ParseReport(XmlDocument xml)
        {
            return null;
        }

    }
}
