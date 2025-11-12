# Local .NET SDK Setup

In this execution environment, the .NET SDK is not preinstalled and the container does not allow downloading installers from the public internet through the default proxy. As a result, `dotnet` commands (such as `dotnet test`) fail with `command not found` until the SDK is provided manually.

To make the SDK available inside the repository, download the official `dotnet-install.sh` script and the matching SDK payloads ahead of time and commit them to the repository (or provide them through an artifact in the exercise environment). Once the script is available locally, you can install the SDK into a temporary directory (for example, `./.dotnet`) with:

```bash
./dotnet-install.sh --channel 8.0 --install-dir ./.dotnet
```

After installation, expose the new SDK to the build scripts by adding the install directory to the `PATH` for each shell session where tests should be run:

```bash
export DOTNET_ROOT="$PWD/.dotnet"
export PATH="$DOTNET_ROOT:$PATH"
```

The repository's test suite can then be executed with:

```bash
DOTNET_ROOT="$PWD/.dotnet" PATH="$DOTNET_ROOT:$PATH" dotnet test
```

If the environment blocks outbound network traffic (as in this workspace), download the necessary script and SDK archives outside the container and copy them into the repository before running the commands above.
