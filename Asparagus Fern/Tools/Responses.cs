using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.IO;
using Asparagus_Fern.Tools;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using System.Text.RegularExpressions;

public partial class Responses
{
    public partial class ResponseAttribute : Attribute
    {
        public string helpMessage;

        public ResponseAttribute(string helpMessage)
        {
            this.helpMessage = helpMessage;
        }
    }

    public class LookupTree<T> where T : class
    {
        public class Node<K> where K : class
        {
            public Dictionary<char, Node<K>> nodes = new Dictionary<char, Node<K>>();

            public K value;
            bool hasValue => value != null;

            public Node()
            {
            }
        }

        Node<T> treeBase = new Node<T>();

        public LookupTree()
        {
            
        }

        public void AddValue(string name, T val)
        {
            int i = 0;
            Node<T> currentNode = treeBase;

            do
            {
                if (currentNode.nodes.ContainsKey(name[i]))
                {
                    currentNode = currentNode.nodes[name[i]];
                }
                else
                {
                    var nextNode = new Node<T>();
                    currentNode.nodes[name[i]] = nextNode;
                    currentNode = nextNode;
                }
                i++;
            } while (i < name.Length);

            currentNode.value = val;
        }

        public T FindHit(string name)
        {
            int i = 0;
            Node<T> currentNode = treeBase;

            while (i < name.Length && currentNode.nodes.Any())
            {
                char c = name[i];
                if (currentNode.nodes.ContainsKey(c))
                {
                    currentNode = currentNode.nodes[c];
                }
                else
                {
                    return null;
                }
                i++;
            } 

            return currentNode.value;
        }

        public T FindExactHit(string name)
        {
            int i = 0;
            Node<T> currentNode = treeBase;

            while (i < name.Length)
            {
                char c = name[i];
                if (currentNode.nodes.ContainsKey(c))
                {
                    currentNode = currentNode.nodes[c];
                }
                else
                {
                    return null;
                }
                i++;
            }

            return currentNode.value;
        }
    }

    static LookupTree<Enum> responseSearchTree = new LookupTree<Enum>();

    public enum Default
    {
        [Response("For general help for commands use `{0}`")] FernHelp,
        [Response("For information on the 150 calories game type `{0}`")] FernHelpCalories150,
    }

    public enum None
    {
        
    }

    public static IEnumerable<string> GetAllResponses()
    {
        return typeof(Responses)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(string))
            .Select(f => (string)f.GetValue(null));
    }

    public static void CompileResponses()
    {
        Regex regex = new Regex("(\\B[A-Z])");

        foreach (var enumType in typeof(Responses).GetNestedTypes())
        {
            if (!enumType.IsEnum)
            {
                continue;
            }

            foreach (var enumVal in Utilities.GetEnums(enumType))
            {
                var key = enumVal.ToString();
                responseSearchTree.AddValue(regex.Replace(key, " $1").ToLower(), enumVal);
            }
        }
    }

    public static Enum SearchForCommand(string messageLowercase)
    {
        return responseSearchTree.FindHit(messageLowercase);
    }
}