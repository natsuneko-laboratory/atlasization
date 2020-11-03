# Atlasization - Unity Editor Extension

The editor extension for Unity that allows you to create an atlas texture/material/mesh in Unity.

## Requirements

- Unity 2018.4.20f1

## Installation

1. Download UnityPackage from BOOTH (Recommended)
2. Install via NPM Scoped Registry

### Download UnityPackage

You can download latest version of UnityPackage from BOOTH (Not Yet Provided).  
Extract downloaded zip package and install UnityPackage into your project.

### Install via NPM

Please add the following section to the top of the package manifest file (`Packages/manifest.json`).  
If the package manifest file already has a `scopedRegistries` section, it will be added there.

```json
{
  "scopedRegistries": [
    {
      "name": "Mochizuki",
      "url": "https://registry.npmjs.com",
      "scopes": ["moe.mochizuki"]
    }
  ]
}
```

And the following line to the `dependencies` section:

```json
"moe.mochizuki.atlasization": "VERSION"
```

## How to use (Documentation / Japanese)

https://docs.mochizuki.moe/Unity/Atlasization/

## License

MIT by [@6jz](https://twitter.com/6jz)
