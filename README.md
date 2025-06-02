# ChirpTools
Journey 3D models decompilation tool<br>
This tool takes Journey's D3D11x64 model files and converts them into .obj files.

## How to use:
1. Download the latest executable from the Releases tab<br>
2. Run it in the command line. "ChirpTools.exe [input file] [output file]"<br>
3. If output file is not specified, since it isn't required, a file of the same name with the collada (.dae) extension will be used.
4. Supported file types are the following:
   - .dae
   - .gltf
5. The following file types are supported but might lack information.
   - .obj
   - .stl
   - .jmdl (NOT IMPLEMENTED YET)
  
## This project is NOT finished yet!
- I have yet to test every model, I've only tested 4.<br>
- Vertex color isn't supported yet.<br>
- There are still PLENTY of things I do not know about this game's model's file format yet.<br>
- (Optional) I may add support for the armatures and bone structures.<br>
- (Optional) If I do add support for armatures and bones, maybe I'll go for animations but I don't know.<br>
- (Optional) Linux support. I know, I know, disappointment.<br>

## Some things I will just NOT make.
- Shaders. My goal is decompiling the models, shader files aren't the same file as the model.
