# Local .NET SDK Setup

The execution environment for this kata does not ship with the .NET SDK and outbound internet access is blocked, so `dotnet`
commands will fail until you provide the SDK yourself. Follow the steps below to stage the SDK inside the repository.

## 1. Download the installer script (outside the container if necessary)

1. From a machine with internet access, download the official installer script:
   ```bash
   curl -o dotnet-install.sh https://dot.net/v1/dotnet-install.sh
   ```
2. Commit or copy the script into this repository at `scripts/dotnet-install.sh`.

If you *do* have internet access where the tests run, the helper script in this repo can fetch `dotnet-install.sh` for you.

## 2. Run the bootstrap script

Execute the helper script which wraps the official installer and places the SDK under `./.dotnet`:

```bash
scripts/setup-dotnet.sh
```

The script will download the installer (when possible) and install the latest .NET 8 SDK into `./.dotnet`.

### Offline/air‑gapped environments
If the download step fails because the environment cannot reach `https://dot.net`, manually copy `dotnet-install.sh` (and, if
needed, the archived SDK payloads) into the repository before running `scripts/setup-dotnet.sh`. The script will reuse the local
copy and skip the network call.

## 3. Export environment variables for the new SDK

For each shell session where you want to run `dotnet` commands, add the local SDK directory to your `PATH`:

```bash
export DOTNET_ROOT="$PWD/.dotnet"
export PATH="$DOTNET_ROOT:$PATH"
```

To run the test suite in one command without modifying your shell configuration, you can wrap the exports inline:

```bash
DOTNET_ROOT="$PWD/.dotnet" PATH="$DOTNET_ROOT:$PATH" dotnet test
```

Once these steps are complete, the Playwright and NUnit tests in this repository can be executed normally.
