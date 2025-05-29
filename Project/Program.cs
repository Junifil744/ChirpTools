using Assimp;
using Assimp.Configs;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Numerics;

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
            // Feel free to complain to me about handling user input myself instead of using a library.
            string exportType = null;
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
            if (args.Length > 1) {
                switch (args[1].Split(".").Last().ToLower()) {
                    case "dae":
                        exportType = "collada";
                        break;
                    case "gltf":
                        exportType = "gltf2";
                        break;
                    case "glb":
                        exportType = "glb2";
                        break;
                    case "obj":
                        exportType = "obj";
                        break;
                    case "stl":
                        exportType = "stl";
                        break;
                    case "jmdl":
                        Console.WriteLine("Sorry, not implemented yet.");
                        Environment.Exit(1);
                        break; // this line is COMPLETELY USELESS but the compiler CRIES without it.
                    default:
                        Console.WriteLine("Unsupported output type.");
                        Console.WriteLine("Fully supported types are the following:\n1 - .dae \n2 - .gltf\n3 - .glb\n");
                        Console.WriteLine("The following types are PARTLY supported:\n4 - .obj (Only mesh and ONE uv map, human readable format)\n5 - .stl (Mesh only)\n6 - .jmdl (Journey model file type, human readable format) (NOT IMPLEMENTED)");
                        Environment.Exit(1);
                        break;
                }
            } else {
                exportType = "collada";
            }
            
            byte[] inputFile = File.ReadAllBytes(args[0]);

            using (var stream = new MemoryStream(inputFile))
            using (var reader = new BinaryReader(stream)) {
                //                                                             //
                //  -- Setting variables to simplify reading the model file -- //
                //                                                             //

                // The start of the data buffer we use, I literally don't know the use of anythin before that in the model file.
                UInt16 dataBufferAddress = (UInt16)(FindFirstPatternIndex(inputFile, new byte[] { 0x0C, 0xB1, 0x01, 0x02, 0x00 }) + 5);
                
                stream.Position = 72;
                UInt32 faceBufferLength = reader.ReadUInt32();
                stream.Position = 76;
                UInt32 vertexBufferLength = reader.ReadUInt32();
                
                int offset = FindLastPatternIndex(inputFile, new byte[] { 0x53, 0x68, 0x61, 0x70, 0x65 });
                while (inputFile[++offset] != 0x00) ; // Skipping to the first 00 byte found
                while (inputFile[++offset] == 0x00) ; // Then skipping to the first non-00 byte from here.
                stream.Position = offset; 
                Byte vertexBufferOffset = reader.ReadByte(); // Then reading the vertexBuffer's per-vertex length from here.
                Byte UVCount = (Byte)((vertexBufferOffset - 20) / 4);

                List<Vector3D> vertex = new List<Vector3D>();
                List<Vector2[]> uvs = new List<Vector2[]>();
                List<UInt16[]> faces = new List<UInt16[]>();

                //                                       //
                //  -- Actually reading the dang file -- //
                //                                       //
                // Processing Vertices & UVs, theyre in the same buffer.
                Console.WriteLine("Moving on to processing vertex & UV");
                for (int i = 0; i < vertexBufferLength; i += vertexBufferOffset) {
                    // Reading the vertices.
                    stream.Position = dataBufferAddress + faceBufferLength + i;
                    float v1 = reader.ReadSingle();
                    float v2 = reader.ReadSingle();
                    float v3 = reader.ReadSingle();
                    stream.Position += 8; // Skipping 8 unknown bytes.

                    // Reading the UVs.
                    Vector2[] UVArray = new Vector2[UVCount];
                    for (int uvIndex = 0; uvIndex < UVCount; uvIndex++) {
                        Half vt1 = reader.ReadHalf();
                        Half vt2 = reader.ReadHalf();
                        UVArray[uvIndex] = new Vector2((float)vt1, (float)vt2);
                    }
                    vertex.Add(new Vector3D(v1, v2, v3));
                    uvs.Add(UVArray);
                }
                // Processing faces
                Console.WriteLine("\nProcessing faces");
                for (int i = 0; i < faceBufferLength; i+=6) {
                    stream.Position = dataBufferAddress + i;
                    UInt16 f1 = reader.ReadUInt16();
                    UInt16 f2 = reader.ReadUInt16();
                    UInt16 f3 = reader.ReadUInt16();

                    faces.Add(new UInt16[] { f1, f2, f3 });
                }

                //                                                                     //
                //  -- Validaing user info. Admittedly, idk why I do it at the end. -- //
                //                                                                     //
                Console.WriteLine("\nChecking file info...");
                string newFilePath = "";
                if(args.Length <= 1) newFilePath = Path.GetDirectoryName(args[0]) + "\\" + Path.GetFileNameWithoutExtension(args[0]) + ".dae";
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

                //                                 //
                //  -- Building the output file -- //
                //                                 //
                var scene = new Scene();

                var mesh = new Mesh("MyMesh", PrimitiveType.Triangle);

                // Add vertices.
                foreach (var vertice in vertex) {
                    mesh.Vertices.Add(vertice);
                }
                scene.Meshes.Add(mesh);

                // Add faces.
                //
                // Because thats fucking required for some reason.
                // I spent weeks researching why my model wouldnt show up
                // IT WAS JUST BECAUSE I WAS TRYNA TEST VERTICES ALONE. RAAAAAAAAH.
                foreach (var faceIndices in faces) {
                    var face = new Face();
                    face.Indices.Add(faceIndices[0]);
                    face.Indices.Add(faceIndices[1]);
                    face.Indices.Add(faceIndices[2]);
                    mesh.Faces.Add(face);
                }

                // Add UVs
                for (int channel = 0; channel < UVCount; channel++) {
                    var uvChannel = mesh.TextureCoordinateChannels[channel];
                    for (int vertexIndex = 0; vertexIndex < uvs.Count; vertexIndex++) {
                        Vector2 uv = uvs[vertexIndex][channel];
                        uvChannel.Add(new Vector3D(uv.X, uv.Y, 0f));
                    }
                }

                // I hate how this is required. I shouldn't HAVE to add a material.
                var material = new Material();
                scene.Materials.Add(material);

                // Export the scene
                scene.RootNode = new Node(Path.GetFileNameWithoutExtension(args[0]));
                scene.RootNode.MeshIndices.Add(0);
                using (var exporter = new AssimpContext()) {
                    exporter.ExportFile(scene, newFilePath, exportType);
                }
                Console.WriteLine("File written!");
                Environment.Exit(0);
            }
        }
    }
}
