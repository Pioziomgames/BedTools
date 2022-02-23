using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using static PiosBedLibrary.Functions;

namespace PiosBedLibrary
{
    public class Epl
    {
        public List<uint> Order { get; set; }
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
            Declarations = new List<Declaration>();
            Order = new List<uint>();
        }
        public Epl(byte[] Bytes)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(Bytes)))
                ReadData(reader);
        }
        public Epl(BinaryReader reader)
        {
            ReadData(reader);  
        }
        public Epl(string path)
        {

            using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
                ReadData(reader);
        }
        public void Save(string path)
        {
            using (BinaryWriter writer = new BinaryWriter(System.IO.File.Open(path, FileMode.Create)))
                WriteData(writer);

        }
        public void Save(BinaryWriter writer)
        {
            WriteData(writer);
        }

        private void ReadData(BinaryReader reader)
        {
            header = new Header(reader);
            Declarations = new List<Declaration>();
            for (int i = 0; i < header.DeclarationCount; i++)
                Declarations.Add(new Declaration(reader));

            Eps = new List<Ep>();

            Order = new List<uint>();
            uint index = 0;
            for (int i = 0; i < header.DeclarationCount; i++)
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
        private void WriteData(BinaryWriter writer)
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
                if (CalculatePadding(size) > 100) // sometimes gives a weird result but only when using a ulong
                    size += (ulong)CalculatePadding((int)size);
                else
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
        public Ep(byte[] Bytes)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(Bytes)))
                ReadData(reader);
        }
        public Ep(BinaryReader reader)
        {
            ReadData(reader);
        }
        public Ep(string path)
        {
            using (BinaryReader reader = new BinaryReader(System.IO.File.Open(path, FileMode.Open)))
                ReadData(reader);


        }
        public void Save(string path)
        {
            using (BinaryWriter writer = new BinaryWriter(System.IO.File.Open(path, FileMode.Create)))
                WriteData(writer);

        }
        public void Save(BinaryWriter writer)
        {
            WriteData(writer);

        }

        private void ReadData(BinaryReader reader)
        {
            long Offset = reader.BaseStream.Position;

            Ep1 = reader.ReadBytes(20);
            DataSize = reader.ReadUInt64();
            Ep2 = reader.ReadBytes(8);
            FileSize = reader.ReadUInt64();
            Data = reader.ReadBytes((int)DataSize);
            int h = (int)CalculatePadding(reader.BaseStream.Position - Offset);
            reader.ReadBytes(h);

            File = reader.ReadBytes((int)FileSize);
            h = (int)CalculatePadding(reader.BaseStream.Position - Offset);
            reader.ReadBytes(h);
        }

        private void WriteData(BinaryWriter writer)
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
            ReadData(reader);
        }
        public Header(byte[] Bytes)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(Bytes)))
                ReadData(reader);
        }
        public Header(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
                ReadData(reader);
        }
        public void Save(string path)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
                WriteData(writer);

        }
        public void Save(BinaryWriter writer)
        {
            WriteData(writer);
        }

        private void ReadData(BinaryReader reader)
        {
            Header1 = reader.ReadBytes(128);
            DeclarationCount = reader.ReadInt32();
            Header2 = reader.ReadBytes(12);
        }

        private void WriteData(BinaryWriter writer)
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
        public Declaration(byte[] Bytes, int DecNum = 1)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(Bytes)))
                ReadData(reader, DecNum);
        }
        public Declaration(BinaryReader reader, int DecNum = 1)
        {
            ReadData(reader,DecNum);
        }
        public Declaration(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
                ReadData(reader);
        }
        public void Save(string path)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
                WriteData(writer);

        }
        public void Save(BinaryWriter writer)
        {
            WriteData(writer);
        }

        private void ReadData(BinaryReader reader, int DecNum = 1)
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
        private void WriteData(BinaryWriter writer)
        {
            writer.Write(Declaration1);
            writer.Write(OffsetOrIndex);
            writer.Write(Declaration2);
            writer.Write(Type);
            writer.Write(Declaration3);
        }
    }
}
