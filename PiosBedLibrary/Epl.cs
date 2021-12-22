using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using static PiosBedLibrary.Functions;

namespace PiosBedLibrary
{
    public class Epl
    {
        public List<uint> Order { get; }
        public Header header { get; set; }
        public List<Declaration> Declarations = new List<Declaration>();
        public List<Ep> Eps = new List<Ep>();
        public ulong EplSize
        {
            get
            {
                ulong size = 144;
                size += (ulong)header.DeclarationCount * 192;
                foreach (Ep Ep in Eps)
                    size += Ep.EpSize;
                return size;
            }
        }

        public Epl()
        {

        }
        public Epl(BinaryReader reader)
        {

            header = new Header(reader);
            Declarations = new List<Declaration>();
            for (int i = 0; i < header.DeclarationCount; i++)
                Declarations.Add(new Declaration(reader));

            Eps = new List<Ep>();

            Order = new List<uint>();
            uint index = 0;
            for (int i =0; i < header.DeclarationCount; i++)
            {
                if (!Declarations[i].UsesIndex)
                {
                    Eps.Add(new Ep(reader));
                    Order.Add(index);
                    index++;
                }
                else
                    Order.Add(Declarations[i].OffsetOrIndex);
            }
                
                    
        }
        public Epl(string path)
        {

            BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open));


            header = new Header(reader);

            Declarations = new List<Declaration>();
            for (int i = 0; i < header.DeclarationCount; i++)
                Declarations.Add(new Declaration(reader));

            Order = new List<uint>();

