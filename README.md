# Duey
A minimal read-only implementation of the [NX PKG4.1 format](http://nxformat.github.io/) specification on .NET Standard 2.0

## 🤔 Why?
* Duey works on runtimes targeting or supporting .NET Standard!
* also, it's strictly parsing only. no caches, no weird voodoo magic.

## 🏹 Supported Types
- [x] Int64 (byte, short, int, long)
- [x] Double (float, double)
- [x] String (string)
- [x] Vector (Point)
- [x] Bitmap (NXBitmap)
- [x] Audio (NXAudio)

## ✏️ Usage
to get started, simply create a new NX File object like so.
```csharp
var file = new NXFile("Data.nx");
```
with that, you can do various parsing magic!
```csharp
// store a node object for usage later on!
var node = file.Resolve("Store/Products");

// resolve and defaults to null
var name = node.ResolveOrDefault<string>("name");
var image = node.ResolveOrDefault<NXBitmap>("image");
var soundFx = node.ResolveOrDefault<NXAudio>("soundFx");

// resolve and defaults to a nullable
var stock = node.Resolve<int>("stock") ?? 0; // 0 is the default value!
var price = node.Resolve<double>("price") ?? 0.0;

// resolve a node ..in a node!
var bundles = node.Resolve("Bundled Products");

foreach (var bundle in bundles)
{
    // resolve even more stuff here!
}

// all the previous resolving examples run at O(n)
// if efficiency and speed is an issue..
// this eager loads direct child of the selected node.
node.ResolveAll(n => { // O(n)
    name = n.ResolveOrDefault<string>("name"); // O(1)
    stock = n.Resolve<int>("stock") ?? 0; // O(1)
    price = n.Resolve<double>("price") ?? 0.0; // O(1)
});

// compared to..
name = node.ResolveOrDefault<string>("name"); // O(n)
stock = node.Resolve<int>("stock") ?? 0; // O(n)
price = node.Resolve<double>("price") ?? 0.0; // O(n)
```
parsing bitmaps/images with ImageSharp
```csharp
var bitmap = node.ResolveOrDefault<NXBitmap>("icon");

using (var image = Image.LoadPixelData<Bgra32>(bitmap.Data, bitmap.Width, bitmap.Height))
using (var output = File.Create("icon.png")) {
    // do image manipulation stuff here
    // save the image!
    image.SaveAsPng(output);
}
```
also, remember to dispose~!
```csharp
using (var file = new NXFile("Data.nx")) {
    // do your parsing thing here!
}

// or manually call dipose
var file = new NXFile("Data.nx");
file.Dispose();
```

## ⭐️ Acknowledgements
* [reNX](https://github.com/angelsl/ms-reNX) - main reference for implementations.
* [PKG1](https://labs.crr.io/maplestory/PKG1) - for the inspiration to create this project.
* all the kind souls who designed the NX format.
