# ChirpTools
Journey 3D models decompilation tool<br>
This tool takes Journey's D3D11x64 model files and converts them into .obj files.

## How to use:
1. Download the latest executable from the Releases tab<br>
2. Run it in the command line. "ChirpTools.exe [input file] [output file]"<br>
3. If output file is not specified, since it isnt required, it will simply output a file with the same name as input file at the same location as the input file, except with the .obj format.
## This project is NOT finished yet!
* I have yet to test every model, I've only tested 4.<br>
* The project uses OBJ files, OBJ files are really easy to create but they do not support much, they can't support multiple UVs or vertex color.<br>
* I will change the output format from OBJ to glTF.<br>
* I may add support for the armatures and bone structures.<br>
* There are still PLENTY of things I do not know about this game's model's file format yet.<br>
