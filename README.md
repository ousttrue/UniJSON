# UniJSON
JSON serializer and deserializer and schema utilities for Unity(.Net3.5)

## Usage

### Json Create

```cs
var f = new JsonFormatter();
f.BeginMap();
f.Key("X"); f.Value(1);
f.Key("Y"); f.Value(1);
f.Key("Z"); f.Value(1);
f.EndMap();
var json = f.ToString();
// {"X":1,"Y":2,"Z":3}
```

### Json Serialize

```cs
var f = new JsonFormatter();
f.Serialize(new Vector3(1, 2, 3));
var json = f.ToString();
// {"X":1,"Y":2,"Z":3}
```

### Json Parse

```cs
var parsed = json.ParseAsJson();
var x = parsed["X"].GetInt32();
```

### Json Deserialize

```cs
var v = default(Vector3);
json.Deserialize(ref v);
```

### JsonSchema

```cs
[Serializable]
public class glTFSparseIndices
{
    [JsonSchema(Minimum = 0)]
    public int bufferView;

    [JsonSchema(Minimum = 0)]
    public int byteOffset;

    [JsonSchema(EnumSerializationType = EnumSerializationType.AsInt)]
    public glComponentType componentType;

    // empty schemas
    public object extensions;
    public object extras;
}


[Test]
public void AccessorSparseIndices()
{
    // from JSON schema
    var path = Path.GetFullPath(Application.dataPath + "/../glTF/specification/2.0/schema");
    var SchemaDir = new FileSystemAccessor(path);
    var fromSchema = JsonSchema.ParseFromPath(SchemaDir.Get("accessor.sparse.indices.schema.json"));

    // from C# type definition
    var fromClass = JsonSchema.FromType<glTFSparseIndices>();

    Assert.AreEqual(fromSchema, fromClass);
}
```

### MsgPack

Same as json interface

```cs
var f = new MsgPackFormatter();
f.Serialize(new Vector3(1, 2, 3));
var msgpack = f.GetStoreBytes();

var parsed = msgpack.ParseAsMsgPack();
var x = parsed["X"].GetInt32();
```

## Reference
### JSON

* https://www.json.org/

### JSON Schema

* http://json-schema.org/
* https://github.com/KhronosGroup/glTF/tree/master/specification/2.0/schema

### JSON Patch

* http://jsonpatch.com/

