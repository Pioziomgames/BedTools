using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using PiosBedLibrary;
using Newtonsoft.Json;
using static PiosBedLibrary.Functions;

namespace EplEditor
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
                Console.WriteLine($"EplEditor v{Version.Major}.{Version.Minor}.{Version.Build}\n" +
                    $"Extracts and Repacks contents of p3 and p4 Epl Files\n" +
                    $"Usage:\n" +
                    $"       EplEditor.exe InputFile (optional)OutputFolder\n" +
                    $"       EplEditor.exe InputFolder (optional)OutputFile");
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
                {
                    
                    Debug.WriteLine($"Exporting ep: {i}");
                    Debug.WriteLine($"DataSize: {EplFile.Eps[i].Data.Length}");
                    Directory.CreateDirectory($@"{path}\ep\{i.ToString(zeros)}");
                    Debug.WriteLine($@"Writing to: {path}\ep\{i.ToString(zeros)}\HeaderData1");
                    File.WriteAllBytes($@"{path}\ep\{i.ToString(zeros)}\HeaderData1", EplFile.Eps[i].Ep1);
                    Debug.WriteLine($@"Writing to: {path}\ep\{i.ToString(zeros)}\HeaderData2");
                    File.WriteAllBytes($@"{path}\ep\{i.ToString(zeros)}\HeaderData2", EplFile.Eps[i].Ep2);
                    Debug.WriteLine($@"Writing to: {path}\ep\{i.ToString(zeros)}\EpData");
                    File.WriteAllBytes($@"{path}\ep\{i.ToString(zeros)}\EpData", EplFile.Eps[i].Data);
                    byte[] embedded = EplFile.Eps[i].File;
                    string extension = "";
                    if (embedded.Length > 12)
                    {

                        if (embedded[0] == (byte)79 && embedded[1] == (byte)77 && embedded[2] == (byte)71)
                            extension = ".gmo";
                        else if (embedded[8] == (byte)84 && embedded[9] == (byte)77 && embedded[10] == (byte)88 && embedded[11] == (byte)48)
                            extension = ".tmx";
                        else if (embedded[0] == (byte)67 && embedded[1] == (byte)72 && embedded[2] == (byte)78 && embedded[3] == (byte)75)
                            extension = ".amd";
                        else if (embedded[15] == (byte)128 && embedded[16] == (byte)63)
                            extension = ".epl";
                        else if (embedded[0] == (byte)240 && embedded[1] == (byte)0 && embedded[2] == (byte)240 && embedded[3] == (byte)240)
                            extension = ".rmd";
                    }
                    File.WriteAllBytes($@"{path}\ep\{i.ToString(zeros)}\embedded{extension}", embedded);
                }

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

                string[] folders = Directory.GetDirectories(InputFile + @"\ep");
                List<Ep> Eps = new List<Ep>();

                foreach (string folder in folders)
                {

                    Ep NewEp = new Ep();
                    foreach (string file in Directory.GetFiles(folder))
                    {
                        Debug.WriteLine($"Reading: {file}");
                        if (file.ToLower().Contains("epdata"))
                            NewEp.Data = File.ReadAllBytes(file);
                        else if (file.ToLower().Contains("embedded"))
                            NewEp.File = File.ReadAllBytes(file);
                        else if (file.ToLower().Contains("headerdata1"))
                            NewEp.Ep1 = File.ReadAllBytes(file);
                        else if (file.ToLower().Contains("headerdata2"))
                            NewEp.Ep2 = File.ReadAllBytes(file);
                    }
                    if (NewEp.Data == null)
                    {
                        Console.WriteLine($"\n {folder}\\epdata doesn't exist");
                        Exit();
                    }

                    if (NewEp.File == null)
                    {
                        Console.WriteLine($"\n {folder}\\embedded doesn't exist");
                        Exit();
                    }

                    if (NewEp.Ep1 == null)
                    {
                        Console.WriteLine($"\n {folder}\\headerdata1 doesn't exist");
                        Exit();
                    }

                    if (NewEp.Ep2 == null)
                    {
                        Console.WriteLine($"\n {folder}\\headerdata2 doesn't exist");
                        Exit();
                    }

                    NewEp.DataSize = (ulong)NewEp.Data.Length;
                    NewEp.FileSize = (ulong)NewEp.File.Length;
                    Eps.Add(NewEp);
                }


                string[] files = Directory.GetFiles(InputFile + @"\Declarations", "*.dec");
                List<Declaration> Declarations = new List<Declaration>();

                foreach (string file in files)
                    Declarations.Add(new Declaration(file));


                string JsonFromFile;
                using (StreamReader reader = new StreamReader(InputFile + @"\Order.json"))
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

                    Debug.WriteLine($"Size: {size}");
                }

                List<int> UsedValues = new List<int>();

                int index = 0;
                for (int i = 0; i < Declarations.Count; i++)
                {
                    if (UsedValues.Contains(Order[i]) || Order[i] > OffsetList.Count -1)
                    {
                        Debug.WriteLine($"order for {i}: {Order[i]}");
                        Declarations[i].OffsetOrIndex = (uint)Order[i];
                        Declarations[i].UsesIndex = true;
                    }
                    else
                    {
                        Debug.WriteLine($"order for {i}: {Order[i]}");
                        Debug.WriteLine($"but it uses offset: {OffsetList[Order[i]]}");
                        Declarations[i].OffsetOrIndex = OffsetList[Order[i]];
                        Declarations[i].UsesIndex = false;
                        UsedValues.Add(Order[i]);
                        index++;
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
