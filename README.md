# Duey
A minimal read-only implementation of the PKG1.0 and [NX PKG4.1 format](http://nxformat.github.io/) file specification on .NET Standard 2.1

## 🤔 Why?
* Duey works on runtimes targeting or supporting .NET Standard!
* also, it's strictly parsing only. no eager caches, no weird voodoo magic.

## 🏹 Supported Types
| Type                           | PKG1.0 (WZ)        | PKG4.1 (NX)        |
|--------------------------------|--------------------|--------------------|
| Int64 (byte, short, int, long) | :heavy_check_mark: | :heavy_check_mark: |
| Double (float, double)         | :heavy_check_mark: | :heavy_check_mark: |
| String (string)                | :heavy_check_mark: | :heavy_check_mark: |
| Vector (DataVector)            | :heavy_check_mark: | :heavy_check_mark: |
| Bitmap (DataBitmap)            |                    | :heavy_check_mark: |
| Audio (DataAudio)              |                    | :heavy_check_mark: |

## ✏️ Usage (PKG4.1)
to get started, simply create a new NX Package object like so.
```csharp
var pkg = new NXPackage("Data.nx");
```
with that, you can do various parsing magic!
```csharp
// store a node object for usage later on!
var node = pkg.ResolvePath("Store/Products");

// resolve to a nullable
var name = node.ResolveString("name");
var image = node.ResolveBitmap("image");
var soundFx = node.ResolveAudio("soundFx");

// resolve a node ..in a node!
var bundles = node.ResolvePath("Bundled Products");

foreach (var bundle in bundles)
{
    // resolve even more stuff here!
}

// all the previous resolving examples run at O(n)
// if efficiency and speed is an issue..
// this eager loads direct child of the selected node.
var resolution = node.Cache(); // O(n)

name = resolution.ResolveString("name"); // O(1)
stock = resolution.ResolveInt("stock") ?? 0; // O(1)
price = resolution.ResolveDouble("price") ?? 0.0; // O(1)

// compared to..
name = node.ResolveString("name"); // O(n)
stock = node.ResolveInt("stock") ?? 0; // O(n)
price = node.ResolveDouble("price") ?? 0.0; // O(n)
```
parsing bitmaps/images with ImageSharp
```csharp
var bitmap = node.ResolveBitmap("icon");

using (var image = Image.LoadPixelData<Bgra32>(bitmap.Data, bitmap.Width, bitmap.Height))
using (var output = File.Create("icon.png")) {
    // do image manipulation stuff here
    // save the image!
    image.SaveAsPng(output);
}
```

## 📖 Usage (PKG1.0)
to get started, simply create a new WZ Package object like so. Do note that a key is required to decode properly.
```csharp
var pkg = new WZPackage("Data.wz", "95");
```

Alternate filesystem and .img file loading
```csharp
var pkg = new FSDirectory("./Data/");
```

All resolve methods are synchronous with the PKG4.1 examples.

## ⭐️ Acknowledgements
* [reNX](https://github.com/angelsl/ms-reNX) - main reference for implementations.
* [PKG1](https://labs.crr.io/maplestory/PKG1) - for the inspiration to create this project.
* all the kind souls who designed the NX format.
