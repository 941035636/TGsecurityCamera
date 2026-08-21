# Third-party Unity dependencies

This repository intentionally excludes the following commercial Unity assets. Their licenses do not permit publishing the purchased source code or precompiled binaries in a public repository.

- AVPro Video, scripts version 1.8.9 (`Assets/AVProVideo`)
- Vuplex 3D WebView for Windows and macOS (`Assets/InWeb`)
- Universal Media Player / UMP Pro (`Assets/UniversalMediaPlayer`)

To open and build the original Unity 2018.4.0f1 project, obtain valid licenses for these packages and restore them to the paths shown above. The local development checkout used to prepare this repository still retains its licensed copies; the directories are ignored only by Git.

Site-specific deployment configuration is also excluded. Copy the corresponding `*.example.json` files in `Assets/StreamingAssets/Configurations` to their non-example names, then supply the deployment's own endpoints and credentials.