            uint index = 0;
            for (int i = 0; i < header.DeclarationCount; i++)
            {
                if (!Declarations[i].UsesIndex)
                {
                    Debug.WriteLine($"Ep: {index} at {reader.BaseStream.Position}");
                    Eps.Add(new Ep(reader));
                    Order.Add(index);
                    index++;
                }
                else
                    Order.Add(Declarations[i].OffsetOrIndex);
            }
        }
        public void Save(string path)
        {
            using (BinaryWriter writer = new BinaryWriter(System.IO.File.Open(path, FileMode.Create)))
            {
                header.Save(writer);
                foreach (Declaration dec in Declarations)
                    dec.Save(writer);
                foreach (Ep Ep in Eps)
                    Ep.Save(writer);
            }

        }
        public void Save(BinaryWriter writer)
        {
            header.Save(writer);
            foreach (Declaration dec in Declarations)
                dec.Save(writer);
            foreach (Ep Ep in Eps)
                Ep.Save(writer);
        }
    }
    public class Ep
    {
        public ulong EpSize
        {
            get
            {
                ulong size = 44;
                size += DataSize;
                size += CalculatePadding(size);
                size += FileSize;
                size += CalculatePadding(size);

                return size;
            }
        }
        public byte[] Ep1 { get; set; }
        public UInt64 DataSize { get; set; }
        public byte[] Ep2 { get; set; }
        public UInt64 FileSize { get; set; }
        public byte[] Data { get; set; }
        public byte[] File { get; set; }
        public Ep()
        {

        }
        public Ep(BinaryReader reader)
        {
            long Offset = reader.BaseStream.Position;

            Ep1 = reader.ReadBytes(20);
            DataSize = reader.ReadUInt64();
            Ep2 = reader.ReadBytes(8);
            FileSize = reader.ReadUInt64();
            Data = reader.ReadBytes((int)DataSize);
            Debug.WriteLine($"DataSize: {DataSize}");
            int h = (int)CalculatePadding(reader.BaseStream.Position - Offset);
            reader.ReadBytes(h);

            File = reader.ReadBytes((int)FileSize);
            h = (int)CalculatePadding(reader.BaseStream.Position - Offset);
            reader.ReadBytes(h);
            Debug.WriteLine($"padding: {h}");
        }
        public Ep(string path)
        {
            BinaryReader reader = new BinaryReader(System.IO.File.Open(path, FileMode.Open));
            long Offset = reader.BaseStream.Position;

            Ep1 = reader.ReadBytes(20);
            DataSize = reader.ReadUInt64();
            Ep2 = reader.ReadBytes(8);
            FileSize = reader.ReadUInt64();
            Data = reader.ReadBytes((int)DataSize);
            reader.ReadBytes((int)CalculatePadding(reader.BaseStream.Position - Offset));

            File = reader.ReadBytes((int)FileSize);
            reader.ReadBytes((int)CalculatePadding(reader.BaseStream.Position - Offset));


        }
        public void Save(string path)
        {
            using (BinaryWriter writer = new BinaryWriter(System.IO.File.Open(path, FileMode.Create)))
            {
                writer.Write(Ep1);
                writer.Write(DataSize);
                writer.Write(Ep2);
                writer.Write(FileSize);
                writer.Write(Data);

                long h = CalculatePadding(writer.BaseStream.Position);
                for (long i = 0; i < h; i++)
                    writer.Write((byte)0);

                writer.Write(File);
                h = CalculatePadding(writer.BaseStream.Position);
                for (long i = 0; i < h; i++)
                    writer.Write((byte)0);
            }

        }
        public void Save(BinaryWriter writer)
        {
            long offset = writer.BaseStream.Position;
            writer.Write(Ep1);
            writer.Write(DataSize);
            writer.Write(Ep2);
            writer.Write(FileSize);
            writer.Write(Data);
            long h = CalculatePadding(writer.BaseStream.Position - offset);
            for (long i = 0; i < h; i++)
                writer.Write((byte)0);

            writer.Write(File);

            h = CalculatePadding(writer.BaseStream.Position - offset);
            for (long i = 0; i < h; i++)
                writer.Write((byte)0);

        }
    }
    public class Header
    {
        public byte[] Header1 {get; set;}
        public int DeclarationCount { get; set; }
        public byte[] Header2 { get; set; }
        public ulong HeaderSize
        {
            get { return 144; }
        }

        public Header()
        {

        }
        public Header(BinaryReader reader)
        {
            Header1 = reader.ReadBytes(128);
            DeclarationCount = reader.ReadInt32();
            Header2 = reader.ReadBytes(12);
        }
        public Header(string path)
        {
            BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open));
            Header1 = reader.ReadBytes(128);
            DeclarationCount = reader.ReadInt32();
            Header2 = reader.ReadBytes(12);
        }
        public void Save(string path)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                writer.Write(Header1);
                writer.Write(DeclarationCount);
                writer.Write(Header2);
            }

        }
        public void Save(BinaryWriter writer)
        {
            writer.Write(Header1);
            writer.Write(DeclarationCount);
            writer.Write(Header2);

        }
    }
    public class Declaration
    {

        public byte[] Declaration1 { get; set; }
        public uint OffsetOrIndex { get; set; }
        public bool UsesIndex { get; set; }
        public byte[] Declaration2 { get; set; }
        public char[] Type { get; set; }
        public byte[] Declaration3 { get; set; }

        public ulong DeclarationSize
        {
            get { return 192; }
        }
        public Declaration()
        {

        }
        public Declaration(BinaryReader reader, int DecNum = 1)
        {
            Declaration1 = reader.ReadBytes(144);
            OffsetOrIndex = reader.ReadUInt32();
            if (OffsetOrIndex < 144 + (192 * DecNum))
                UsesIndex = true;
            Declaration2 = reader.ReadBytes(8);
            byte[] TypeInBytes = reader.ReadBytes(8);//Gives a weird error if I use ReadChars
            var TypeList = new List<char>();
            foreach (byte Byte in TypeInBytes)
                TypeList.Add(Convert.ToChar(Byte));
            Type = TypeList.ToArray();
            Declaration3 = reader.ReadBytes(28);
        }
        public Declaration(string path)
        {
            BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open));
            Declaration1 = reader.ReadBytes(144);
            OffsetOrIndex = reader.ReadUInt32();
            Declaration2 = reader.ReadBytes(8);
            byte[] TypeInBytes = reader.ReadBytes(8);//Gives a weird error if I use ReadChars
            var TypeList = new List<char>();
            foreach (byte Byte in TypeInBytes)
                TypeList.Add(Convert.ToChar(Byte));
            Type = TypeList.ToArray();
            Declaration3 = reader.ReadBytes(28);
        }
        public void Save(string path)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                writer.Write(Declaration1);
                writer.Write(OffsetOrIndex);
                writer.Write(Declaration2);
                writer.Write(Type);
                writer.Write(Declaration3);
            }

        }
        public void Save(BinaryWriter writer)
        {
            writer.Write(Declaration1);
            writer.Write(OffsetOrIndex);
            writer.Write(Declaration2);
            writer.Write(Type);
            writer.Write(Declaration3);


        }
    }
}
