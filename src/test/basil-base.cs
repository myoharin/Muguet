using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SineVita.Basil
{
    // Basil is a debugger class! The inheritance is nothing but fluff~, but also can sometimes help route error message
    public abstract class Basil
    {
        // Optional: Set this to true to enable debugging output
        public static bool DebugEnabled { get; set; } = true;
        public static int MaxDebugLogCount = 1024;
        public static int DebugLogCount = 0;
        public static LinkedList<Tuple<byte, string>> DebugLog= new LinkedList<Tuple<byte, string>>();

        // instantiate
        public Basil(){}

        // Log - 0
        // Warn - 1
        // Error - 2
        // Debug - 3
        // breakers and other ornaments - 4

        public static void Log(string message)
        {
            if (DebugEnabled){Console.WriteLine($"LOG: {message}");}
            DebugLog.AddLast(new Tuple<byte, string>(0, message));
            if (DebugLogCount > MaxDebugLogCount) {
                DebugLog.RemoveFirst();
            } else {
                DebugLogCount++;
            }
        }
        public static void Warn(string message)
        {
            if (DebugEnabled){Console.WriteLine($"WARNING: {message}");}
            DebugLog.AddLast(new Tuple<byte, string>(1, message));
            if (DebugLogCount > MaxDebugLogCount) {
                DebugLog.RemoveFirst();
            } else {
                DebugLogCount++;
            }
        }
        public static void Error(string message)
        {
            if (DebugEnabled){Console.WriteLine($"ERROR: {message}");}
            DebugLog.AddLast(new Tuple<byte, string>(2, message));
            if (DebugLogCount > MaxDebugLogCount) {
                DebugLog.RemoveFirst();
            } else {
                DebugLogCount++;
            }
        }
        public static void Debug(string message)
        {
            if (DebugEnabled){Console.WriteLine($"DEBUG: {message}");}
            DebugLog.AddLast(new Tuple<byte, string>(3, message));
            if (DebugLogCount > MaxDebugLogCount) {
                DebugLog.RemoveFirst();
            } else {
                DebugLogCount++;
            }
        }

        // breaker function
        public static void AddBreaker_1() {
            string message = "| - - - - - - - - - |";
            if (DebugEnabled){Console.WriteLine(message);}
            DebugLog.AddLast(new Tuple<byte, string>(4, message));
            if (DebugLogCount > MaxDebugLogCount) {
                DebugLog.RemoveFirst();
            } else {
                DebugLogCount++;
            }
        }


    }
}