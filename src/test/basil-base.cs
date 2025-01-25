using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SineVita.Basil
{
    // Basil is a debugger class! The inheritance is nothing but fluff~, but also can sometimes help route error message
    public abstract class Basil
    {
        // Optional: Set this to true to enable debugging output
        public static bool DebugEnabled { get; set; } = true;
        public static string? DebugFolder = "debug";
        public static string? DebugFile;
        public static bool DebugFolderSet { get { return DebugFolder != null; } }

        // instantiate
        public Basil(){}


        public static void initialize() {
            if (DebugFolder == null) {return;}
            if (!Directory.Exists(DebugFolder)) {
                Directory.CreateDirectory(DebugFolder);
            }
            DebugFile = Path.Combine(DebugFolder, $"log_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            Log("Debugger session started.");
        }


        public static void Log(string message) {
            if (DebugEnabled){
                Console.WriteLine("Log: " + message);
                if (DebugFolder != null) {
                    if (DebugFile == null) {initialize();}
                    string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                    File.AppendAllText(DebugFile!, logMessage + Environment.NewLine);
                }
            }
        }


        // breaker function
        public static void AddBreaker_1() {
            string message = "| - - - - - - - - - |";
            if (DebugEnabled){Console.WriteLine(message);}
        }


    }
}