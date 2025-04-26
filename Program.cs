using System.Diagnostics;
using System.Text;

namespace ChirpTools
{
    internal class Program
    {
        static UInt16 FindFirstPatternIndex(byte[] data, byte[] pattern) {
            for (UInt16 i = 0; i <= data.Length - pattern.Length; i++) {
                bool found = true;
                for (int j = 0; j < pattern.Length; j++) {
                    if (data[i+j] != pattern[j]) {
                        found = false;
                        break;
                    }
                }
                if (found) return i;   
            }
            throw new InvalidDataException("Pattern was not found within the data.");
        }
        static UInt16 FindLastPatternIndex(byte[] data, byte[] pattern) {
            for (int i = data.Length - pattern.Length; i >= 0; i--) {
                bool found = true;
                for (int j = 0; j < pattern.Length; j++) {
                    if (data[i + j] != pattern[j]) {
                        found = false;
                        break;
                    }
                }
                if (found) return (UInt16)i;
            }
            throw new InvalidDataException("Pattern was not found within the data.");
        }

        // I need more comments. I know. Hush hush.

        static void Main(string[] args)
        {
            if (Environment.UserInteractive && !string.IsNullOrEmpty(Process.GetCurrentProcess().MainWindowTitle)) {
                Console.WriteLine("This is a command line tool!!");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(1);
            }  else if (args.Length == 0) {
                Console.WriteLine("No input file was specified.");
                Environment.Exit(1);
            } else if (args[0].Split(".").Last() != "D3D11x64") {
                Console.WriteLine("Input file is not D3D11x64 file format.");
                Environment.Exit(1);
            } else if (!File.Exists(args[0])) {
                Console.WriteLine("Input file was not found.");
                Environment.Exit(1);
            }

            byte[] inputFile = File.ReadAllBytes(args[0]);

            using (var stream = new MemoryStream(inputFile))
            using (var reader = new BinaryReader(stream)) {
                // Setting things right
                UInt16 dataBufferAddress = (UInt16)(FindFirstPatternIndex(inputFile, new byte[] { 0x0C, 0xB1, 0x01, 0x02, 0x00 }) + 5);
                
                stream.Position = 72;
                UInt32 faceBufferLength = reader.ReadUInt32();
                stream.Position = 76;
                UInt32 vertexBufferLength = reader.ReadUInt32();
                
                int offset = FindLastPatternIndex(inputFile, new byte[] { 0x53, 0x68, 0x61, 0x70, 0x65 });

                while (inputFile[++offset] != 0x00) ;
                while (inputFile[++offset] == 0x00) ;

                stream.Position = offset;
                Console.WriteLine(stream.Position.ToString("X"));
                Byte vertexBufferOffset = reader.ReadByte();

                string outputFaces = "\n# Faces " + Convert.ToString(faceBufferLength / 6);
                string outputVertex = "# Vertices " + Convert.ToString(vertexBufferLength / vertexBufferOffset);
                string outputUV = "\n# UVs " + Convert.ToString(vertexBufferLength / vertexBufferOffset);


                // Processing faces (they come first in the file)
                Console.WriteLine("\nProcessing faces");
                for (int i = 0; i < faceBufferLength; i+=6) {
                    stream.Position = dataBufferAddress + i;
                    UInt16 f1 = reader.ReadUInt16();
                    UInt16 f2 = reader.ReadUInt16();
                    UInt16 f3 = reader.ReadUInt16();
                    
                    outputFaces += $"\nf  {f1 + 1}/{f1 + 1} {f2 + 1}/{f2 + 1} {f3 + 1}/{f3 + 1}";
                }

                // Processing Vertices & UVs, theyre in the same buffer.
                Console.WriteLine("Moving on to processing vertex & UV");
                for (int i = 0; i < vertexBufferLength; i += vertexBufferOffset) {
                    stream.Position = dataBufferAddress + faceBufferLength + i;
                    float v1 = reader.ReadSingle();
                    float v2 = reader.ReadSingle();
                    float v3 = reader.ReadSingle();
                    stream.Position += 8;
                    Half vt1 = reader.ReadHalf();
                    Half vt2 = reader.ReadHalf();

                    outputVertex += $"\nv  {v1} {v2} {v3}";
                    outputUV += $"\nvt  {vt1} {vt2}";
                }

                Console.WriteLine("Outputting file!");
                string newFilePath = "";
                if(args.Length <= 1) newFilePath = Path.GetDirectoryName(args[0]) + "\\" + Path.GetFileNameWithoutExtension(args[0]) + ".obj";
                else newFilePath = args[1];

                if (File.Exists(newFilePath)) {
                    Console.WriteLine($"The file \"{newFilePath}\" already exists, do you want to overwrite it? (y/n, Defaults to n)");
                    ConsoleKey responseKey = Console.ReadKey(true).Key;
                    if (responseKey != ConsoleKey.Y) {
                        Console.WriteLine("Aborting.");
                        Environment.Exit(0);
                    } else {
                        Console.WriteLine("Overwriting...");
                        File.Delete(newFilePath);
                    }
                }

                using (FileStream fs = File.Create(newFilePath)) {
                    byte[] va = new UTF8Encoding(true).GetBytes(outputVertex + "\n");
                    fs.Write(va, 0, va.Length);
                    byte[] vta = new UTF8Encoding(true).GetBytes(outputUV + "\n");
                    fs.Write(vta, 0, vta.Length);
                    byte[] fa = new UTF8Encoding(true).GetBytes(outputFaces);
                    fs.Write(fa, 0, fa.Length);
                }
                Console.WriteLine("File written!");
                Environment.Exit(0);
            }
        }
    }
}
