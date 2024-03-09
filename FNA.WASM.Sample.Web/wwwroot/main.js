// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

const maxLogs = 500;
const maxErrors = 500;
let logs = [];
let errors = [];
let oldConsoleLog = console.log;
console.log = function(msg) {
    logs.push(msg);
    while (logs.length > maxLogs)
    {
        logs.pop();
    }
    oldConsoleLog(msg);
}
let oldConsoleError = console.error;
console.error = function(e) {
    errors.push(e);
    while (errors.length > maxErrors)
    {
        errors.pop();
    }
    oldConsoleError(e);
}
let oldConsoleWarn = console.warn;
console.warn = function (e) {
    errors.push(e);
    while (errors.length > maxErrors)
    {
        errors.pop();
    }
    oldConsoleWarn(e);
}

import { dotnet } from './_framework/dotnet.js'

//we load the asset manifest early so that we can use it to set the dotnet config
let assetManifest = await globalThis.fetch("asset_manifest.csv");
let assetManifestText = "";
if (!assetManifest.ok)
{
    console.error("Unable to load asset manifest");
    console.error(assetManifest);
}
else {
    assetManifestText = await assetManifest.text();
}
let assetList = assetManifestText.split('\n')
    .filter(i => i)
    .map(i => i.trim().replaceAll('\\', '/'));
console.log(`Found ${assetList.length} assets in manifest`);
console.log(assetList);

const { setModuleImports, getAssemblyExports, getConfig } = await dotnet
    .withModuleConfig({
        onConfigLoaded: (config) => {
            if (!config.resources.vfs) {
                config.resources.vfs = {}
            }

            for (let asset of assetList)
            {
                asset = asset.trim();
                if (asset[0] === '/')
                {
                    asset = asset.substring(1);
                }
                console.log(`Found ${asset}, adding to VFS`);
                config.resources.vfs[asset] = {};
                const assetPath = `../${asset}`;
                config.resources.vfs[asset][assetPath] = null;
            }
        },
    })
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();

//set canvas
var canvas = document.getElementById("canvas");
dotnet.instance.Module.canvas = canvas;

const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);
const onUserInteraction = exports.Program.OnUserInteraction;

canvas.addEventListener("click", (e) => {
    onUserInteraction() 
});
canvas.addEventListener("touchstart",  (e) => {
    onUserInteraction() 
});

setModuleImports('main.js', {
    setMainLoop: (cb) => dotnet.instance.Module.setMainLoop(cb)
});

//logging frontend
let logsEl = document.getElementById("logs");
let errorsEl = document.getElementById("errors");
let logElements = [];
for (let i = 0; i < maxLogs; i++) {
    let div = document.createElement("div");
    logsEl.appendChild(div);
    let p = document.createElement("p");
    div.appendChild(p);
    logElements.push(p);
}
let errElements = [];
for (let i = 0; i < maxErrors; i++) {
    let div = document.createElement("div");
    errorsEl.appendChild(div);
    let p = document.createElement("p");
    div.appendChild(p);
    errElements.push(p);
}
let updateLogs = () => {
    for (let i = 0; i < maxLogs; i++){
        let x = logs.length - 1 - i;
        if (x < 0)
        {
            logElements[i].parentElement.classList.add("empty");
            logElements[i].innerText = "";
        }
        else 
        {
            logElements[i].parentElement.classList.remove("empty");
            logElements[i].innerText = logs[x].toString();
        }
    }
    window.requestAnimationFrame(updateLogs);
}
updateLogs();
let updateErrors = () => {
    for (let i = 0; i < maxErrors; i++){
        let x = errors.length - 1 - i;
        if (x < 0)
        {
            errElements[i].parentElement.classList.add("empty");
            errElements[i].innerText = "";
        }
        else
        {
            errElements[i].parentElement.classList.remove("empty");
            errElements[i].innerText = errors[x].toString();
        }
    }
    window.requestAnimationFrame(updateErrors);
}
updateErrors();

globalThis.dotnetInstance = dotnet.instance;
await dotnet.run();