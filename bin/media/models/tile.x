xof 0303txt 0032
template ColorRGBA {
 <35ff44e0-6c7c-11cf-8f52-0040333594a3>
 FLOAT red;
 FLOAT green;
 FLOAT blue;
 FLOAT alpha;
}

template ColorRGB {
 <d3e16e81-7835-11cf-8f52-0040333594a3>
 FLOAT red;
 FLOAT green;
 FLOAT blue;
}

template Material {
 <3d82ab4d-62da-11cf-ab39-0020af71e433>
 ColorRGBA faceColor;
 FLOAT power;
 ColorRGB specularColor;
 ColorRGB emissiveColor;
 [...]
}

template TextureFilename {
 <a42790e1-7810-11cf-8f52-0040333594a3>
 STRING filename;
}

template Frame {
 <3d82ab46-62da-11cf-ab39-0020af71e433>
 [...]
}

template Matrix4x4 {
 <f6f23f45-7686-11cf-8f52-0040333594a3>
 array FLOAT matrix[16];
}

template FrameTransformMatrix {
 <f6f23f41-7686-11cf-8f52-0040333594a3>
 Matrix4x4 frameMatrix;
}

template Vector {
 <3d82ab5e-62da-11cf-ab39-0020af71e433>
 FLOAT x;
 FLOAT y;
 FLOAT z;
}

template MeshFace {
 <3d82ab5f-62da-11cf-ab39-0020af71e433>
 DWORD nFaceVertexIndices;
 array DWORD faceVertexIndices[nFaceVertexIndices];
}

template Mesh {
 <3d82ab44-62da-11cf-ab39-0020af71e433>
 DWORD nVertices;
 array Vector vertices[nVertices];
 DWORD nFaces;
 array MeshFace faces[nFaces];
 [...]
}

template MeshNormals {
 <f6f23f43-7686-11cf-8f52-0040333594a3>
 DWORD nNormals;
 array Vector normals[nNormals];
 DWORD nFaceNormals;
 array MeshFace faceNormals[nFaceNormals];
}

template MeshMaterialList {
 <f6f23f42-7686-11cf-8f52-0040333594a3>
 DWORD nMaterials;
 DWORD nFaceIndexes;
 array DWORD faceIndexes[nFaceIndexes];
 [Material <3d82ab4d-62da-11cf-ab39-0020af71e433>]
}


Material PDX01_-_Default {
 1.000000;1.000000;1.000000;1.000000;;
 3.200000;
 0.000000;0.000000;0.000000;;
 0.000000;0.000000;0.000000;;

 TextureFilename {
  "tile.tga";
 }
}

Frame Rectangle01 {
 

 FrameTransformMatrix {
  1.000000,0.000000,0.000000,0.000000,0.000000,1.000000,0.000000,0.000000,0.000000,0.000000,1.000000,0.000000,0.000000,0.000000,0.000000,1.000000;;
 }

 Mesh  {
  20;
  512.00000;0.000000;512.00000;,
  -512.00000;0.000000;512.00000;,
  -512.00000;0.000000;-512.00000;,
  512.00000;0.000000;-512.00000;,
  512.00000;0.000000;-512.00000;,
  -512.00000;0.000000;-512.00000;,
  -512.00000;128.000000;-511.99997;,
  512.00000;128.000000;-511.99997;,
  -512.00000;0.000000;512.00000;,
  -511.99997;128.000000;512.00000;,
  -511.99997;128.000000;-512.00000;,
  -512.00000;0.000000;-512.00000;,
  511.99997;0.000000;512.00000;,
  512.00000;128.000000;512.00000;,
  512.00000;128.000000;-512.00000;,
  511.99997;0.000000;-512.00000;,
  512.00000;128.000000;511.99997;,
  -512.00000;128.000000;511.99997;,
  -512.00000;0.000000;512.00000;,
  512.00000;0.000000;512.00000;;
  10;
  3;2,0,3;,
  3;1,0,2;,
  3;6,4,7;,
  3;5,4,6;,
  3;10,8,11;,
  3;9,8,10;,
  3;14,12,15;,
  3;13,12,14;,
  3;18,16,19;,
  3;17,16,18;;

  MeshNormals  {
   4;
   0.000000;1.000000;0.000000;,
   0.000000;-0.000000;1.000000;,
   1.000000;-0.000000;0.000000;,
   0.000000;-0.000000;-1.000000;;
   10;
   3;0,0,0;,
   3;0,0,0;,
   3;1,1,1;,
   3;1,1,1;,
   3;2,2,2;,
   3;2,2,2;,
   3;2,2,2;,
   3;2,2,2;,
   3;3,3,3;,
   3;3,3,3;;
  }

  MeshMaterialList  {
   1;
   10;
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0,
   0;
   { PDX01_-_Default }
  }
 }
}