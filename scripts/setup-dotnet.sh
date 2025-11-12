#!/usr/bin/env bash
set -euo pipefail

CHANNEL="8.0"
INSTALL_DIR="${INSTALL_DIR:-$(pwd)/.dotnet}"
SCRIPT_PATH="${SCRIPT_PATH:-$(pwd)/scripts/dotnet-install.sh}"

log() {
  echo "[setup-dotnet] $*"
}

ensure_prereqs() {
  if command -v curl >/dev/null 2>&1; then
    DOWNLOADER=(curl -sSL)
  elif command -v wget >/dev/null 2>&1; then
    DOWNLOADER=(wget -qO-)
  else
    log "Neither curl nor wget is available. Please download dotnet-install.sh manually from https://dot.net/v1/dotnet-install.sh and place it at $SCRIPT_PATH."
    return 1
  fi

  if [ ! -f "$SCRIPT_PATH" ]; then
    log "Downloading dotnet-install.sh to $SCRIPT_PATH ..."
    mkdir -p "$(dirname "$SCRIPT_PATH")"
    "${DOWNLOADER[@]}" https://dot.net/v1/dotnet-install.sh >"$SCRIPT_PATH"
    chmod +x "$SCRIPT_PATH"
  else
    log "Reusing existing dotnet-install.sh at $SCRIPT_PATH."
  fi

  return 0
}

run_install() {
  log "Installing .NET SDK channel $CHANNEL into $INSTALL_DIR ..."
  mkdir -p "$INSTALL_DIR"
  "$SCRIPT_PATH" --channel "$CHANNEL" --install-dir "$INSTALL_DIR"
}

print_exports() {
  cat <<EOM

[setup-dotnet] Installation complete. Add the following to your shell to use this SDK:
  export DOTNET_ROOT="$INSTALL_DIR"
  export PATH="$INSTALL_DIR:\$PATH"

You can then run tests with:
  DOTNET_ROOT="$INSTALL_DIR" PATH="$INSTALL_DIR:\$PATH" dotnet test
EOM
}

main() {
  if ! ensure_prereqs; then
    exit 1
  fi

  run_install
  print_exports
}

main "$@"
