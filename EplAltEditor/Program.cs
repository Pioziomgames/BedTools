using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using PiosBedLibrary;
using Newtonsoft.Json;

namespace EplAltEditor
{
    public class Declarations
    {
        public List<Declaration> declarations { get; set; }

    }
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
                Console.WriteLine($"EplAltEditor v{Version.Major}.{Version.Minor}.{Version.Build}\n" +
                    $"Extracts and Repacks contents of p3 and p4 Epl Files\n" +
                    $"Without unpacking ep files\n" +
                    $"Usage:\n" +
                    $"       EplAltEditor.exe InputFile (optional)OutputFolder\n" +
                    $"       EplAltEditor.exe InputFolder (optional)OutputFile");
                Exit();
            }
            string path = @$"{Path.GetDirectoryName(InputFile)}\{Path.GetFileNameWithoutExtension(InputFile)}_extracted"; // deletes the extension from the filename and adds _extracted

            if (args.Length > 1)
                path = args[1];


            if (File.Exists(InputFile))
            {
                Console.WriteLine($"Loading: {InputFile}...");

                Epl EplFile = new Epl(InputFile);

                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                Directory.CreateDirectory(path + @"\ep");
                Directory.CreateDirectory(path + @"\Declarations");

                EplFile.header.Save(path + @"\header");



                string zeros = "";
                for (int i = 0; i < EplFile.Eps.Count.ToString().Length; i++)
                    zeros += "0";

                if (zeros == "0")
                    zeros = "00";

                for (int i = 0; i < EplFile.Eps.Count; i++)
                    EplFile.Eps[i].Save($@"{path}\ep\{i.ToString(zeros)}.ep");

                zeros = "";
                for (int i = 0; i < EplFile.Declarations.Count.ToString().Length; i++)
                    zeros += "0";

                if (zeros == "0")
                    zeros = "00";

                for (int i = 0; i < EplFile.Declarations.Count; i++)
                    EplFile.Declarations[i].Save($@"{path}\Declarations\{i.ToString(zeros)}.dec");



                var json = EplFile.Order;

                var JsonToWrite = JsonConvert.SerializeObject(json, Formatting.Indented);

                using (var writer = new StreamWriter(path + @"\Order.json"))
                {
                    writer.Write(JsonToWrite);
                }

                Console.WriteLine($"Epl File extracted to: {path}");
                return;
            }
            else if (!Directory.Exists(InputFile))
            {
                Console.WriteLine($"\n{InputFile} does not exist");
                Exit();
            }

            if (args.Length == 1)
                path = InputFile + ".epl";

            Console.WriteLine($"Reading the contents of: {InputFile}...");

            if (!File.Exists(InputFile + @"\header"))
            {
                Console.WriteLine("\nheader file does not exist");
                Exit();
            }

            if (!File.Exists(InputFile + @"\Order.json"))
            {
                Console.WriteLine("\nOrder.json does not exist");
                Exit();
            }

            Header header = new Header(InputFile + @"\header");

            string[] files = Directory.GetFiles(InputFile + @"\ep", "*.ep");
            List<Ep> Eps = new List<Ep>();

            foreach (string file in files)
                Eps.Add(new Ep(file));

            files = Directory.GetFiles(InputFile + @"\Declarations", "*.dec");
            List<Declaration> Declarations = new List<Declaration>();

            foreach (string file in files)
                Declarations.Add(new Declaration(file));


            string JsonFromFile;
            using (var reader = new StreamReader(InputFile + @"\Order.json"))
                JsonFromFile = reader.ReadToEnd();

            List<int> Order = JsonConvert.DeserializeObject<List<int>>(JsonFromFile);

            if (Order.Count != Declarations.Count)
            {
                Console.WriteLine($"\nNumber of declarations({Declarations.Count}) doesn't match the number of values in Order.json({Order.Count})");
                Exit();
            }

            header.DeclarationCount = Declarations.Count;

            List<uint> OffsetList = new List<uint>();

            uint size = 144;
            size += (uint)Declarations.Count * 192;
            foreach (Ep Ep in Eps)
            {
                OffsetList.Add(size);
                size += (uint)Ep.EpSize;
            }

            List<int> UsedValues = new List<int>();

            for (int i = 0; i < Declarations.Count; i++)
            {

                if (UsedValues.Contains(Order[i]))
                {
                    Declarations[i].OffsetOrIndex = (uint)Order[i];
                    Declarations[i].UsesIndex = true;
                }
                else
                {
                    Declarations[i].OffsetOrIndex = OffsetList[Order[i]];
                    Declarations[i].UsesIndex = false;
                    UsedValues.Add(Order[i]);
                }

            }

            Epl eplFile = new Epl();

            eplFile.header = header;
            eplFile.Eps = Eps;
            eplFile.Declarations = Declarations;

            eplFile.Save(path);

            Console.WriteLine($"\nEpl File Saved to: {path}");
        }
    }
}
