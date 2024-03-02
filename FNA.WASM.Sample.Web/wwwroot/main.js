// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

const { setModuleImports, getAssemblyExports, getConfig } = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();

setModuleImports('main.js', {
    setMainLoop: (cb) => dotnet.instance.Module.setMainLoop(cb)
});

//logging frontend
const maxLogs = 500;
const maxErrors = 500;
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

//set canvas
var canvas = document.getElementById("canvas");
dotnet.instance.Module.canvas = canvas;
await dotnet.run();