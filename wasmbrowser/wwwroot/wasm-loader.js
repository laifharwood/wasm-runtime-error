// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

import { dotnet } from './_framework/dotnet.js'

const { setModuleImports, getAssemblyExports, getConfig, runMain } = await dotnet
    //.withApplicationArguments("start")
    .create();

const config = getConfig();
const assemblyExports = await getAssemblyExports(config.mainAssemblyName);

// run the C# Main() method and keep the runtime process running and executing further API calls
await runMain();
export { assemblyExports }