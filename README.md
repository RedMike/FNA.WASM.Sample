# FNA.WASM.Sample

This is a sample showing FNA running in the browser with the latest .NET SDK (currently 8)
with WebAssembly.

This project relies on 
[Emscripten-built FNAlibs automatically built and available here](https://github.com/RedMike/FNA-WASM-Build/releases),
and on having installed the `wasm-tools` and `wasm-experimental` workloads.

The `main` branch publishes its built WASM binary to [Github Pages here](https://redmike.github.io/FNA.WASM.Sample).

![Github Pages action status](https://github.com/RedMike/FNA.WASM.Sample/actions/workflows/deploy.yml/badge.svg)

[Information on how to set up a project like this is available on the wiki here.](https://github.com/RedMike/FNA.WASM.Sample/wiki/Manually-setting-up-FNA-Project-for-WASM)