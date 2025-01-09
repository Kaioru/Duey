using Duey.Abstractions;
using Duey.Provider.NX;
using Duey.Provider.WZ;

var pkg = new WZPackage("/Users/keith/Workspace/Shared/MapleStory/UI.wz", "95");
var node = pkg.ResolveBitmap("Logo.img/Nexon/2");
var bitmap = node!.Value;

Console.WriteLine(bitmap);

var nx = new NXPackage("/Users/keith/Workspace/Shared/MapleStory/UI.nx");
var nn = nx.ResolveBitmap("Logo.img/Nexon/2");

Console.WriteLine(nn.Value);