using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using static PiosBedLibrary.Functions;

namespace PiosBedLibrary
{
    public class Effect_Epl
    {
        public ushort Id { get; set; }
        public ushort Id2 { get; set; }

        public Epl EmbededEpl { get; set; }

        public ulong EplSize
        {
            get{return EmbededEpl.EplSize;}
        }

        public ulong Effect_eplSize
        {
            get
            {
                ulong size = 20;
                size += EplSize;
                size += CalculatePadding(size);
                return size;
            }
        }
        public Effect_Epl()
        {

        }
        public Effect_Epl(BinaryReader reader)
        {
            long Offset = reader.BaseStream.Position;

            Id = reader.ReadUInt16();
            Id2 = reader.ReadUInt16();
            int Eplsize = reader.ReadInt32();
            reader.ReadBytes(12);
            byte[] eplbytes = reader.ReadBytes(Eplsize);
            reader.ReadBytes((int)CalculatePadding(reader.BaseStream.Position - Offset));
            using (BinaryReader Eplreader = new BinaryReader(new MemoryStream(eplbytes)))
                EmbededEpl = new Epl(Eplreader);
        }
        public Effect_Epl(string path)
        {
            BinaryReader reader = new BinaryReader(System.IO.File.Open(path, FileMode.Open));

            Id = reader.ReadUInt16();
            Id2 = reader.ReadUInt16();
            int Eplsize = reader.ReadInt32();
            reader.ReadBytes(12);
            byte[] eplbytes = reader.ReadBytes(Eplsize);
            reader.ReadBytes((int)CalculatePadding(reader.BaseStream.Position));
            using (BinaryReader Eplreader = new BinaryReader(new MemoryStream(eplbytes)))
                EmbededEpl = new Epl(Eplreader);
        }
        public void Save(string path)
        {
            using (BinaryWriter writer = new BinaryWriter(System.IO.File.Open(path, FileMode.Create)))
            {
                writer.Write(Id);
                writer.Write(Id2);
                writer.Write((uint)EmbededEpl.EplSize);
                for (int i = 0; i < 12; i++)
                    writer.Write((byte)0);

                EmbededEpl.Save(writer);

                long h = CalculatePadding(writer.BaseStream.Position);// have to declare it as a variable or it doesn't work for some reason
                for (long i = 0; i < h; i++)
                    writer.Write((byte)0);
                    
            }

        }
        public void Save(BinaryWriter writer)
        {
            long Offset = writer.BaseStream.Position;
            writer.Write(Id);
            writer.Write(Id2);
            writer.Write((uint)EmbededEpl.EplSize);
            for (int i = 0; i < 12; i++)
                writer.Write((byte)0);

            EmbededEpl.Save(writer);

            long h = CalculatePadding(writer.BaseStream.Position);
            for (int i = 0; i < h; i++)
                writer.Write((byte)0);
        }
    }
}
