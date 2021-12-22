using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using PiosBedLibrary;

namespace BedEditor
{
    class Program
    {
        public static Assembly Assembly = Assembly.GetExecutingAssembly();
        public static AssemblyName AssemblyName = Assembly.GetName();
        public static Version Version = AssemblyName.Version;
        public static Type[] Types = Assembly.GetTypes();
        public static void Exit()
        {
            Console.WriteLine("\nPress Any Key to Quit");
            Console.ReadKey();
            System.Environment.Exit(0);
        }
        static void Main(string[] args)
        {

            string InputFile = "";


            if (args.Length > 0)
                InputFile = args[0];
            else
            {
                Console.WriteLine($"BedEditor v{Version.Major}.{Version.Minor}.{Version.Build}\n" +
                    $"Extracts and Repacks contents of p3 and p4 Bed Files\n" +
                    $"Usage:\n" +
                    $"       BedEditor.exe InputFile (optional)OutputFolder\n" +
                    $"       BedEditor.exe InputFolder (optional)OutputFile");
                Exit();
            }
            string path = @$"{Path.GetDirectoryName(InputFile)}\{Path.GetFileNameWithoutExtension(InputFile)}_extracted"; // deletes the extension from the filename and adds _extracted
            
            if (args.Length > 1)
                path = args[1];


            if(File.Exists(InputFile))
            {
                Console.WriteLine($"Loading: {InputFile}...");

                Bed BedFile = new Bed(InputFile);

                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                Directory.CreateDirectory(path);

                using (BinaryWriter writer = new BinaryWriter(File.Open(path + @"\header", FileMode.Create)))
                {
                    writer.Write(BedFile.MAGIC);
                    writer.Write(BedFile.header);
                }

                string zeros = "";
                for (int i=0; i < BedFile.EplFiles.Count.ToString().Length; i++)
                {
                    zeros += "0";
                }
                if (zeros == "0")
                    zeros = "00";

                for (int i = 0; i < BedFile.EplFiles.Count; i++)
                    BedFile.EplFiles[i].Save($@"{path}\{i.ToString(zeros)}.epl");

                Console.WriteLine($"Bed File extracted to: {path}");
                return;
            }
            else if (!Directory.Exists(InputFile))
            {
                Console.WriteLine($"\n{InputFile} does not exist");
                Exit();
            }

            if (args.Length == 1)
                path = InputFile + ".bed";

            Console.WriteLine($"Reading the contents of: {InputFile}...");

            if (!File.Exists(InputFile + @"\header"))
            {
                Console.WriteLine("\nheader file does not exist");
                Exit();
            }

            byte[] Header;
            char[] MAGIC;
            using (BinaryReader reader = new BinaryReader(File.Open(InputFile + @"\header", FileMode.Open)))
            {
                MAGIC = reader.ReadChars(8);
                Header = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
            }
            string[] files = Directory.GetFiles(InputFile, "*.epl");
            List<Epl> EplFiles = new List<Epl>();

            foreach (string file in files)
                EplFiles.Add(new Epl(file));

            Bed bedFile = new Bed();
            bedFile.MAGIC = MAGIC;
            bedFile.header = Header;
            bedFile.EplFiles = EplFiles;

            bedFile.Save(path);

            Console.WriteLine($"\nBed File Saved to: {path}");
        }
    }
}
